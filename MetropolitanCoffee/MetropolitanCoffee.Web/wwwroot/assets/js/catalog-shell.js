document.addEventListener('DOMContentLoaded', () => {
  const pageSize = 8;
  function paginate(grid, controls) {
    if (!grid || !controls) return;
    const cards = [...grid.querySelectorAll('.catalog-product')];
    const total = Math.ceil(cards.length / pageSize);
    let page = 1;
    function show(next) {
      page = Math.max(1, Math.min(total, next));
      cards.forEach((card, index) => { card.style.display = index >= (page - 1) * pageSize && index < page * pageSize ? '' : 'none'; });
      controls.replaceChildren();
      if (total <= 1) return;
      function button(label, target, disabled, current = false) {
        const element = document.createElement('button');
        element.type = 'button';
        element.className = current ? 'btn btn-sm btn-dark' : 'btn btn-sm btn-outline-dark';
        element.textContent = label;
        element.disabled = disabled;
        element.addEventListener('click', () => show(target));
        controls.append(element);
      }
      button('←', page - 1, page === 1);
      for (let i = 1; i <= total; i++) button(String(i), i, i === page, i === page);
      button('→', page + 1, page === total);
    }
    show(1);
  }
  document.querySelectorAll('#menuSections .menu-section').forEach(section =>
    paginate(section.querySelector('[data-grid]'), section.querySelector('[data-pagination]')));
  paginate(document.getElementById('searchGrid'), document.getElementById('searchPagination'));

  const form = document.getElementById('catalogSearch');
  const name = document.getElementById('searchInput');
  const category = document.getElementById('category');
  const min = document.getElementById('minPrice');
  const max = document.getElementById('maxPrice');
  const priceError = document.getElementById('priceError');
  const suggestions = document.getElementById('suggestions');
  const productNames = [...new Set([...document.querySelectorAll('#menuSections .catalog-product, #searchGrid .catalog-product')]
    .map(card => card.dataset.name))];
  form?.addEventListener('submit', event => {
    const low = min.value === '' ? null : Number(min.value);
    const high = max.value === '' ? null : Number(max.value);
    if (low !== null && high !== null && low > high) {
      event.preventDefault();
      priceError.style.display = 'block';
    } else {
      priceError.style.display = 'none';
    }
  });
  name?.addEventListener('input', () => {
    const keyword = name.value.trim().toLowerCase();
    suggestions.replaceChildren();
    if (!keyword) { suggestions.style.display = 'none'; return; }
    productNames.filter(item => item.toLowerCase().includes(keyword) && item.toLowerCase() !== keyword)
      .slice(0, 8).forEach(item => {
        const option = document.createElement('div');
        option.className = 'suggestion-item';
        option.textContent = item;
        option.addEventListener('click', () => { name.value = item; suggestions.style.display = 'none'; });
        suggestions.append(option);
      });
    suggestions.style.display = suggestions.childElementCount ? 'block' : 'none';
  });
  document.addEventListener('click', event => {
    if (!event.target.closest('#searchInput, #suggestions')) suggestions.style.display = 'none';
  });
  const back = document.getElementById('backToTop');
  window.addEventListener('scroll', () => { back.style.display = window.scrollY > 300 ? 'flex' : 'none'; });
  back.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
});
