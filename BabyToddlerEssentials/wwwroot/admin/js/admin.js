/* ==========================================================================
   baby basket Admin — App shell (sidebar nav state, header, notifications)
   Runs on every admin page.
   ========================================================================== */

document.addEventListener('DOMContentLoaded', () => {

  /* Highlight the active sidebar item based on body[data-page] */
  const currentPage = document.body.getAttribute('data-page');
  if (currentPage) {
    document.querySelectorAll('.nav-item[data-page]').forEach((item) => {
      item.classList.toggle('is-active', item.getAttribute('data-page') === currentPage);
    });
  }

  /* Quick action navigation */
  document.querySelectorAll('[data-navigate]').forEach((el) => {
    el.addEventListener('click', () => {
      window.location.href = el.getAttribute('data-navigate');
    });
  });


});
