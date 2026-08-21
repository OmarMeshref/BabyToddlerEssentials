/* ============================================================
   Jellybean — shared app logic
   ============================================================ */

const STORE = {
  cart: "jb_cart",
  wish: "jb_wishlist",
};

/* ---------- storage helpers ---------- */
function read(key) {
  try { return JSON.parse(localStorage.getItem(key)) || (key === STORE.cart ? {} : []); }
  catch { return key === STORE.cart ? {} : []; }
}
function write(key, val) { localStorage.setItem(key, JSON.stringify(val)); }

function getCart()  { return read(STORE.cart); }        // { id: qty }
function getWish()  { return read(STORE.wish); }        // [id, id]

function cartCount() { return Object.values(getCart()).reduce((a, b) => a + b, 0); }
function cartSubtotal() {
  const cart = getCart();
  return Object.entries(cart).reduce((sum, [id, qty]) => {
    const p = findProduct(id); return p ? sum + p.price * qty : sum;
  }, 0);
}

function addToCart(id, qty = 1) {
  const cart = getCart();
  cart[id] = (cart[id] || 0) + qty;
  write(STORE.cart, cart);
  syncBadges();
  const p = findProduct(id);
  toast(`${p.emoji} Added to bag`, "Nice pick!");
}
function setQty(id, qty) {
  const cart = getCart();
  if (qty <= 0) delete cart[id]; else cart[id] = qty;
  write(STORE.cart, cart);
  syncBadges();
}
function removeFromCart(id) {
  const cart = getCart();
  delete cart[id];
  write(STORE.cart, cart);
  syncBadges();
}

function toggleWish(id) {
  const wish = getWish();
  const i = wish.indexOf(id);
  let added;
  if (i === -1) { wish.push(id); added = true; } else { wish.splice(i, 1); added = false; }
  write(STORE.wish, wish);
  syncBadges();
  const p = findProduct(id);
  toast(added ? `💖 Saved to favorites` : `💔 Removed from favorites`, p.name);
  return added;
}
function inWish(id) { return getWish().includes(id); }

const money = n => "$" + n.toFixed(2);

/* ---------- orders ---------- */
const ORDERS_KEY = "jb_orders";
function getOrders() { try { return JSON.parse(localStorage.getItem(ORDERS_KEY)) || []; } catch { return []; } }
function writeOrders(o) { localStorage.setItem(ORDERS_KEY, JSON.stringify(o)); }

function seedOrders() {
  // seed the demo history once; real checkouts append to the same list
  if (localStorage.getItem(ORDERS_KEY) === null) writeOrders(SEED_ORDERS.slice());
}

function saveOrder(order) {
  const all = getOrders();
  all.unshift(order);          // newest first
  writeOrders(all);
}
function findOrder(id) { return getOrders().find(o => o.id === id); }

/* ---------- product reviews ---------- */
const REVIEWS_KEY = "jb_reviews";               // { productId: [ {..}, ... ] }
function _allUserReviews() { try { return JSON.parse(localStorage.getItem(REVIEWS_KEY)) || {}; } catch { return {}; } }
function userReviews(pid) { return _allUserReviews()[pid] || []; }

function addReview(pid, review) {
  const all = _allUserReviews();
  const list = all[pid] || [];
  list.unshift({
    id: pid + "-u" + Date.now(),
    author: review.author,
    avatar: "🙂",
    rating: review.rating,
    title: review.title,
    body: review.body,
    date: new Date().toISOString(),
    verified: false,
    seeded: false,
  });
  all[pid] = list;
  localStorage.setItem(REVIEWS_KEY, JSON.stringify(all));
}

/* combined (user + seed), newest first */
function getReviews(pid) {
  return [...userReviews(pid), ...seedReviews(pid)]
    .sort((a, b) => new Date(b.date) - new Date(a.date));
}

function reviewStats(pid) {
  const list = getReviews(pid);
  const dist = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };
  let sum = 0;
  list.forEach(r => { dist[r.rating] = (dist[r.rating] || 0) + 1; sum += r.rating; });
  const count = list.length;
  return { count, avg: count ? sum / count : 0, dist };
}

// recompute totals from an order's items (shipping/tax rules live here)
function orderTotals(order) {
  const subtotal = order.items.reduce((s, it) => {
    const p = findProduct(it.id); return p ? s + p.price * it.qty : s;
  }, 0);
  const discount = order.discount || 0;
  const shipping = (subtotal - discount) >= 35 ? 0 : 4.95;
  const tax = +((subtotal - discount) * 0.07).toFixed(2);
  const total = +(subtotal - discount + shipping + tax).toFixed(2);
  return { subtotal, discount, shipping, tax, total };
}

/* ---------- header / footer ---------- */
const NAV = [
  { href: "index.html",        label: "Home" },
  { href: "shop.html",         label: "Shop" },
  { href: "testimonials.html", label: "Reviews" },
  { href: "about.html",        label: "About" },
  { href: "contact.html",      label: "Contact" },
];

function renderHeader() {
  const page = document.body.dataset.page;
  const links = NAV.map(n =>
    `<li><a href="${n.href}" class="${n.href.startsWith(page) && page ? "active" : ""}">${n.label}</a></li>`
  ).join("");

  const el = document.getElementById("site-header");
  if (!el) return;
  el.className = "site-header";
  el.innerHTML = `
    <nav class="nav">
      <a href="index.html" class="brand"><span class="bean">🍬</span> Jellybean</a>
      <ul class="nav-links">${links}</ul>
      <div class="nav-actions">
        <div class="account" id="account">
          <button class="icon-btn" aria-label="Account" aria-haspopup="true" onclick="toggleAccount(event)">👤</button>
          <div class="account-menu" id="account-menu">${accountMenu()}</div>
        </div>
        <a href="wishlist.html" class="icon-btn" aria-label="Favorites">💖<span class="badge" id="wish-badge">0</span></a>
        <a href="cart.html" class="icon-btn" aria-label="Shopping bag">🛍️<span class="badge" id="cart-badge">0</span></a>
        <button class="icon-btn hamburger" aria-label="Menu" onclick="openDrawer()">☰</button>
      </div>
    </nav>
    <div class="drawer" id="drawer">
      <div class="drawer-bg" onclick="closeDrawer()"></div>
      <div class="drawer-panel">
        <button class="drawer-close" onclick="closeDrawer()" aria-label="Close menu">✕</button>
        ${NAV.map(n => `<a href="${n.href}" onclick="closeDrawer()">${n.label}</a>`).join("")}
        <a href="wishlist.html">💖 Favorites</a>
        <a href="cart.html">🛍️ Shopping bag</a>
        <hr style="border:none;border-top:2px solid var(--line);margin:8px 0">
        ${drawerAccountLinks()}
      </div>
    </div>`;
}

function accountMenu() {
  const u = currentUser();
  if (u) {
    return `
      <div class="account-hi">Hi, ${u.firstName} 👋</div>
      <a href="orders.html">📦 Order history</a>
      <a href="wishlist.html">💖 Favorites</a>
      <button type="button" onclick="doLogout()">↩︎ Log out</button>`;
  }
  return `
    <div class="account-hi">Welcome!</div>
    <a href="login.html">🔑 Log in</a>
    <a href="register.html">✨ Create account</a>`;
}
function drawerAccountLinks() {
  const u = currentUser();
  if (u) {
    return `<a href="orders.html">📦 Order history</a>
            <a href="#" onclick="doLogout();return false;">↩︎ Log out</a>`;
  }
  return `<a href="login.html">🔑 Log in</a><a href="register.html">✨ Create account</a>`;
}

function toggleAccount(e) {
  e.stopPropagation();
  document.getElementById("account").classList.toggle("open");
}
function doLogout() { logout(); toast("👋 See you soon!"); setTimeout(() => location.href = "index.html", 500); }

document.addEventListener("click", e => {
  const acc = document.getElementById("account");
  if (acc && !acc.contains(e.target)) acc.classList.remove("open");
});

function openDrawer()  { document.getElementById("drawer").classList.add("open"); }
function closeDrawer() { document.getElementById("drawer").classList.remove("open"); }

function renderFooter() {
  const el = document.getElementById("site-footer");
  if (!el) return;
  el.className = "site-footer";
  el.innerHTML = `
    <div class="wrap foot-grid">
      <div>
        <a href="index.html" class="brand"><span class="bean">🍬</span> Jellybean</a>
        <p>Little things for little ones — thoughtfully chosen, safety-tested, and made to be loved.</p>
      </div>
      <div class="foot-col">
        <h4>Shop</h4>
        <a href="shop.html?cat=toys">Toys</a>
        <a href="shop.html?cat=clothing">Clothing</a>
        <a href="shop.html?cat=nursery">Nursery</a>
        <a href="shop.html?cat=feeding">Feeding</a>
      </div>
      <div class="foot-col">
        <h4>Company</h4>
        <a href="about.html">Our story</a>
        <a href="testimonials.html">Reviews</a>
        <a href="contact.html">Contact</a>
        <a href="shop.html">All products</a>
      </div>
      <div class="foot-col">
        <h4>Account</h4>
        <a href="login.html">Log in</a>
        <a href="register.html">Create account</a>
        <a href="orders.html">Order history</a>
        <a href="wishlist.html">Favorites</a>
      </div>
      <div class="foot-col">
        <h4>Help</h4>
        <a href="contact.html">Shipping</a>
        <a href="contact.html">Returns</a>
        <a href="contact.html">Safety</a>
        <a href="contact.html">FAQ</a>
      </div>
    </div>
    <div class="foot-bottom wrap">© ${new Date().getFullYear()} Jellybean. Made with 💖 for tiny humans. This is a demo store — no real orders are placed.</div>`;
}

function syncBadges() {
  const c = cartCount(), w = getWish().length;
  const cb = document.getElementById("cart-badge");
  const wb = document.getElementById("wish-badge");
  if (cb) { cb.textContent = c; cb.classList.toggle("show", c > 0); }
  if (wb) { wb.textContent = w; wb.classList.toggle("show", w > 0); }
}

/* ---------- product card ---------- */
function productCard(p) {
  const wished = inWish(p.id) ? "on" : "";
  const heart = inWish(p.id) ? "❤️" : "🤍";
  const was = p.was ? `<span class="was">${money(p.was)}</span>` : "";
  return `
    <article class="card">
      <a class="card-media ${p.bg}" href="product.html?id=${p.id}" aria-label="${p.name}">
        <span class="age-badge">${ageName(p.age)}</span>
        <span class="emoji">${p.emoji}</span>
      </a>
      <button class="wish-btn ${wished}" data-wish="${p.id}" aria-label="Toggle favorite">${heart}</button>
      <div class="card-body">
        <span class="card-cat">${catName(p.cat)}</span>
        <a href="product.html?id=${p.id}"><h3 class="card-title">${p.name}</h3></a>
        <div class="card-rating">⭐ ${p.rating} <span style="opacity:.6">(${p.reviews})</span></div>
        <div class="card-foot">
          <div class="price">${money(p.price)}${was}</div>
          <button class="add-btn" data-add="${p.id}" aria-label="Add ${p.name} to bag">+</button>
        </div>
      </div>
    </article>`;
}

/* delegate clicks for add / wish buttons */
document.addEventListener("click", e => {
  const add = e.target.closest("[data-add]");
  if (add) { addToCart(add.dataset.add); bump(add); return; }
  const wish = e.target.closest("[data-wish]");
  if (wish) {
    const on = toggleWish(wish.dataset.wish);
    wish.classList.toggle("on", on);
    wish.textContent = on ? "❤️" : "🤍";
    bump(wish);
  }
});
function bump(el) { el.animate(
  [{ transform: "scale(1)" }, { transform: "scale(1.3)" }, { transform: "scale(1)" }],
  { duration: 300, easing: "cubic-bezier(.34,1.56,.64,1)" }); }

/* ---------- toast ---------- */
function toast(title, sub = "") {
  let wrap = document.querySelector(".toast-wrap");
  if (!wrap) { wrap = document.createElement("div"); wrap.className = "toast-wrap"; document.body.appendChild(wrap); }
  const t = document.createElement("div");
  t.className = "toast";
  t.innerHTML = `<span>${title}</span>${sub ? `<span style="opacity:.7;font-weight:600">· ${sub}</span>` : ""}`;
  wrap.appendChild(t);
  setTimeout(() => {
    t.style.transition = "opacity .3s, transform .3s";
    t.style.opacity = "0"; t.style.transform = "translateY(10px)";
    setTimeout(() => t.remove(), 300);
  }, 2200);
}

/* ---------- scroll reveal ---------- */
function initReveal() {
  const els = document.querySelectorAll(".reveal");
  if (!els.length) return;
  const io = new IntersectionObserver(entries => {
    entries.forEach(en => { if (en.isIntersecting) { en.target.classList.add("in"); io.unobserve(en.target); } });
  }, { threshold: .12 });
  els.forEach(el => io.observe(el));
}

/* ---------- boot ---------- */
document.addEventListener("DOMContentLoaded", () => {
  seedOrders();
  renderHeader();
  renderFooter();
  syncBadges();
  initReveal();
  const page = document.body.dataset.page;
  const initFn = window["init_" + (page || "").replace(".html", "")];
  if (typeof initFn === "function") initFn();
});
