/* ============================================================
   Jellybean — auth module
   ------------------------------------------------------------
   BACKEND SEAM: every function below is written so the
   localStorage block can be swapped for a real API call.
   Look for the "// → API:" comments — that's exactly where a
   fetch('/api/...') goes when the backend is ready. The rest of
   the app only talks to auth through these functions, so nothing
   else has to change.
   ============================================================ */

const AUTH_KEYS = {
  users:   "jb_users",     // demo user store — moves server-side later
  session: "jb_session",   // current signed-in user's email
};

/* small helpers (demo only) */
function _readUsers() {
  try { return JSON.parse(localStorage.getItem(AUTH_KEYS.users)) || {}; }
  catch { return {}; }
}
function _writeUsers(u) { localStorage.setItem(AUTH_KEYS.users, JSON.stringify(u)); }

/* ------------------------------------------------------------
   register({ firstName, lastName, email, password })
   Returns { ok, error?, user? }
   ------------------------------------------------------------ */
async function register({ firstName, lastName, email, password }) {
  email = (email || "").trim().toLowerCase();

  // → API: POST /api/register  { firstName, lastName, email, password }
  //   and return the created user + set an auth cookie/token.
  const users = _readUsers();
  if (users[email]) return { ok: false, error: "An account with that email already exists." };
  const user = { firstName, lastName, email, password }; // demo: never store raw passwords in production
  users[email] = user;
  _writeUsers(users);
  _setSession(email);
  return { ok: true, user: _publicUser(user) };
}

/* ------------------------------------------------------------
   login({ email, password }) → { ok, error?, user? }
   ------------------------------------------------------------ */
async function login({ email, password }) {
  email = (email || "").trim().toLowerCase();

  // → API: POST /api/login  { email, password }  → returns user + token
  const users = _readUsers();
  const user = users[email];
  if (!user || user.password !== password) {
    return { ok: false, error: "That email and password don't match." };
  }
  _setSession(email);
  return { ok: true, user: _publicUser(user) };
}

/* ------------------------------------------------------------
   requestPasswordReset(email) → { ok, error? }
   ------------------------------------------------------------ */
async function requestPasswordReset(email) {
  email = (email || "").trim().toLowerCase();
  if (!/^\S+@\S+\.\S+$/.test(email)) return { ok: false, error: "Enter a valid email address." };

  // → API: POST /api/password/forgot  { email }
  //   The backend emails a reset link. We always report success
  //   so the UI never reveals whether an email is registered.
  return { ok: true };
}

/* ------------------------------------------------------------
   session helpers
   ------------------------------------------------------------ */
function _setSession(email) { localStorage.setItem(AUTH_KEYS.session, email); }

function currentUser() {
  // → API: GET /api/me  (validate token) — for now read local session
  const email = localStorage.getItem(AUTH_KEYS.session);
  if (!email) return null;
  const user = _readUsers()[email];
  return user ? _publicUser(user) : null;
}

function isLoggedIn() { return !!currentUser(); }

function logout() {
  // → API: POST /api/logout
  localStorage.removeItem(AUTH_KEYS.session);
}

/* Redirect guests to login, remembering where they wanted to go. */
function requireAuth() {
  if (isLoggedIn()) return true;
  const here = location.pathname.split("/").pop() + location.search;
  location.href = "login.html?next=" + encodeURIComponent(here);
  return false;
}

function _publicUser(u) { return { firstName: u.firstName, lastName: u.lastName, email: u.email }; }
