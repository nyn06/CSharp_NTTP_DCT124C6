const $ = (selector, context = document) => context.querySelector(selector);
const $$ = (selector, context = document) => Array.from(context.querySelectorAll(selector));
const fmt = value => '$' + Number(value || 0).toFixed(2);

function toText(value) {
  if (typeof value === 'string') {
    return value;
  }

  if (value === null || value === undefined) {
    return '';
  }

  return String(value);
}

function toNumber(value, fallback = 0) {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : fallback;
}

function escapeHtml(value) {
  return toText(value).replace(/[&<>"']/g, char => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  }[char]));
}

function titleCase(value) {
  return toText(value)
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

function normalizeProduct(product) {
  if (!product || typeof product !== 'object') {
    return null;
  }

  const id = toText(product.id).trim();
  const rawCategory = toText(product.cat || product.category).trim();
  const cat = rawCategory.toLowerCase();

  if (!id || !toText(product.name).trim()) {
    return null;
  }

  return {
    id,
    name: toText(product.name).trim(),
    cat,
    category: toText(product.category).trim() || titleCase(cat),
    price: toNumber(product.price ?? product.sell_price, 0),
    img: toText(product.img || product.image).trim(),
    desc: toText(product.desc || product.shortDesc || product.short_description).trim(),
    shortDesc: toText(product.shortDesc || product.desc || product.short_description).trim(),
    description: toText(product.description).trim(),
    ingredients: toText(product.ingredients).trim(),
    qty: Math.max(0, Math.floor(toNumber(product.qty ?? product.quantity, 0))),
    status: toText(product.status || 'active').trim().toLowerCase() || 'active',
    hasSize: Boolean(product.hasSize ?? product.has_size),
    hasIce: Boolean(product.hasIce ?? product.has_ice),
    hasSugar: Boolean(product.hasSugar ?? product.has_sugar),
    createdAt: toText(product.createdAt || product.created_at).trim()
  };
}

function normalizeProducts(products) {
  if (!Array.isArray(products)) {
    return [];
  }

  return products
    .map(normalizeProduct)
    .filter(Boolean);
}

function getProductCategoryLabel(product) {
  return toText(product.category).trim() || titleCase(product.cat);
}

function getProductImagePath(product) {
  const image = toText(product.img).trim();

  if (!image) {
    return 'assets/img/Metropolitan Cafe Logo with Flowing Script.png';
  }

  if (image.startsWith('data:image') || image.includes('assets/') || image.includes('http')) {
    return image;
  }

  return 'assets/img/' + image;
}

function isSoldOut(product) {
  return toNumber(product.qty, 0) <= 0 || toText(product.status).toLowerCase() === 'inactive';
}

function compareProductsByNewest(a, b) {
  const aTimestamp = Date.parse(a.createdAt);
  const bTimestamp = Date.parse(b.createdAt);
  const aScore = Number.isFinite(aTimestamp) ? aTimestamp : toNumber(a.id, 0);
  const bScore = Number.isFinite(bTimestamp) ? bTimestamp : toNumber(b.id, 0);

  return bScore - aScore;
}

let PRODUCTS = normalizeProducts(window.PRODUCTS || []);
window.PRODUCTS = PRODUCTS;

function setProducts(products, { rerender = true } = {}) {
  PRODUCTS = normalizeProducts(products);
  window.PRODUCTS = PRODUCTS;

  if (rerender) {
    rerenderProductViews();
  }
}

function cardHTML(product) {
  const soldOut = isSoldOut(product);
  const addButton = soldOut
    ? '<button class="btn btn-sm btn-secondary disabled" disabled>Sold Out</button>'
    : `<button class="btn btn-sm btn-add" data-id="${escapeHtml(product.id)}">Add</button>`;

  return `<div class="col-6 col-md-4 col-lg-3">
    <div class="card h-100 shadow-sm border-0">
      <img src="${escapeHtml(getProductImagePath(product))}" class="card-img-top rounded" alt="${escapeHtml(product.name)}" loading="lazy" height="180">
      <div class="card-body d-flex flex-column">
        <span class="badge badge-cat align-self-start mb-2 text-uppercase">${escapeHtml(product.cat)}</span>
        <h6 class="card-title flex-grow-1">${escapeHtml(product.name)}</h6>
        <p class="small text-muted mb-2">${escapeHtml(product.shortDesc || product.desc)}</p>
        <div class="d-flex justify-content-between align-items-center">
          <span class="price">${fmt(product.price)}</span>
          <div class="btn-group">
            <a href="product.php?id=${encodeURIComponent(product.id)}" class="btn btn-sm btn-view">View</a>
            ${addButton}
          </div>
        </div>
      </div>
    </div>
  </div>`;
}

function addToCart(item) {
  const loggedUser = window.CURRENT_USER;

  if (!loggedUser) {
    alert('Please login to add products to your cart!');
    window.location.href = 'login.php';
    return;
  }

  const product = PRODUCTS.find(entry => String(entry.id) === String(item.id));

  if (!product) {
    alert('Product not found!');
    return;
  }

  if (isSoldOut(product)) {
    alert('This product is sold out and cannot be purchased.');
    return;
  }

  fetch('add_to_cart.php', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    credentials: 'same-origin',
    body: JSON.stringify(item)
  })
    .then(response => response.json())
    .then(data => {
      if (data.success) {
        alert(`${product.name} added to cart!`);

        const badge = document.getElementById('navCartQty');
        if (badge) {
          badge.textContent = data.totalQty;
        }

        return;
      }

      alert(data.message || 'Error adding to cart.');
    })
    .catch(error => {
      console.error(error);
      alert('Something went wrong!');
    });
}

function renderHomePicks() {
  const grid = $('#homePicks');
  if (!grid) {
    return;
  }

  const coffeeProducts = PRODUCTS.filter(product => product.cat === 'coffee');
  const source = coffeeProducts.length ? coffeeProducts : PRODUCTS;
  const picks = [...source].sort(compareProductsByNewest).slice(0, 8);

  grid.innerHTML = picks.length
    ? picks.map(cardHTML).join('')
    : '<p class="text-muted">No products available right now.</p>';
}

const menuPage = (() => {
  const sectionsContainer = document.getElementById('menuSections');
  if (!sectionsContainer) {
    return null;
  }

  const searchSection = document.getElementById('searchResults');
  const searchGrid = document.getElementById('searchGrid');
  const searchPagination = document.getElementById('searchPagination');
  const searchInput = document.getElementById('searchInput');
  const categorySelect = document.getElementById('category');
  const minInput = document.getElementById('minPrice');
  const maxInput = document.getElementById('maxPrice');
  const priceError = document.getElementById('priceError');
  const searchBtn = document.getElementById('searchBtn');
  const clearBtn = document.getElementById('clearFilters');
  const suggestionBox = document.getElementById('suggestions');
  const ITEMS_PER_PAGE = 8;
  let appliedFilters = null;
  let suppressSuggestions = false;
  let currentSearchPage = 1;
  const currentCategoryPages = {};

  function getDraftFilters() {
    return {
      keyword: toText(searchInput?.value).trim(),
      category: toText(categorySelect?.value).trim().toLowerCase() || 'all',
      minPriceRaw: toText(minInput?.value).trim(),
      maxPriceRaw: toText(maxInput?.value).trim()
    };
  }

  function hasFilterValues(filters) {
    if (!filters) {
      return false;
    }

    return Boolean(
      toText(filters.keyword).trim() ||
      toText(filters.category).toLowerCase() !== 'all' ||
      toText(filters.minPriceRaw).trim() ||
      toText(filters.maxPriceRaw).trim()
    );
  }

  function validatePriceRange() {
    const minRaw = toText(minInput?.value).trim();
    const maxRaw = toText(maxInput?.value).trim();
    const min = minRaw === '' ? NaN : toNumber(minRaw, NaN);
    const max = maxRaw === '' ? NaN : toNumber(maxRaw, NaN);
    const invalid = !Number.isNaN(min) && !Number.isNaN(max) && min > max;

    if (priceError) {
      priceError.style.display = invalid ? 'block' : 'none';
    }

    return invalid;
  }

  function getCategories() {
    const categories = new Map();

    PRODUCTS.forEach(product => {
      if (!product.cat) {
        return;
      }

      categories.set(product.cat, getProductCategoryLabel(product));
    });

    return Array.from(categories.entries())
      .sort((a, b) => a[1].localeCompare(b[1]))
      .map(([key, label]) => ({ key, label }));
  }

  function syncCategoryOptions() {
    if (!categorySelect) {
      return;
    }

    const currentValue = toText(categorySelect.value).toLowerCase() || 'all';
    const categories = getCategories();

    categorySelect.innerHTML = [
      '<option value="all">All Category</option>',
      ...categories.map(category => (
        `<option value="${escapeHtml(category.key)}">${escapeHtml(category.label)}</option>`
      ))
    ].join('');

    categorySelect.value = categories.some(category => category.key === currentValue)
      ? currentValue
      : 'all';
  }

  function renderPagination(container, totalPages, currentPage, onChange) {
    if (!container) {
      return;
    }

    container.innerHTML = '';

    if (totalPages <= 1) {
      return;
    }

    const previousButton = document.createElement('button');
    previousButton.innerHTML = '&larr;';
    previousButton.className = 'btn btn-sm btn-outline-dark';
    previousButton.disabled = currentPage === 1;
    previousButton.addEventListener('click', () => onChange(currentPage - 1));
    container.appendChild(previousButton);

    for (let page = 1; page <= totalPages; page += 1) {
      const pageButton = document.createElement('button');
      pageButton.textContent = page;
      pageButton.className = page === currentPage ? 'btn btn-sm btn-dark' : 'btn btn-sm btn-outline-dark';
      pageButton.disabled = page === currentPage;
      pageButton.addEventListener('click', () => onChange(page));
      container.appendChild(pageButton);
    }

    const nextButton = document.createElement('button');
    nextButton.innerHTML = '&rarr;';
    nextButton.className = 'btn btn-sm btn-outline-dark';
    nextButton.disabled = currentPage === totalPages;
    nextButton.addEventListener('click', () => onChange(currentPage + 1));
    container.appendChild(nextButton);
  }

  function renderCategorySection(categoryKey, page = 1) {
    const section = sectionsContainer.querySelector(`[data-category="${categoryKey}"]`);
    const grid = section?.querySelector('[data-grid]');
    const paginator = section?.querySelector('[data-pagination]');

    if (!grid || !paginator) {
      return;
    }

    const items = PRODUCTS.filter(product => product.cat === categoryKey);
    const totalPages = Math.max(1, Math.ceil(items.length / ITEMS_PER_PAGE));
    const currentPage = Math.min(Math.max(page, 1), totalPages);
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    const currentItems = items.slice(start, start + ITEMS_PER_PAGE);

    grid.innerHTML = currentItems.length
      ? currentItems.map(cardHTML).join('')
      : '<p class="text-muted">No products found.</p>';

    renderPagination(paginator, totalPages, currentPage, nextPage => {
      currentCategoryPages[categoryKey] = nextPage;
      renderCategorySection(categoryKey, nextPage);
    });
  }

  function renderDefaultSections() {
    const categories = getCategories();
    searchSection.style.display = 'none';
    sectionsContainer.style.display = '';

    if (!categories.length) {
      sectionsContainer.innerHTML = '<p class="text-muted">No products available right now.</p>';
      return;
    }

    sectionsContainer.innerHTML = categories.map(category => `
      <section class="menu-section mb-5" data-category="${escapeHtml(category.key)}">
        <h3 class="menu-heading mb-4">${escapeHtml(category.label)}</h3>
        <div class="row g-4" data-grid="${escapeHtml(category.key)}"></div>
        <div class="pagination-btns d-flex justify-content-center mt-3 gap-2" data-pagination="${escapeHtml(category.key)}"></div>
      </section>
    `).join('');

    categories.forEach(category => {
      renderCategorySection(category.key, currentCategoryPages[category.key] || 1);
    });
  }

  function hasActiveFilters() {
    return hasFilterValues(getDraftFilters());
  }

  function buildFilteredResults(filters = appliedFilters) {
    if (!filters) {
      return [];
    }

    const keyword = toText(filters.keyword).trim().toLowerCase();
    const selectedCategory = toText(filters.category).trim().toLowerCase() || 'all';
    const minPrice = toText(filters.minPriceRaw).trim() === '' ? 0 : Math.max(0, toNumber(filters.minPriceRaw, 0));
    const maxPrice = toText(filters.maxPriceRaw).trim() === '' ? Infinity : Math.max(0, toNumber(filters.maxPriceRaw, Infinity));

    return PRODUCTS.filter(product => {
      const matchesKeyword = !keyword || product.name.toLowerCase().includes(keyword);
      const matchesCategory = selectedCategory === 'all' || product.cat === selectedCategory;
      const matchesPrice = product.price >= minPrice && product.price <= maxPrice;

      return matchesKeyword && matchesCategory && matchesPrice;
    });
  }

  function renderSearchResults(page = 1) {
    if (!hasFilterValues(appliedFilters)) {
      renderDefaultSections();
      return;
    }

    const results = buildFilteredResults(appliedFilters);
    const totalPages = Math.max(1, Math.ceil(results.length / ITEMS_PER_PAGE));
    const currentPage = Math.min(Math.max(page, 1), totalPages);
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    const currentItems = results.slice(start, start + ITEMS_PER_PAGE);

    sectionsContainer.style.display = 'none';
    searchSection.style.display = '';
    searchGrid.innerHTML = currentItems.length
      ? currentItems.map(cardHTML).join('')
      : '<p class="text-muted">No products found.</p>';

    const title = searchSection.querySelector('h3');
    if (title) {
      title.textContent = `Search Results (${results.length} found)`;
    }

    renderPagination(searchPagination, totalPages, currentPage, nextPage => {
      currentSearchPage = nextPage;
      renderSearchResults(nextPage);
    });
  }

  function updateClearButtonVisibility() {
    if (!clearBtn) {
      return;
    }

    clearBtn.style.display = hasActiveFilters() ? 'inline-block' : 'none';
  }

  function updateSuggestions() {
    if (!suggestionBox || !searchInput || !categorySelect) {
      return;
    }

    const keyword = searchInput.value.trim().toLowerCase();
    const selectedCategory = categorySelect.value.toLowerCase();

    if (!keyword || suppressSuggestions) {
      suggestionBox.innerHTML = '';
      suggestionBox.style.display = 'none';
      return;
    }

    const matches = Array.from(new Set(
      PRODUCTS
        .filter(product => {
          const matchesCategory = selectedCategory === 'all' || product.cat === selectedCategory;
          return matchesCategory && product.name.toLowerCase().includes(keyword);
        })
        .map(product => product.name)
    ))
      .filter(name => name.trim().toLowerCase() !== keyword)
      .slice(0, 8);

    if (!matches.length) {
      suggestionBox.innerHTML = '';
      suggestionBox.style.display = 'none';
      return;
    }

    suggestionBox.innerHTML = matches
      .map(name => `<div class="suggestion-item">${escapeHtml(name)}</div>`)
      .join('');
    suggestionBox.style.display = 'block';
  }

  function renderCurrentView() {
    syncCategoryOptions();

    if (hasFilterValues(appliedFilters)) {
      renderSearchResults(currentSearchPage);
    } else {
      renderDefaultSections();
    }

    updateClearButtonVisibility();
    // Không gọi updateSuggestions() ở đây — polling/SSE không được làm hiện lại suggestion
  }

  searchBtn?.addEventListener('click', () => {
    if (validatePriceRange()) {
      return;
    }

    suppressSuggestions = true;
    if (suggestionBox) {
      suggestionBox.innerHTML = '';
      suggestionBox.style.display = 'none';
    }

    appliedFilters = getDraftFilters();
    currentSearchPage = 1;
    renderCurrentView();
  });

  searchInput?.addEventListener('keydown', event => {
    if (event.key === 'Enter') {
      event.preventDefault();
    }
  });

  clearBtn?.addEventListener('click', () => {
    if (searchInput) {
      searchInput.value = '';
    }

    if (categorySelect) {
      categorySelect.value = 'all';
    }

    if (minInput) {
      minInput.value = '';
    }

    if (maxInput) {
      maxInput.value = '';
    }

    appliedFilters = null;
    currentSearchPage = 1;
    Object.keys(currentCategoryPages).forEach(k => delete currentCategoryPages[k]);
    suppressSuggestions = false;
    validatePriceRange();
    renderCurrentView();
  });

  [searchInput, categorySelect].forEach(input => {
    input?.addEventListener('input', () => {
      suppressSuggestions = false;
      updateClearButtonVisibility();
      updateSuggestions();
    });

    input?.addEventListener('change', () => {
      suppressSuggestions = false;
      updateClearButtonVisibility();
      updateSuggestions();
    });
  });

  [minInput, maxInput].forEach(input => {
    input?.addEventListener('keydown', event => {
      if (['e', 'E', '+', '-', ','].includes(event.key)) {
        event.preventDefault();
      }
    });

    input?.addEventListener('input', () => {
      if (toNumber(input.value, 0) < 0) {
        input.value = '0';
      }

      validatePriceRange();
      updateClearButtonVisibility();
    });
  });

  function applySuggestionSelection(item) {
    if (!item || !searchInput || !suggestionBox) {
      return;
    }

    searchInput.value = item.textContent.trim();
    suppressSuggestions = true;
    suggestionBox.innerHTML = '';
    suggestionBox.style.display = 'none';
    searchInput.blur();
    updateClearButtonVisibility();
  }

  function handleSuggestionInteraction(event) {
    const item = event.target.closest('.suggestion-item');
    if (!item || !searchInput) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    applySuggestionSelection(item);
  }

  suggestionBox?.addEventListener('pointerdown', handleSuggestionInteraction);
  suggestionBox?.addEventListener('click', handleSuggestionInteraction);

  document.addEventListener('click', event => {
    if (!event.target.closest('#searchInput') && !event.target.closest('#suggestions')) {
      if (suggestionBox) {
        suggestionBox.style.display = 'none';
      }
    }
  });

  return {
    rerender: renderCurrentView
  };
})();

function syncProductDetailState() {
  if (location.pathname.split('/').pop() !== 'product.php' || !window.PRODUCT) {
    return;
  }

  const product = normalizeProduct(window.PRODUCT);
  if (!product || !isSoldOut(product)) {
    return;
  }

  const addButton = document.getElementById('addBtn');
  const buyNowButton = document.getElementById('buyNowBtn');
  const quantityInput = document.getElementById('qty');

  if (addButton) {
    addButton.classList.remove('btn-dark', 'btn-light');
    addButton.classList.add('btn-secondary');
    addButton.textContent = 'Sold Out';
    addButton.disabled = true;
  }

  if (buyNowButton) {
    buyNowButton.disabled = true;
    buyNowButton.style.display = 'none';
  }

  if (quantityInput) {
    quantityInput.disabled = true;
  }
}

function rerenderProductViews() {
  renderHomePicks();
  menuPage?.rerender();
  syncProductDetailState();
}

async function fetchCatalogProducts() {
  const response = await fetch('get_products.php', {
    cache: 'no-store',
    headers: {
      Accept: 'application/json'
    }
  });

  if (!response.ok) {
    throw new Error('Unable to load products');
  }

  const payload = await response.json();
  return Array.isArray(payload?.products) ? payload.products : payload;
}

let productRefreshPromise = null;
let productPollingTimer = null;
let productStream = null;

async function refreshProductsFromServer() {
  if (productRefreshPromise) {
    return productRefreshPromise;
  }

  productRefreshPromise = (async () => {
    try {
      const products = await fetchCatalogProducts();
      setProducts(products);
    } catch (error) {
      console.error(error);
    } finally {
      productRefreshPromise = null;
    }
  })();

  return productRefreshPromise;
}

function hasRealtimeProductView() {
  return Boolean(document.getElementById('homePicks') || document.getElementById('menuSections'));
}

function startProductPollingFallback() {
  if (productPollingTimer || !hasRealtimeProductView()) {
    return;
  }

  productPollingTimer = window.setInterval(() => {
    if (!document.hidden) {
      refreshProductsFromServer();
    }
  }, 15000);
}

function initRealtimeProducts() {
  if (!hasRealtimeProductView()) {
    return;
  }

  window.addEventListener('focus', () => {
    refreshProductsFromServer();
  });

  document.addEventListener('visibilitychange', () => {
    if (!document.hidden) {
      refreshProductsFromServer();
    }
  });

  if (!('EventSource' in window)) {
    startProductPollingFallback();
    return;
  }

  productStream = new EventSource('products_stream.php');
  productStream.addEventListener('products-sync', () => {
    refreshProductsFromServer();
  });
  productStream.onerror = () => {
    startProductPollingFallback();
  };

  window.addEventListener('beforeunload', () => {
    productStream?.close();
  });
}

rerenderProductViews();
initRealtimeProducts();

document.addEventListener('click', event => {
  const button = event.target.closest('.btn-add');
  if (!button) {
    return;
  }

  const product = PRODUCTS.find(entry => String(entry.id) === String(button.dataset.id));
  if (!product) {
    alert('Product not found!');
    return;
  }

  addToCart({
    id: product.id,
    name: product.name,
    price: product.price,
    size: null,
    ice: null,
    sugar: null,
    qty: 1
  });
});