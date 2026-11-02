using System.Text;
using System.Text.RegularExpressions;
using BCrypt.Net;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed class CustomerAccountService(ICustomerUserRepository users)
{
    private const int MaxAvatarBytes = 5 * 1024 * 1024;

    public Task<CustomerProfile?> GetProfileAsync(int userId, CancellationToken ct = default)
        => users.GetProfileAsync(userId, ct);

    public async Task<(bool Success, string Message)> UpdateAvatarAsync(int userId, Stream upload, long declaredLength,
        string webRootPath, CancellationToken ct = default)
    {
        if (userId <= 0) return (false, "Please log in again.");
        if (declaredLength <= 0) return (false, "Please choose an image.");
        if (declaredLength > MaxAvatarBytes) return (false, "Avatar must be 5MB or smaller.");

        using var content = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await upload.ReadAsync(buffer, ct)) > 0)
        {
            if (content.Length + read > MaxAvatarBytes) return (false, "Avatar must be 5MB or smaller.");
            await content.WriteAsync(buffer.AsMemory(0, read), ct);
        }
        var bytes = content.ToArray();
        var extension = DetectImageExtension(bytes);
        if (extension is null) return (false, "Only valid JPG, PNG, GIF, and WEBP images are allowed.");

        var folder = Path.Combine(webRootPath, "assets", "img", "avatars");
        var fileName = Guid.NewGuid().ToString("N") + extension;
        var destination = Path.Combine(folder, fileName);
        var saved = false;
        try
        {
            Directory.CreateDirectory(folder);
            await using (var file = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                81920, FileOptions.Asynchronous))
            {
                saved = true;
                await file.WriteAsync(bytes, ct);
            }
            var path = "assets/img/avatars/" + fileName;
            if (!await users.UpdateAvatarAsync(userId, path, ct))
                return (false, "Unable to update your avatar. Please log in again.");
            saved = false; // The file is now referenced by this customer's profile.
            return (true, "Profile picture updated successfully.");
        }
        finally
        {
            if (saved)
            {
                try { File.Delete(destination); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
    }

    private static string? DetectImageExtension(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 45 && data[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) &&
            System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(data.Slice(8, 4)) == 13 &&
            data.Slice(12, 4).SequenceEqual("IHDR"u8) &&
            System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(data.Slice(16, 4)) is > 0 and <= 4096 &&
            System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(data.Slice(20, 4)) is > 0 and <= 4096 &&
            data.Slice(data.Length - 8, 4).SequenceEqual("IEND"u8)) return ".png";
        if (data.Length >= 14 && (data[..6].SequenceEqual("GIF87a"u8) || data[..6].SequenceEqual("GIF89a"u8)) &&
            System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6, 2)) is > 0 and <= 4096 &&
            System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(8, 2)) is > 0 and <= 4096 &&
            data[^1] == 0x3B) return ".gif";
        if (data.Length >= 20 && data[..4].SequenceEqual("RIFF"u8) && data.Slice(8, 4).SequenceEqual("WEBP"u8) &&
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4, 4)) == data.Length - 8 &&
            (data.Slice(12, 4).SequenceEqual("VP8 "u8) || data.Slice(12, 4).SequenceEqual("VP8L"u8) || data.Slice(12, 4).SequenceEqual("VP8X"u8)) &&
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(16, 4)) <= data.Length - 20) return ".webp";
        if (data.Length >= 16 && data[0] == 0xFF && data[1] == 0xD8 && data[^2] == 0xFF && data[^1] == 0xD9)
        {
            var offset = 2;
            var hasDimensions = false;
            while (offset + 4 <= data.Length)
            {
                if (data[offset++] != 0xFF) break;
                while (offset < data.Length && data[offset] == 0xFF) offset++;
                if (offset >= data.Length) break;
                var marker = data[offset++];
                if (marker == 0xDA) return hasDimensions ? ".jpg" : null;
                if (marker is 0xD8 or 0xD9 or 0x01 or >= 0xD0 and <= 0xD7) continue;
                if (offset + 2 > data.Length) break;
                var length = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, 2));
                if (length < 2 || offset + length > data.Length) break;
                if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
                {
                    if (length < 7) break;
                    var height = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset + 3, 2));
                    var width = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset + 5, 2));
                    hasDimensions = width is > 0 and <= 4096 && height is > 0 and <= 4096;
                }
                offset += length;
            }
        }
        return null;
    }

    public async Task<string?> UpdateProfileAsync(int userId, CustomerProfileUpdate input, CancellationToken ct = default)
    {
        var profile = new CustomerProfileUpdate(input.FirstName.Trim(), input.LastName.Trim(), input.Phone.Trim(),
            input.Ward.Trim(), input.Address.Trim());
        if (new[] { profile.FirstName, profile.LastName, profile.Phone, profile.Ward, profile.Address }.Any(string.IsNullOrWhiteSpace))
            return "Please fill in all required fields.";
        if (profile.FirstName.Length > 50 || profile.LastName.Length > 50 ||
            !Regex.IsMatch(profile.FirstName, @"^[\p{L}\s]+$") || !Regex.IsMatch(profile.LastName, @"^[\p{L}\s]+$"))
            return "Names can only contain letters and spaces (up to 50 characters).";
        if (!Regex.IsMatch(profile.Phone, @"^0[0-9]{9}$"))
            return "Phone number must start with 0 and contain 10 digits.";
        if (profile.Ward.Length > 100 || !Regex.IsMatch(profile.Ward, @"^[A-Za-z\s]+$"))
            return "Please select a valid ward from the list.";
        if (profile.Address.Length > 255 || !Regex.IsMatch(profile.Address, @"^[\p{L}0-9\s/\-]+$"))
            return "Address can only contain letters, numbers, spaces, / and -.";
        return await users.UpdateProfileAsync(userId, profile, ct) ? null : "Unable to update your profile. Please log in again.";
    }

    public async Task<(CustomerUser? User, string Message)> ChangePasswordAsync(int userId, string? currentPassword,
        string? newPassword, string? confirmPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            return (null, "Please fill in all password fields.");
        if (newPassword != confirmPassword) return (null, "New passwords do not match.");
        if (newPassword != newPassword.Trim()) return (null, "Password cannot start or end with spaces.");
        var byteCount = Encoding.UTF8.GetByteCount(newPassword);
        if (byteCount < 6 || byteCount > 72) return (null, "Password must be between 6 and 72 bytes long.");

        var user = await users.FindByIdAsync(userId, ct);
        if (user is null || user.Role != "user" || user.Status != "active") return (null, "Please log in again.");
        bool verified;
        try { verified = BCrypt.Net.BCrypt.Verify(currentPassword.Trim(), user.PasswordHash); }
        catch (SaltParseException) { verified = false; }
        if (!verified) return (null, "Current password is incorrect.");

        var nextVersion = await users.ChangePasswordAsync(userId, user.PasswordHash, user.AuthVersion,
            BCrypt.Net.BCrypt.HashPassword(newPassword), ct);
        if (nextVersion is null) return (null, "Your account changed. Please try again.");
        var updated = await users.FindByIdAsync(userId, ct);
        return updated is not null && updated.AuthVersion == nextVersion
            ? (updated, "Password changed successfully.")
            : (null, "Please log in again.");
    }
}
