
namespace MetropolitanCoffee.Core.Models;

// Thông tin sản phẩm để lựa chọn khi nhập hàng.
public sealed record PurchaseProduct(
    int Id,
    string Name,
    decimal CostPrice,
    int Quantity,
    string Status
);

// Một dòng sản phẩm trong phiếu nhập.
public sealed record PurchaseOrderLine(
    int Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal ImportPrice
)
{
    public decimal Subtotal => Quantity * ImportPrice;
}

// Thông tin phiếu nhập.
public sealed record PurchaseOrder(
    int Id,
    DateTime CreatedAt,
    string Status,
    IReadOnlyList<PurchaseOrderLine> Items
)
{
    public decimal TotalValue => Items.Sum(item => item.Subtotal);
}

// Dữ liệu một dòng sản phẩm do Admin nhập.
public sealed record PurchaseOrderItemInput(
    int ProductId,
    int Quantity,
    decimal ImportPrice
);


// Dữ liệu tạo hoặc chỉnh sửa phiếu nhập.
public sealed record PurchaseOrderInput(
    DateTime OrderDate,
    IReadOnlyList<PurchaseOrderItemInput> Items
);


// Trạng thái kết quả xử lý.
public enum PurchaseWriteStatus
{
    Saved,
    Completed,
    NotFound,
    InvalidStatus,
    InvalidProduct,
    EmptyItems,
    DuplicateProduct
}

// Kết quả trả về từ Repository.
public sealed record PurchaseWriteResult(
    PurchaseWriteStatus Status,
    int? Id = null
);


 // Thông tin tóm tắt để hiển thị trong danh sách phiếu nhập.
 public sealed record PurchaseOrderSummary(
     int Id,
     DateTime CreatedAt,
     decimal TotalValue,
     string Status
 );

