/* ============================================================
   Jellybean — per-page logic
   ============================================================ */

/* ---------------- HOME ---------------- */
function init_index() {
  // category tiles
  const cats = document.getElementById("cat-grid");
  if (cats) cats.innerHTML = CATEGORIES.map(c => `
    <a class="cat-tile" href="shop.html?cat=${c.id}">
      <div class="emoji">${c.emoji}</div>
      <div class="name">${c.name}</div>
    </a>`).join("");

  // featured = top 8 by rating
  const feat = document.getElementById("featured-grid");
  if (feat) {
    const top = [...PRODUCTS].sort((a, b) => b.rating - a.rating).slice(0, 8);
    feat.innerHTML = top.map(productCard).join("");
  }

  // newsletter
  wireNewsletter();
}

/* ---------------- SHOP ---------------- */
function init_shop() {
  const params = new URLSearchParams(location.search);
  const state = {
    q: params.get("q") || "",
    cats: new Set(params.get("cat") ? [params.get("cat")] : []),
    ages: new Set(),
    max: 40,
    sort: "featured",
  };

  const grid   = document.getElementById("shop-grid");
  const count  = document.getElementById("result-count");
  const search = document.getElementById("search");
  const sortSel= document.getElementById("sort");
  const rangeEl= document.getElementById("price-range");
  const rangeV = document.getElementById("price-val");

  if (search) search.value = state.q;

  // build filter chips
  const catBox = document.getElementById("cat-chips");
  catBox.innerHTML = CATEGORIES.map(c =>
    `<button class="chip ${state.cats.has(c.id) ? "on" : ""}" data-cat="${c.id}">${c.emoji} ${c.name}</button>`).join("");
  const ageBox = document.getElementById("age-chips");
  ageBox.innerHTML = AGES.map(a =>
    `<button class="chip" data-age="${a.id}">${a.name}</button>`).join("");

  function render() {
    let list = PRODUCTS.filter(p => {
      if (state.q && !(`${p.name} ${catName(p.cat)}`.toLowerCase().includes(state.q.toLowerCase()))) return false;
      if (state.cats.size && !state.cats.has(p.cat)) return false;
      if (state.ages.size && !state.ages.has(p.age)) return false;
      if (p.price > state.max) return false;
      return true;
    });
    if (state.sort === "low")  list.sort((a, b) => a.price - b.price);
    if (state.sort === "high") list.sort((a, b) => b.price - a.price);
    if (state.sort === "rating") list.sort((a, b) => b.rating - a.rating);

    count.textContent = `${list.length} ${list.length === 1 ? "item" : "items"}`;
    grid.innerHTML = list.length
      ? list.map(productCard).join("")
      : `<div class="empty" style="grid-column:1/-1">
           <div class="big">🫧</div>
           <h3>No matches yet</h3>
           <p>Try clearing a filter or searching something else.</p>
           <button class="btn btn--ghost" onclick="location.href='shop.html'">Reset filters</button>
         </div>`;
  }

  // events
  catBox.addEventListener("click", e => {
    const b = e.target.closest("[data-cat]"); if (!b) return;
    const id = b.dataset.cat;
    state.cats.has(id) ? state.cats.delete(id) : state.cats.add(id);
    b.classList.toggle("on"); render();
  });
  ageBox.addEventListener("click", e => {
    const b = e.target.closest("[data-age]"); if (!b) return;
    const id = b.dataset.age;
    state.ages.has(id) ? state.ages.delete(id) : state.ages.add(id);
    b.classList.toggle("on"); render();
  });
  if (search) search.addEventListener("input", e => { state.q = e.target.value; render(); });
  if (sortSel) sortSel.addEventListener("change", e => { state.sort = e.target.value; render(); });
  if (rangeEl) rangeEl.addEventListener("input", e => {
    state.max = +e.target.value; rangeV.textContent = money(state.max); render();
  });
  const resetBtn = document.getElementById("reset-filters");
  if (resetBtn) resetBtn.addEventListener("click", () => location.href = "shop.html");

  render();
}

/* ---------------- PRODUCT DETAIL ---------------- */
function init_product() {
  const id = new URLSearchParams(location.search).get("id");
  const p = findProduct(id);
  const root = document.getElementById("pdp-root");
  if (!p) {
    root.innerHTML = `<div class="empty"><div class="big">🔍</div><h3>Product not found</h3>
      <p>That item may have wandered off.</p><a class="btn" href="shop.html">Back to shop</a></div>`;
    return;
  }
  document.title = `${p.name} · Jellybean`;
  let qty = 1;

  // gallery: use real image URLs if present, else placeholder slots
  const hasReal = Array.isArray(p.images) && p.images.length > 0;
  const slots = hasReal
    ? p.images.map((src, i) => ({ src, label: GALLERY_LABELS[i] || `Photo ${i + 1}` }))
    : Array.from({ length: GALLERY_SLOTS }, (_, i) => ({ src: null, label: GALLERY_LABELS[i] || `Photo ${i + 1}` }));

  const mainSlot = i => slots[i].src
    ? `<img src="${slots[i].src}" alt="${p.name} — ${slots[i].label}">`
    : `<div class="ph-main"><span class="ph-emoji">${p.emoji}</span><span class="ph-tag">${slots[i].label} photo</span></div>`;

  const thumbs = slots.map((s, i) => `
    <button class="thumb ${i === 0 ? "on" : ""}" data-thumb="${i}" aria-label="View ${s.label}">
      ${s.src ? `<img src="${s.src}" alt="${s.label}">` : `<span class="ph-emoji sm">${p.emoji}</span>`}
    </button>`).join("");
  const was = p.was ? `<span style="font-size:1.1rem;color:var(--ink-soft);text-decoration:line-through;font-weight:600;margin-left:8px">${money(p.was)}</span>` : "";
  const save = p.was ? `<span class="pill" style="background:#FFF0F7;color:var(--pink-deep)">Save ${money(p.was - p.price)}</span>` : "";
  const st = reviewStats(p.id);
  const ratingLine = `⭐ ${st.avg.toFixed(1)} · <a href="#reviews-section" style="color:inherit;text-decoration:underline">${st.count} review${st.count === 1 ? "" : "s"}</a>`;

  root.innerHTML = `
    <div class="gallery">
      <div class="gallery-main ${p.bg}" id="gallery-main">${mainSlot(0)}</div>
      <div class="gallery-thumbs" id="gallery-thumbs">${thumbs}</div>
    </div>
    <div class="pdp-info">
      <span class="pdp-cat">${catName(p.cat)}</span>
      <h1>${p.name}</h1>
      <div class="card-rating" style="font-size:1rem">${ratingLine}</div>
      <div class="pdp-price">${money(p.price)}${was}</div>
      <p class="pdp-desc">${p.desc}</p>
      <div class="pdp-meta">
        <span class="pill">👶 ${ageName(p.age)}</span>
        <span class="pill">🚚 Free shipping over $35</span>
        ${save}
      </div>
      <div class="pdp-actions">
        <div class="qty">
          <button id="q-minus" aria-label="Decrease">−</button>
          <span id="q-val">1</span>
          <button id="q-plus" aria-label="Increase">+</button>
        </div>
        <button class="btn" id="pdp-add">🛍️ Add to bag</button>
        <button class="btn btn--ghost" id="pdp-wish">${inWish(p.id) ? "❤️ Saved" : "🤍 Save"}</button>
      </div>
      <ul class="feature-list">
        ${p.features.map(f => `<li><span class="tick">✓</span> ${f}</li>`).join("")}
      </ul>
    </div>`;

  // gallery thumbnail switching
  const mainEl = root.querySelector("#gallery-main");
  root.querySelectorAll("[data-thumb]").forEach(btn => btn.onclick = () => {
    root.querySelectorAll(".thumb").forEach(t => t.classList.remove("on"));
    btn.classList.add("on");
    mainEl.innerHTML = mainSlot(+btn.dataset.thumb);
  });

  const qv = root.querySelector("#q-val");
  root.querySelector("#q-minus").onclick = () => { qty = Math.max(1, qty - 1); qv.textContent = qty; };
  root.querySelector("#q-plus").onclick  = () => { qty++; qv.textContent = qty; };
  root.querySelector("#pdp-add").onclick = () => addToCart(p.id, qty);
  const wb = root.querySelector("#pdp-wish");
  wb.onclick = () => { const on = toggleWish(p.id); wb.textContent = on ? "❤️ Saved" : "🤍 Save"; };

  // reviews
  renderReviews(p.id);

  // related
  const rel = document.getElementById("related-grid");
  if (rel) {
    const related = PRODUCTS.filter(x => x.cat === p.cat && x.id !== p.id).slice(0, 4);
    const fill = PRODUCTS.filter(x => x.id !== p.id && !related.includes(x)).slice(0, 4 - related.length);
    rel.innerHTML = [...related, ...fill].map(productCard).join("");
  }
}

function starRow(n) { return "★".repeat(n) + "☆".repeat(5 - n); }
function relDate(iso) {
  const days = Math.round((Date.now() - new Date(iso)) / 86400000);
  if (days < 1) return "today";
  if (days < 7) return days + (days === 1 ? " day ago" : " days ago");
  if (days < 30) { const w = Math.round(days / 7); return w + (w === 1 ? " week ago" : " weeks ago"); }
  const m = Math.round(days / 30); return m + (m === 1 ? " month ago" : " months ago");
}

function renderReviews(pid) {
  const root = document.getElementById("reviews-root");
  if (!root) return;
  const st = reviewStats(pid);
  const list = getReviews(pid);
  const maxBar = Math.max(...Object.values(st.dist), 1);

  const bars = [5, 4, 3, 2, 1].map(n => `
    <div class="dist-row">
      <span class="dist-label">${n}★</span>
      <span class="dist-track"><span class="dist-fill" style="width:${(st.dist[n] / maxBar) * 100}%"></span></span>
      <span class="dist-num">${st.dist[n]}</span>
    </div>`).join("");

  const reviewItems = list.map(r => `
    <article class="review">
      <div class="review-avatar">${r.avatar}</div>
      <div class="review-body">
        <div class="review-top">
          <b>${r.author}</b>
          ${r.verified ? `<span class="verified">✓ Verified buyer</span>` : ""}
          <span class="review-date">${relDate(r.date)}</span>
        </div>
        <div class="review-stars">${starRow(r.rating)}</div>
        <h4 class="review-title">${escapeHtml(r.title)}</h4>
        <p class="review-text">${escapeHtml(r.body)}</p>
      </div>
    </article>`).join("");

  root.innerHTML = `
    <div class="section-head" style="text-align:left;margin-bottom:24px;max-width:none">
      <span class="eyebrow">Reviews</span>
      <h2>What families think</h2>
    </div>
    <div class="reviews-grid">
      <aside class="review-summary">
        <div class="rs-avg">${st.avg.toFixed(1)}</div>
        <div class="rs-stars">${starRow(Math.round(st.avg))}</div>
        <div class="rs-count">${st.count} review${st.count === 1 ? "" : "s"}</div>
        <div class="dist">${bars}</div>
        <button class="btn btn--block btn--sm" id="open-review" style="margin-top:16px">✍️ Write a review</button>
      </aside>
      <div>
        <form class="review-form form-card" id="pr-form" style="display:none" novalidate>
          <h3 style="margin-top:0">Review this product</h3>
          <div class="auth-msg" id="auth-msg"></div>
          <div class="field"><label>Your name</label>
            <input data-req id="pr-name" placeholder="Your name"><span class="err">Required</span></div>
          <div class="field"><label>Rating</label>
            <div class="star-pick" id="pr-stars">${"<span>★</span>".repeat(5)}</div>
          </div>
          <div class="field"><label>Title</label>
            <input data-req id="pr-title" placeholder="Sum it up in a few words"><span class="err">Required</span></div>
          <div class="field"><label>Your review</label>
            <textarea data-req id="pr-text" rows="4" placeholder="What did you and your little one think?"></textarea>
            <span class="err">Required</span></div>
          <div style="display:flex;gap:10px">
            <button class="btn" type="submit">Post review 💖</button>
            <button class="btn btn--ghost" type="button" id="cancel-review">Cancel</button>
          </div>
        </form>
        <div class="review-list" id="review-list">${reviewItems}</div>
      </div>
    </div>`;

  // toggle form
  const form = root.querySelector("#pr-form");
  root.querySelector("#open-review").onclick = () => {
    form.style.display = form.style.display === "none" ? "block" : "none";
    if (form.style.display === "block") form.scrollIntoView({ behavior: "smooth", block: "center" });
  };
  root.querySelector("#cancel-review").onclick = () => { form.style.display = "none"; };

  // star picker
  let stars = 5;
  const box = root.querySelector("#pr-stars");
  const paint = () => box.querySelectorAll("span").forEach((s, i) => s.textContent = i < stars ? "★" : "☆");
  box.querySelectorAll("span").forEach((s, i) => s.onclick = () => { stars = i + 1; paint(); });
  paint();

  form.addEventListener("submit", e => {
    e.preventDefault();
    if (!validateFields(form)) { authMsg("Please fill in every field."); return; }
    addReview(pid, {
      author: root.querySelector("#pr-name").value.trim(),
      rating: stars,
      title: root.querySelector("#pr-title").value.trim(),
      body: root.querySelector("#pr-text").value.trim(),
    });
    toast("💖 Thanks for your review!");
    // refresh the whole section + the headline rating
    renderReviews(pid);
    const line = document.querySelector("#pdp-root .card-rating");
    const ns = reviewStats(pid);
    if (line) line.innerHTML = `⭐ ${ns.avg.toFixed(1)} · <a href="#reviews-section" style="color:inherit;text-decoration:underline">${ns.count} review${ns.count === 1 ? "" : "s"}</a>`;
  });
}

function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

/* ---------------- CART ---------------- */
function init_cart() { renderCart(); }

function renderCart() {
  const root = document.getElementById("cart-root");
  const cart = getCart();
  const ids = Object.keys(cart);

  if (!ids.length) {
    root.innerHTML = `<div class="empty">
        <div class="big">🛍️</div>
        <h3>Your bag is empty</h3>
        <p>Let's fix that — there are treats waiting.</p>
        <a class="btn" href="shop.html">Start shopping</a>
      </div>`;
    return;
  }

  const items = ids.map(id => {
    const p = findProduct(id); const qty = cart[id];
    return `<div class="cart-item">
      <div class="cart-thumb ${p.bg}">${p.emoji}</div>
      <div>
        <h4>${p.name}</h4>
        <div class="meta">${catName(p.cat)} · ${ageName(p.age)} · ${money(p.price)} each</div>
        <div class="row2">
          <div class="qty qty--sm">
            <button aria-label="Decrease" data-cq="minus" data-id="${id}">−</button>
            <span>${qty}</span>
            <button aria-label="Increase" data-cq="plus" data-id="${id}">+</button>
          </div>
          <button class="remove" data-remove="${id}">Remove</button>
        </div>
      </div>
      <div class="line-price">${money(p.price * qty)}</div>
    </div>`;
  }).join("");

  const sub = cartSubtotal();
  const ship = sub >= 35 || sub === 0 ? 0 : 4.95;
  const total = sub + ship;

  root.innerHTML = `
    <div class="cart-layout">
      <div class="cart-list">${items}</div>
      <aside class="summary">
        <h3>Order summary</h3>
        <div class="sum-row"><span>Subtotal</span><span>${money(sub)}</span></div>
        <div class="sum-row"><span>Shipping</span><span>${ship === 0 ? "Free 🎉" : money(ship)}</span></div>
        ${sub < 35 ? `<div class="sum-row" style="color:var(--sky-deep);font-weight:700"><span>Add ${money(35 - sub)} for free shipping</span></div>` : ""}
        <div class="promo">
          <input type="text" id="promo" placeholder="Promo code">
          <button class="btn btn--sm btn--sky" id="promo-btn">Apply</button>
        </div>
        <div id="promo-msg" style="font-weight:700;font-size:.85rem;margin:-6px 0 10px"></div>
        <div class="sum-row total"><span>Total</span><span id="cart-total">${money(total)}</span></div>
        <a class="btn btn--block" href="checkout.html" style="margin-top:14px">Checkout →</a>
        <a class="btn btn--ghost btn--block" href="shop.html" style="margin-top:10px">Keep shopping</a>
      </aside>
    </div>`;

  // qty & remove
  root.querySelectorAll("[data-cq]").forEach(b => b.onclick = () => {
    const id = b.dataset.id; const cur = getCart()[id] || 1;
    setQty(id, b.dataset.cq === "plus" ? cur + 1 : cur - 1);
    renderCart();
  });
  root.querySelectorAll("[data-remove]").forEach(b => b.onclick = () => {
    removeFromCart(b.dataset.remove); toast("🗑️ Removed from bag"); renderCart();
  });

  // promo (demo): JELLY10 = 10% off
  const pBtn = root.querySelector("#promo-btn");
  pBtn.onclick = () => {
    const code = root.querySelector("#promo").value.trim().toUpperCase();
    const msg = root.querySelector("#promo-msg");
    if (code === "JELLY10") {
      const disc = sub * 0.1;
      root.querySelector("#cart-total").textContent = money(total - disc);
      msg.style.color = "var(--mint)"; msg.textContent = "✓ JELLY10 applied — 10% off!";
    } else if (code) {
      msg.style.color = "var(--pink-deep)"; msg.textContent = "That code isn't valid. Try JELLY10.";
    }
  };
}

/* ---------------- CHECKOUT ---------------- */
function init_checkout() {
  const root = document.getElementById("checkout-root");
  const cart = getCart();
  if (!Object.keys(cart).length) {
    root.innerHTML = `<div class="empty"><div class="big">🛍️</div><h3>Nothing to check out</h3>
      <p>Add a few treats first.</p><a class="btn" href="shop.html">Go shopping</a></div>`;
    return;
  }

  const sub = cartSubtotal();
  const ship = sub >= 35 ? 0 : 4.95;
  const tax = +(sub * 0.07).toFixed(2);
  const total = sub + ship + tax;

  const lines = Object.entries(cart).map(([id, q]) => {
    const p = findProduct(id);
    return `<div class="sum-row"><span>${p.emoji} ${p.name} × ${q}</span><span>${money(p.price * q)}</span></div>`;
  }).join("");

  root.innerHTML = `
    <div class="cart-layout">
      <div>
        <div class="form-card">
          <h3>📦 Delivery details</h3>
          <div class="field-grid">
            <div class="field"><label>First name</label><input data-req id="fn" placeholder="Alex"><span class="err">Required</span></div>
            <div class="field"><label>Last name</label><input data-req id="ln" placeholder="Rivera"><span class="err">Required</span></div>
            <div class="field full"><label>Email</label><input data-req data-email id="em" type="email" placeholder="you@email.com"><span class="err">Enter a valid email</span></div>
            <div class="field full"><label>Street address</label><input data-req id="ad" placeholder="12 Buttercup Lane"><span class="err">Required</span></div>
            <div class="field"><label>City</label><input data-req id="ci" placeholder="Springfield"><span class="err">Required</span></div>
            <div class="field"><label>ZIP / Postcode</label><input data-req id="zp" placeholder="00210"><span class="err">Required</span></div>
          </div>
        </div>

        <div class="form-card">
          <h3>💳 Payment</h3>
          <div class="pay-toggle" id="pay-toggle">
            <div class="pay-opt on" data-pay="card">💳 Card</div>
            <div class="pay-opt" data-pay="paypal">🅿️ PayPal</div>
            <div class="pay-opt" data-pay="apple">🍎 Apple Pay</div>
          </div>
          <div id="card-fields" style="margin-top:16px">
            <div class="field full"><label>Card number</label><input data-req id="cc" placeholder="4242 4242 4242 4242" inputmode="numeric"><span class="err">Required</span></div>
            <div class="field-grid">
              <div class="field"><label>Expiry</label><input data-req id="ex" placeholder="MM/YY"><span class="err">Required</span></div>
              <div class="field"><label>CVC</label><input data-req id="cv" placeholder="123" inputmode="numeric"><span class="err">Required</span></div>
            </div>
          </div>
          <p style="font-size:.82rem;color:var(--ink-soft);margin:10px 0 0">🔒 Demo checkout — please don't enter real card details.</p>
        </div>
      </div>

      <aside class="summary">
        <h3>Your order</h3>
        ${lines}
        <div class="sum-row" style="border-top:2px dashed var(--line);margin-top:8px;padding-top:12px"><span>Subtotal</span><span>${money(sub)}</span></div>
        <div class="sum-row"><span>Shipping</span><span>${ship === 0 ? "Free 🎉" : money(ship)}</span></div>
        <div class="sum-row"><span>Tax</span><span>${money(tax)}</span></div>
        <div class="sum-row total"><span>Total</span><span>${money(total)}</span></div>
        <button class="btn btn--block" id="place-order" style="margin-top:14px">Place order 🎁</button>
      </aside>
    </div>`;

  // payment toggle
  const toggle = root.querySelector("#pay-toggle");
  const cardFields = root.querySelector("#card-fields");
  toggle.addEventListener("click", e => {
    const o = e.target.closest("[data-pay]"); if (!o) return;
    toggle.querySelectorAll(".pay-opt").forEach(x => x.classList.remove("on"));
    o.classList.add("on");
    cardFields.style.display = o.dataset.pay === "card" ? "block" : "none";
  });

  root.querySelector("#place-order").onclick = () => {
    const usingCard = toggle.querySelector(".pay-opt.on").dataset.pay === "card";
    let ok = true;
    root.querySelectorAll("[data-req]").forEach(inp => {
      const field = inp.closest(".field");
      const isCard = cardFields.contains(inp);
      if (isCard && !usingCard) { field.classList.remove("invalid"); return; }
      let bad = !inp.value.trim();
      if (inp.dataset.email && inp.value && !/^\S+@\S+\.\S+$/.test(inp.value)) bad = true;
      field.classList.toggle("invalid", bad);
      if (bad) ok = false;
    });
    if (!ok) { toast("⚠️ Please check the highlighted fields"); return; }

    // build & save the order (real checkouts land in order history)
    const orderId = "JB-" + Math.floor(100000 + Math.random() * 900000);
    const order = {
      id: orderId,
      date: new Date().toISOString(),
      status: "Processing",
      items: Object.entries(getCart()).map(([id, q]) => ({ id, qty: q })),
      customer: {
        firstName: root.querySelector("#fn").value.trim(),
        lastName:  root.querySelector("#ln").value.trim(),
        email:     root.querySelector("#em").value.trim(),
        address:   root.querySelector("#ad").value.trim(),
        city:      root.querySelector("#ci").value.trim(),
        zip:       root.querySelector("#zp").value.trim(),
      },
      payment: toggle.querySelector(".pay-opt.on").dataset.pay,
    };
    saveOrder(order);

    write(STORE.cart, {});
    syncBadges();
    document.querySelector("main").innerHTML = `
      <div class="wrap"><div class="success">
        <div class="burst">🎉</div>
        <h1>Order placed!</h1>
        <p style="font-size:1.15rem;color:var(--ink-soft);max-width:46ch;margin:0 auto 6px">
          Thank you! A confirmation is on its way to your inbox. Your little one's treats will ship soon.</p>
        <p style="font-family:var(--font-display);font-weight:800;font-size:1.3rem;margin:18px 0">
          Order #${orderId}</p>
        <div style="display:flex;gap:12px;justify-content:center;flex-wrap:wrap">
          <a class="btn" href="invoice.html?id=${orderId}">View invoice</a>
          <a class="btn btn--sky" href="orders.html">Order history</a>
          <a class="btn btn--ghost" href="shop.html">Keep shopping</a>
        </div>
      </div></div>`;
    window.scrollTo({ top: 0, behavior: "smooth" });
  };
}

/* ---------------- WISHLIST ---------------- */
function init_wishlist() {
  const root = document.getElementById("wishlist-root");
  const draw = () => {
    const ids = getWish();
    if (!ids.length) {
      root.innerHTML = `<div class="empty">
        <div class="big">💖</div>
        <h3>No favorites yet</h3>
        <p>Tap the heart on anything you love to save it here.</p>
        <a class="btn" href="shop.html">Explore products</a>
      </div>`;
      return;
    }
    root.innerHTML = `<div class="product-grid">${ids.map(id => productCard(findProduct(id))).join("")}</div>`;
  };
  draw();
  // re-draw when a heart is toggled off from this page
  root.addEventListener("click", e => {
    if (e.target.closest("[data-wish]")) setTimeout(draw, 0);
  });
}

/* ---------------- CONTACT ---------------- */
function init_contact() {
  const form = document.getElementById("contact-form");
  if (!form) return;
  form.addEventListener("submit", e => {
    e.preventDefault();
    let ok = true;
    form.querySelectorAll("[data-req]").forEach(inp => {
      const field = inp.closest(".field");
      let bad = !inp.value.trim();
      if (inp.dataset.email && inp.value && !/^\S+@\S+\.\S+$/.test(inp.value)) bad = true;
      field.classList.toggle("invalid", bad);
      if (bad) ok = false;
    });
    if (!ok) { toast("⚠️ Please check the highlighted fields"); return; }
    form.innerHTML = `<div style="text-align:center;padding:20px">
        <div style="font-size:3.4rem" class="success"><span class="burst">💌</span></div>
        <h3 style="margin-top:10px">Message sent!</h3>
        <p style="color:var(--ink-soft)">Thanks for reaching out — we'll reply within one business day.</p>
      </div>`;
    toast("💌 Message sent!", "We'll be in touch");
  });
}

/* ---------------- shared: form validation ---------------- */
function validateFields(scope) {
  let ok = true;
  scope.querySelectorAll("[data-req]").forEach(inp => {
    const field = inp.closest(".field");
    let bad = !inp.value.trim();
    if (inp.dataset.email && inp.value && !/^\S+@\S+\.\S+$/.test(inp.value)) bad = true;
    if (inp.dataset.min && inp.value.length < +inp.dataset.min) bad = true;
    if (field) field.classList.toggle("invalid", bad);
    if (bad) ok = false;
  });
  return ok;
}
function authMsg(text, kind = "err") {
  const el = document.getElementById("auth-msg");
  if (!el) return;
  el.textContent = text;
  el.className = "auth-msg show " + (kind === "ok" ? "ok" : "bad");
}

/* ---------------- LOGIN ---------------- */
function init_login() {
  const form = document.getElementById("login-form");
  if (!form) return;
  const next = new URLSearchParams(location.search).get("next");
  form.addEventListener("submit", async e => {
    e.preventDefault();
    if (!validateFields(form)) { authMsg("Please fill in the highlighted fields."); return; }
    const res = await login({
      email: form.querySelector("#li-email").value,
      password: form.querySelector("#li-pass").value,
    });
    if (!res.ok) { authMsg(res.error); return; }
    authMsg(`Welcome back, ${res.user.firstName}!`, "ok");
    toast("🎉 Welcome back!", res.user.firstName);
    setTimeout(() => location.href = next || "orders.html", 600);
  });
}

/* ---------------- REGISTER ---------------- */
function init_register() {
  const form = document.getElementById("register-form");
  if (!form) return;
  form.addEventListener("submit", async e => {
    e.preventDefault();
    if (!validateFields(form)) { authMsg("Please check the highlighted fields."); return; }
    const pass = form.querySelector("#rg-pass").value;
    const pass2 = form.querySelector("#rg-pass2").value;
    if (pass !== pass2) {
      form.querySelector("#rg-pass2").closest(".field").classList.add("invalid");
      authMsg("Those passwords don't match."); return;
    }
    const res = await register({
      firstName: form.querySelector("#rg-fn").value.trim(),
      lastName:  form.querySelector("#rg-ln").value.trim(),
      email:     form.querySelector("#rg-email").value,
      password:  pass,
    });
    if (!res.ok) { authMsg(res.error); return; }
    authMsg(`Account created — welcome, ${res.user.firstName}!`, "ok");
    toast("✨ Account created!", "Welcome to Jellybean");
    setTimeout(() => location.href = "orders.html", 700);
  });
}

/* ---------------- FORGOT PASSWORD ---------------- */
window["init_forgot-password"] = function () {
  const form = document.getElementById("forgot-form");
  if (!form) return;
  form.addEventListener("submit", async e => {
    e.preventDefault();
    if (!validateFields(form)) { authMsg("Enter the email on your account."); return; }
    const email = form.querySelector("#fp-email").value;
    const res = await requestPasswordReset(email);
    if (!res.ok) { authMsg(res.error); return; }
    document.getElementById("forgot-card").innerHTML = `
      <div style="text-align:center">
        <div style="font-size:3.4rem" class="success"><span class="burst">📬</span></div>
        <h2 style="margin-top:8px">Check your inbox</h2>
        <p style="color:var(--ink-soft)">If <b>${email}</b> has an account, a reset link is on its way.
        The link expires in 30 minutes.</p>
        <a class="btn btn--block" href="login.html" style="margin-top:12px">Back to log in</a>
      </div>`;
  });
};

/* ---------------- TESTIMONIALS ---------------- */
function init_testimonials() {
  const grid = document.getElementById("testi-grid");
  if (grid) grid.innerHTML = TESTIMONIALS.map(t => `
    <figure class="testi-card">
      <div class="stars">${"★".repeat(t.rating)}${"☆".repeat(5 - t.rating)}</div>
      <blockquote>"${t.quote}"</blockquote>
      <figcaption>
        <span class="testi-avatar">${t.avatar}</span>
        <span><b>${t.name}</b><br><span class="testi-sub">${t.role}</span></span>
      </figcaption>
    </figure>`).join("");

  // review form (demo)
  const form = document.getElementById("review-form");
  if (form) {
    let stars = 5;
    const starBox = form.querySelector("#star-pick");
    const paint = () => starBox.querySelectorAll("span").forEach((s, i) =>
      s.textContent = i < stars ? "★" : "☆");
    starBox.querySelectorAll("span").forEach((s, i) =>
      s.onclick = () => { stars = i + 1; paint(); });
    paint();
    form.addEventListener("submit", e => {
      e.preventDefault();
      if (!validateFields(form)) { toast("⚠️ Please check the highlighted fields"); return; }
      form.reset();
      paint();
      toast("💖 Thanks for your review!", "It'll appear after moderation");
    });
  }
}

/* ---------------- ORDER HISTORY ---------------- */
function init_orders() {
  const root = document.getElementById("orders-root");
  if (!isLoggedIn()) {
    root.innerHTML = `<div class="empty">
      <div class="big">🔒</div>
      <h3>Please log in to view your orders</h3>
      <p>Sign in to see your order history, track shipments and download invoices.</p>
      <div style="display:flex;gap:10px;justify-content:center;flex-wrap:wrap">
        <a class="btn" href="login.html?next=orders.html">Log in</a>
        <a class="btn btn--ghost" href="register.html">Create account</a>
      </div></div>`;
    return;
  }

  const draw = () => {
    const orders = getOrders();
    if (!orders.length) {
      root.innerHTML = `<div class="empty"><div class="big">📦</div>
        <h3>No orders yet</h3><p>When you place an order it'll show up here.</p>
        <a class="btn" href="shop.html">Start shopping</a></div>`;
      return;
    }
    root.innerHTML = `<div class="orders-list">${orders.map(orderCard).join("")}</div>`;
    root.querySelectorAll("[data-reorder]").forEach(b => b.onclick = () => {
      const o = findOrder(b.dataset.reorder);
      o.items.forEach(it => { const c = getCart(); c[it.id] = (c[it.id] || 0) + it.qty; write(STORE.cart, c); });
      syncBadges();
      toast("🛍️ Added to your bag", "Reordered");
    });
  };
  draw();
}

function orderCard(o) {
  const t = orderTotals(o);
  const count = o.items.reduce((s, it) => s + it.qty, 0);
  const d = new Date(o.date).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
  const thumbs = o.items.slice(0, 5).map(it => {
    const p = findProduct(it.id); return `<span class="o-thumb ${p.bg}" title="${p.name}">${p.emoji}</span>`;
  }).join("");
  const more = o.items.length > 5 ? `<span class="o-thumb o-more">+${o.items.length - 5}</span>` : "";
  return `
    <article class="order-card">
      <div class="order-head">
        <div>
          <span class="order-id">Order #${o.id}</span>
          <span class="order-date">Placed ${d}</span>
        </div>
        <span class="status status--${o.status.toLowerCase()}">${o.status}</span>
      </div>
      <div class="order-body">
        <div class="o-thumbs">${thumbs}${more}</div>
        <div class="order-meta">${count} item${count > 1 ? "s" : ""} · <b>${money(t.total)}</b></div>
      </div>
      <div class="order-actions">
        <a class="btn btn--sm" href="invoice.html?id=${o.id}">🧾 View invoice</a>
        <button class="btn btn--sm btn--ghost" data-reorder="${o.id}">Reorder</button>
      </div>
    </article>`;
}

/* ---------------- INVOICE ---------------- */
function init_invoice() {
  const id = new URLSearchParams(location.search).get("id");
  const root = document.getElementById("invoice-root");
  const o = findOrder(id);
  if (!o) {
    root.innerHTML = `<div class="empty"><div class="big">🧾</div>
      <h3>Invoice not found</h3><p>We couldn't find that order.</p>
      <a class="btn" href="orders.html">Back to orders</a></div>`;
    return;
  }
  document.title = `Invoice ${o.id} · Jellybean`;
  const t = orderTotals(o);
  const d = new Date(o.date).toLocaleDateString(undefined, { year: "numeric", month: "long", day: "numeric" });
  const payLabel = { card: "Card", paypal: "PayPal", apple: "Apple Pay" }[o.payment] || o.payment;

  const rows = o.items.map(it => {
    const p = findProduct(it.id);
    return `<tr>
      <td><span class="inv-emoji ${p.bg}">${p.emoji}</span> ${p.name}</td>
      <td class="num">${money(p.price)}</td>
      <td class="num">${it.qty}</td>
      <td class="num">${money(p.price * it.qty)}</td>
    </tr>`;
  }).join("");

  root.innerHTML = `
    <div class="invoice" id="invoice-sheet">
      <div class="inv-top">
        <div class="inv-brand"><span class="bean">🍬</span> Jellybean
          <div class="inv-brand-sub">Little things for little ones</div>
        </div>
        <div class="inv-title">
          <h1>Invoice</h1>
          <div class="inv-num">#${o.id}</div>
          <span class="status status--${o.status.toLowerCase()}">${o.status}</span>
        </div>
      </div>

      <div class="inv-parties">
        <div>
          <span class="inv-label">Billed to</span>
          <b>${o.customer.firstName} ${o.customer.lastName}</b><br>
          ${o.customer.address}<br>
          ${o.customer.city}, ${o.customer.zip}<br>
          ${o.customer.email}
        </div>
        <div>
          <span class="inv-label">Details</span>
          <div class="inv-kv"><span>Invoice date</span><b>${d}</b></div>
          <div class="inv-kv"><span>Payment</span><b>${payLabel}</b></div>
          <div class="inv-kv"><span>Order status</span><b>${o.status}</b></div>
        </div>
      </div>

      <table class="inv-table">
        <thead><tr><th>Item</th><th class="num">Price</th><th class="num">Qty</th><th class="num">Total</th></tr></thead>
        <tbody>${rows}</tbody>
      </table>

      <div class="inv-totals">
        <div class="inv-kv"><span>Subtotal</span><b>${money(t.subtotal)}</b></div>
        ${t.discount ? `<div class="inv-kv"><span>Discount</span><b>−${money(t.discount)}</b></div>` : ""}
        <div class="inv-kv"><span>Shipping</span><b>${t.shipping === 0 ? "Free" : money(t.shipping)}</b></div>
        <div class="inv-kv"><span>Tax (7%)</span><b>${money(t.tax)}</b></div>
        <div class="inv-kv inv-grand"><span>Total paid</span><b>${money(t.total)}</b></div>
      </div>

      <p class="inv-foot">Thank you for shopping with Jellybean 💖 &nbsp;·&nbsp; Questions? hello@jellybean.demo<br>
      This is a demo invoice — no real payment was processed.</p>
    </div>

    <div class="inv-actions no-print">
      <a class="link-back" href="orders.html">← Back to orders</a>
      <button class="btn" onclick="window.print()">🖨️ Print / Save PDF</button>
    </div>`;
}

/* ---------------- shared: newsletter ---------------- */
function wireNewsletter() {
  const form = document.getElementById("news-form");
  if (!form) return;
  form.addEventListener("submit", e => {
    e.preventDefault();
    const input = form.querySelector("input");
    if (/^\S+@\S+\.\S+$/.test(input.value)) {
      toast("💌 You're on the list!", "Welcome to the club");
      input.value = "";
    } else {
      toast("⚠️ Enter a valid email");
    }
  });
}
