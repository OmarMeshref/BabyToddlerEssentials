/* Jellybean — sample catalogue.
   Emoji stand in for product photos so the site works fully offline. */

const CATEGORIES = [
  { id: "toys",     name: "Toys",     emoji: "🧸" },
  { id: "clothing", name: "Clothing", emoji: "👕" },
  { id: "nursery",  name: "Nursery",  emoji: "🌙" },
  { id: "books",    name: "Books",    emoji: "📚" },
  { id: "feeding",  name: "Feeding",  emoji: "🍼" },
  { id: "bath",     name: "Bath",     emoji: "🦆" },
];

const AGES = [
  { id: "0-12m", name: "0–12 mo" },
  { id: "1-2y",  name: "1–2 yrs" },
  { id: "3-4y",  name: "3–4 yrs" },
  { id: "5y",    name: "5 yrs +" },
];

const PRODUCTS = [
  { id: "p01", name: "Snuggle Bear Plush", emoji: "🧸", cat: "toys", age: "0-12m", price: 24, was: 30, rating: 4.9, reviews: 212, bg: "bg-a",
    desc: "A cloud-soft teddy with an extra-huggable belly and embroidered eyes — no small parts, so it's safe from day one.",
    features: ["Machine washable", "Hypoallergenic filling", "No small parts", "OEKO-TEX certified fabric"] },
  { id: "p02", name: "Rainbow Stacking Rings", emoji: "🌈", cat: "toys", age: "0-12m", price: 18, rating: 4.8, reviews: 176, bg: "bg-b",
    desc: "Seven wobbly rings that teach size, color and order. Built to be gummed, dropped and stacked a thousand times.",
    features: ["BPA-free silicone", "Encourages motor skills", "Wipe-clean", "Wobble base"] },
  { id: "p03", name: "Wooden Choo-Choo Train", emoji: "🚂", cat: "toys", age: "1-2y", price: 32, was: 39, rating: 4.7, reviews: 98, bg: "bg-c",
    desc: "A five-car train with magnetic couplings and chunky wheels sized for little hands. Sustainably sourced beech wood.",
    features: ["FSC-certified wood", "Magnetic couplings", "Non-toxic paint", "Rounded edges"] },
  { id: "p04", name: "Chunky Shape Puzzle", emoji: "🧩", cat: "toys", age: "1-2y", price: 21, rating: 4.6, reviews: 134, bg: "bg-d",
    desc: "Six fat, grippable pieces that pop into their matching homes with a satisfying click. Great first puzzle.",
    features: ["Grip knobs", "Self-correcting slots", "Solid wood", "Ages 1+"] },
  { id: "p05", name: "Little Artist Paint Set", emoji: "🎨", cat: "toys", age: "3-4y", price: 26, rating: 4.8, reviews: 87, bg: "bg-e",
    desc: "Washable, non-toxic paints in ten poppy colors with a chunky brush and a spill-proof palette. Aprons optional (but wise).",
    features: ["Washes off skin & clothes", "Non-toxic", "10 colors", "Spill-proof pots"] },
  { id: "p06", name: "Roar Dino Figure Pack", emoji: "🦖", cat: "toys", age: "3-4y", price: 19, was: 24, rating: 4.9, reviews: 260, bg: "bg-b",
    desc: "A herd of six soft-touch dinosaurs with bendy tails and friendly faces. Prehistoric adventures, zero sharp edges.",
    features: ["Soft-touch finish", "Bendy tails", "6 figures", "Phthalate-free"] },
  { id: "p07", name: "Organic Cotton Onesie", emoji: "👕", cat: "clothing", age: "0-12m", price: 16, rating: 4.9, reviews: 340, bg: "bg-a",
    desc: "Buttery GOTS-certified cotton with easy shoulder snaps and a fold-over mitten cuff. Comes in a jellybean print.",
    features: ["GOTS organic cotton", "Snap shoulders", "Tag-free", "Machine washable"] },
  { id: "p08", name: "Fuzzy Star Socks (3 pk)", emoji: "🧦", cat: "clothing", age: "1-2y", price: 12, rating: 4.5, reviews: 145, bg: "bg-c",
    desc: "Three pairs of grippy-sole socks that actually stay on wiggly feet. Non-slip stars keep new walkers steady.",
    features: ["Non-slip soles", "Stay-put cuff", "3 pairs", "Soft combed cotton"] },
  { id: "p09", name: "Puddle-Jump Rain Boots", emoji: "🥾", cat: "clothing", age: "3-4y", price: 28, was: 34, rating: 4.7, reviews: 76, bg: "bg-b",
    desc: "Bright, bendy boots made for the deepest puddles. Pull-on loops and a cushioned insole for all-day splashing.",
    features: ["Waterproof", "Pull-on loops", "Cushioned insole", "Reflective heel"] },
  { id: "p10", name: "Cozy Cloud Sleep Sack", emoji: "🛏️", cat: "nursery", age: "0-12m", price: 34, rating: 4.9, reviews: 190, bg: "bg-e",
    desc: "A wearable blanket that keeps tiny sleepers snug without loose bedding. Two-way zip for midnight changes.",
    features: ["Two-way zip", "TOG 1.0 rated", "Breathable weave", "Hip-healthy cut"] },
  { id: "p11", name: "Starlight Nursery Nightlight", emoji: "🌙", cat: "nursery", age: "0-12m", price: 29, was: 36, rating: 4.8, reviews: 156, bg: "bg-b",
    desc: "Projects a slow drift of stars in warm amber light, with a timer that fades as your little one settles.",
    features: ["Amber sleep-safe light", "30/60 min timer", "USB rechargeable", "Whisper-quiet"] },
  { id: "p12", name: "Twinkle Crib Mobile", emoji: "🎐", cat: "nursery", age: "0-12m", price: 38, rating: 4.6, reviews: 64, bg: "bg-a",
    desc: "Hand-felted clouds and moons that turn to a soft lullaby. Winds down gently — no batteries, no beeps.",
    features: ["Wind-up lullaby", "Hand-felted", "Fits most cribs", "No batteries"] },
  { id: "p13", name: "Bedtime Story Board Books", emoji: "📚", cat: "books", age: "0-12m", price: 22, rating: 4.9, reviews: 288, bg: "bg-c",
    desc: "A set of four chunky board books with high-contrast art and rounded corners — built to survive enthusiastic reading.",
    features: ["4-book set", "Rounded corners", "Wipe-clean pages", "High-contrast art"] },
  { id: "p14", name: "Touch & Feel Animals", emoji: "📖", cat: "books", age: "1-2y", price: 14, was: 18, rating: 4.8, reviews: 205, bg: "bg-d",
    desc: "Fuzzy, bumpy, silky textures on every page. Little fingers explore while you name each friendly animal.",
    features: ["Textured pages", "Sturdy board", "Ages 1+", "First-words friendly"] },
  { id: "p15", name: "Grow-With-Me Sippy Cup", emoji: "🥤", cat: "feeding", age: "1-2y", price: 15, rating: 4.7, reviews: 320, bg: "bg-b",
    desc: "Leak-proof from high chair to backpack, with a soft spout and removable handles that grow with your toddler.",
    features: ["100% leak-proof", "Removable handles", "BPA-free", "Dishwasher safe"] },
  { id: "p16", name: "Bamboo Bowl & Spoon Set", emoji: "🥣", cat: "feeding", age: "0-12m", price: 23, rating: 4.8, reviews: 142, bg: "bg-d",
    desc: "A suction-base bowl that stays put through the flingiest of meals, plus two soft-tip spoons sized for gums.",
    features: ["Strong suction base", "Soft-tip spoons", "Bamboo fibre", "Dishwasher safe"] },
  { id: "p17", name: "First Feeding Bottle (2 pk)", emoji: "🍼", cat: "feeding", age: "0-12m", price: 20, was: 25, rating: 4.6, reviews: 118, bg: "bg-a",
    desc: "Anti-colic vented bottles with a slow-flow nipple shaped for an easy latch. Two bottles, fewer 2am scrambles.",
    features: ["Anti-colic vent", "Slow-flow nipple", "BPA-free", "Easy-clean wide neck"] },
  { id: "p18", name: "Splashy Duck Bath Toys", emoji: "🦆", cat: "bath", age: "1-2y", price: 17, rating: 4.9, reviews: 233, bg: "bg-c",
    desc: "A family of squirty ducks with sealed bottoms — no hidden water, no sneaky mold. Bath time, sorted.",
    features: ["Mold-free sealed design", "Squirt-free option", "Floats upright", "Set of 4"] },
  { id: "p19", name: "Hooded Bath Towel", emoji: "🛁", cat: "bath", age: "0-12m", price: 25, rating: 4.8, reviews: 174, bg: "bg-e",
    desc: "An oversized bamboo-cotton towel with a snug bunny-ear hood to wrap up warm, wiggly post-bath bundles.",
    features: ["Bamboo-cotton blend", "Extra-absorbent", "Bunny-ear hood", "Gets softer with washing"] },
  { id: "p20", name: "Tear-Free Bubble Wash", emoji: "🧴", cat: "bath", age: "1-2y", price: 13, was: 16, rating: 4.7, reviews: 96, bg: "bg-b",
    desc: "A gentle 2-in-1 hair and body wash with a light pear scent. Tear-free formula that plays nice with sensitive skin.",
    features: ["Tear-free", "Dermatologist tested", "2-in-1 wash", "Plant-based"] },
];

function findProduct(id) { return PRODUCTS.find(p => p.id === id); }
function catName(id) { const c = CATEGORIES.find(c => c.id === id); return c ? c.name : id; }
function ageName(id) { const a = AGES.find(a => a.id === id); return a ? a.name : id; }

/* ============================================================
   Gallery: how many placeholder slots to show on the product
   page when a product has no real image URLs yet. To use real
   photos later, add an `images` array to a product, e.g.
     images: ["snuggle-front.jpg", "snuggle-side.jpg"]
   and the gallery will render them instead of placeholders.
   ============================================================ */
const GALLERY_SLOTS = 4;
const GALLERY_LABELS = ["Main", "Side", "Detail", "In use"];

/* ---------------- Site-wide testimonials ----------------
   These are about the whole Jellybean experience — service,
   shipping, safety, packaging — not any single product.
   Per-product reviews live in the review system further below.
   -------------------------------------------------------- */
const TESTIMONIALS = [
  { name: "Maya T.",    role: "Verified customer · 6 orders", avatar: "👩🏽", rating: 5,
    quote: "Jellybean is the only shop I fully trust for the kids. Every order arrives fast, beautifully packed, and exactly as described." },
  { name: "Daniel R.",  role: "Verified customer · 3 orders", avatar: "👨🏻", rating: 5,
    quote: "Customer service actually answers, and quickly. They replaced a damaged item within two days, no questions asked." },
  { name: "Priya K.",   role: "Verified customer",            avatar: "👩🏾", rating: 5,
    quote: "What sold me is the safety testing. Knowing everything is properly checked means I can shop here without second-guessing." },
  { name: "Sofia L.",   role: "Verified customer · twins",    avatar: "👩🏼", rating: 4,
    quote: "Prices are fair and the quality is a step above the big stores. The whole site is a joy to browse, too." },
  { name: "Marcus B.",  role: "Verified customer · 4 orders", avatar: "🧔🏾", rating: 5,
    quote: "Free shipping over $35 and it genuinely turns up in a day or two. Packaging is plastic-free, which we love." },
  { name: "Aisha M.",   role: "Verified customer",            avatar: "🧕🏽", rating: 5,
    quote: "Returns were painless — one tap, a free label, refunded in days. That's rare and it keeps me coming back." },
  { name: "Tom & Ella", role: "New parents",                  avatar: "👩🏻", rating: 5,
    quote: "As first-time parents we were overwhelmed. Jellybean's stage-by-stage layout made choosing so much less scary." },
  { name: "Grace O.",   role: "Verified customer · 2 orders", avatar: "👩🏿", rating: 4,
    quote: "Lovely curation — I never feel bombarded with junk. Everything here feels chosen with actual care." },
];

/* ---------------- Seed orders (demo history) ----------------
   Real checkouts get appended to the same store (jb_orders).
   ------------------------------------------------------------ */
function daysAgoISO(d) { const t = new Date(); t.setDate(t.getDate() - d); return t.toISOString(); }

const SEED_ORDERS = [
  {
    id: "JB-100482", date: daysAgoISO(42), status: "Delivered", seeded: true,
    items: [ { id: "p01", qty: 1 }, { id: "p13", qty: 1 } ],
    customer: { firstName: "Sample", lastName: "Family", email: "hello@jellybean.demo",
                address: "12 Buttercup Lane", city: "Springfield", zip: "00210" },
    payment: "card",
  },
  {
    id: "JB-100731", date: daysAgoISO(15), status: "Shipped", seeded: true,
    items: [ { id: "p06", qty: 2 }, { id: "p18", qty: 1 }, { id: "p08", qty: 1 } ],
    customer: { firstName: "Sample", lastName: "Family", email: "hello@jellybean.demo",
                address: "12 Buttercup Lane", city: "Springfield", zip: "00210" },
    payment: "paypal",
  },
  {
    id: "JB-100905", date: daysAgoISO(3), status: "Processing", seeded: true,
    items: [ { id: "p10", qty: 1 }, { id: "p11", qty: 1 } ],
    customer: { firstName: "Sample", lastName: "Family", email: "hello@jellybean.demo",
                address: "12 Buttercup Lane", city: "Springfield", zip: "00210" },
    payment: "apple",
  },
];

/* ============================================================
   Per-product reviews
   ------------------------------------------------------------
   Each product gets its own stable set of seed reviews, built
   deterministically from its id so they don't shuffle on every
   load. Shoppers can add their own reviews on the product page
   (stored in localStorage and merged in — see app.js).
   ============================================================ */
const REVIEW_AUTHORS = [
  ["Emma W.","👩🏼"], ["Liam H.","👨🏽"], ["Noah P.","🧔🏻"], ["Olivia S.","👩🏾"],
  ["Ava R.","👩🏻"], ["Sophia L.","🧕🏽"], ["Jack M.","👨🏻"], ["Mia C.","👩🏿"],
  ["Ben T.","🧔🏾"], ["Chloe D.","👩🏽"], ["Ryan K.","👨🏾"], ["Zoe A.","👩🏼"],
  ["Hannah B.","👩🏻"], ["Leo G.","👨🏼"], ["Isla F.","👩🏾"], ["Owen J.","🧔🏻"],
];

/* rating-tagged snippets; {t: title, b: body} — kept generic so
   they read naturally for any product */
const REVIEW_SNIPPETS = {
  5: [
    { t: "Absolutely love it", b: "Even better in person. The quality is lovely and it went down a treat — we'd buy again in a heartbeat." },
    { t: "Worth every penny", b: "Beautifully made and clearly built to last. My little one reaches for it every single day." },
    { t: "New favourite", b: "This has quickly become the most-used thing in our house. Feels safe, sturdy and thoughtfully designed." },
    { t: "Fast and perfect", b: "Arrived next day, packaged with care, and exactly as pictured. Couldn't be happier with it." },
    { t: "Grandkids adore it", b: "Bought as a gift and it was a huge hit. Sturdy enough to survive very enthusiastic play." },
    { t: "So well made", b: "You can feel the quality straight away. No rough edges, no odd smells — just lovely craftsmanship." },
  ],
  4: [
    { t: "Really happy", b: "Great quality and does exactly what we hoped. Knocked one star only because I wish it came in more colours." },
    { t: "Lovely, minor niggle", b: "We love it overall. Slightly smaller than I pictured, but the quality more than makes up for it." },
    { t: "Good value", b: "Solid and well finished for the price. Cleaning is easy, which matters a lot in this house." },
    { t: "Would recommend", b: "Does the job nicely and looks great. Delivery was quick and everything was well packed." },
  ],
  3: [
    { t: "Good, not perfect", b: "It's fine and my child likes it, but I expected it to feel a touch more premium for the price." },
    { t: "Does the job", b: "No complaints about quality, it just didn't wow us the way some other bits from here have." },
  ],
};

function _hash(str) {
  let h = 2166136261;
  for (let i = 0; i < str.length; i++) { h ^= str.charCodeAt(i); h = Math.imul(h, 16777619); }
  return h >>> 0;
}
function _rng(seed) {
  let s = seed >>> 0;
  return () => { s = (Math.imul(s, 1664525) + 1013904223) >>> 0; return s / 4294967296; };
}

/* Stable seed reviews for a product, derived from its id. */
function seedReviews(productId) {
  const rnd = _rng(_hash(productId));
  const count = 3 + Math.floor(rnd() * 4); // 3–6 reviews
  const usedAuthors = new Set();
  const reviews = [];
  for (let i = 0; i < count; i++) {
    // rating distribution weighted toward 5★
    const roll = rnd();
    const rating = roll < 0.62 ? 5 : roll < 0.9 ? 4 : 3;
    const pool = REVIEW_SNIPPETS[rating];
    const snip = pool[Math.floor(rnd() * pool.length)];
    let ai = Math.floor(rnd() * REVIEW_AUTHORS.length);
    while (usedAuthors.has(ai)) ai = (ai + 1) % REVIEW_AUTHORS.length;
    usedAuthors.add(ai);
    const [author, avatar] = REVIEW_AUTHORS[ai];
    const daysAgo = 6 + Math.floor(rnd() * 320);
    reviews.push({
      id: productId + "-s" + i,
      author, avatar, rating,
      title: snip.t, body: snip.b,
      date: daysAgoISO(daysAgo),
      verified: true,
      seeded: true,
    });
  }
  return reviews;
}
