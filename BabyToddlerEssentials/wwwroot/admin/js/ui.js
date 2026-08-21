/* ==========================================================================
   LittleNest Admin — UI helpers (toasts, modals, dropdowns, sidebar)
   ========================================================================== */

const LN_UI = (function () {

  /* ---------------- Toasts ---------------- */

  function ensureToastStack() {
    let stack = document.querySelector('.toast-stack');
    if (!stack) {
      stack = document.createElement('div');
      stack.className = 'toast-stack';
      stack.setAttribute('aria-live', 'polite');
      document.body.appendChild(stack);
    }
    return stack;
  }

  const TOAST_ICONS = { success: 'fa-circle-check', error: 'fa-circle-exclamation', info: 'fa-circle-info' };

  function toast(message, type = 'success', duration = 3200) {
    const stack = ensureToastStack();
    const el = document.createElement('div');
    el.className = `toast toast--${type}`;
    el.innerHTML = `
      <i class="fa-solid ${TOAST_ICONS[type] || TOAST_ICONS.success}"></i>
      <span>${message}</span>
      <button class="toast__close" aria-label="Dismiss notification"><i class="fa-solid fa-xmark"></i></button>
    `;
    stack.appendChild(el);
    requestAnimationFrame(() => el.classList.add('is-visible'));

    const remove = () => {
      el.classList.remove('is-visible');
      setTimeout(() => el.remove(), 220);
    };
    el.querySelector('.toast__close').addEventListener('click', remove);
    setTimeout(remove, duration);
  }

  /* ---------------- Modals ---------------- */

  function openModal(id) {
    const overlay = document.getElementById(id);
    if (!overlay) return;
    overlay.classList.add('is-open');
    document.body.style.overflow = 'hidden';
  }

  function closeModal(id) {
    const overlay = document.getElementById(id);
    if (!overlay) return;
    overlay.classList.remove('is-open');
    document.body.style.overflow = '';
  }

  function initModalDismiss() {
    document.querySelectorAll('.modal-overlay').forEach((overlay) => {
      overlay.addEventListener('click', (e) => {
        if (e.target === overlay) closeModal(overlay.id);
      });
    });
    document.querySelectorAll('[data-modal-close]').forEach((btn) => {
      btn.addEventListener('click', () => closeModal(btn.closest('.modal-overlay').id));
    });
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        document.querySelectorAll('.modal-overlay.is-open').forEach((o) => closeModal(o.id));
      }
    });
  }

  /* ---------------- Dropdowns ---------------- */

  function initDropdowns() {
    const triggers = document.querySelectorAll('[data-dropdown-trigger]');
    triggers.forEach((trigger) => {
      const panelId = trigger.getAttribute('data-dropdown-trigger');
      const panel = document.getElementById(panelId);
      if (!panel) return;
      trigger.addEventListener('click', (e) => {
        e.stopPropagation();
        const willOpen = !panel.classList.contains('is-open');
        closeAllDropdowns();
        if (willOpen) panel.classList.add('is-open');
      });
    });
    document.addEventListener('click', closeAllDropdowns);
  }

  function closeAllDropdowns() {
    document.querySelectorAll('.dropdown-panel.is-open').forEach((p) => p.classList.remove('is-open'));
    document.querySelectorAll('.action-menu.is-open').forEach((p) => p.classList.remove('is-open'));
  }

  /* ---------------- Row action menus (delegated) ---------------- */

  function initActionMenus() {
    document.addEventListener('click', (e) => {
      const btn = e.target.closest('.action-menu-btn');
      if (btn) {
        e.stopPropagation();
        const menu = btn.nextElementSibling;
        const wasOpen = menu.classList.contains('is-open');
        closeAllDropdowns();
        if (!wasOpen) menu.classList.add('is-open');
      }
    });
  }

  /* ---------------- Sidebar (mobile) ---------------- */

  function initSidebar() {
    const sidebar = document.getElementById('sidebar');
    const backdrop = document.getElementById('sidebarBackdrop');
    const openBtn = document.getElementById('menuToggle');
    if (!sidebar || !backdrop || !openBtn) return;

    const open = () => { sidebar.classList.add('is-open'); backdrop.classList.add('is-open'); };
    const close = () => { sidebar.classList.remove('is-open'); backdrop.classList.remove('is-open'); };

    openBtn.addEventListener('click', open);
    backdrop.addEventListener('click', close);
    sidebar.querySelectorAll('.nav-item').forEach((item) => item.addEventListener('click', close));
  }

  /* ---------------- Form validation ---------------- */

  function validateField(field, rules) {
    const errorEl = field.parentElement.querySelector('.field-error');
    let message = '';

    for (const rule of rules) {
      if (rule.test(field.value.trim()) === false) { message = rule.message; break; }
    }

    if (message) {
      field.parentElement.classList.add('has-error');
      if (errorEl) errorEl.textContent = message;
      return false;
    }
    field.parentElement.classList.remove('has-error');
    return true;
  }

  const rules = {
    required: (label) => ({ test: (v) => v.length > 0, message: `${label} is required.` }),
    email: () => ({ test: (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v), message: 'Enter a valid email address.' }),
    minLen: (n, label) => ({ test: (v) => v.length >= n, message: `${label} must be at least ${n} characters.` }),
    positiveNumber: (label) => ({ test: (v) => v !== '' && !isNaN(v) && Number(v) > 0, message: `${label} must be a valid positive number.` }),
    nonNegativeInt: (label) => ({ test: (v) => v !== '' && /^\d+$/.test(v), message: `${label} must be a whole number, 0 or greater.` }),
  };

  /* ---------------- Init on load ---------------- */

  document.addEventListener('DOMContentLoaded', () => {
    initModalDismiss();
    initDropdowns();
    initActionMenus();
    initSidebar();
  });

  return { toast, openModal, closeModal, validateField, rules };
})();
