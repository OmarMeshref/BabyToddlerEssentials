document.addEventListener('submit', async function (e) {
    const form = e.target;
    if (!form.matches('.wish-form, .cart-form')) return;

    e.preventDefault();

    const isWishForm = form.classList.contains('wish-form');
    const btn = form.querySelector('button');
    const formData = new FormData(form);

    try {
        const res = await fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        // User is not logged in → redirect to Login
        if (
            res.redirected &&
            res.url.includes('/Identity/Account/Login')
        ) {
            window.location.href = res.url;
            return;
        }

        const data = await res.json();

        if (!data.success) {
            window.toast(data.message || 'Something went wrong.', 'Error');
            return;
        }

        if (isWishForm) {

            btn.classList.toggle('on', data.inWishlist);

            const heart =
                btn.querySelector('span');

            if (heart) {
                heart.textContent =
                    data.inWishlist ? '♥' : '♡';
            }

            btn.title =
                data.inWishlist
                    ? 'Remove from wishlist'
                    : 'Add to wishlist';

            window.toast(
                data.message,
                data.inWishlist ? 'Wishlist' : 'Wishlist'
            );

        } else {

            updateCartBadge(data.cartCount);

            window.toast(
                data.message,
                data.capped ? 'Stock limit reached' : 'Cart'
            );
        }

    } catch (err) {

        console.error('Request failed:', err);

        window.toast(
            'Something went wrong. Please try again.',
            'Error'
        );
    }
});


function updateCartBadge(count) {

    const badge =
        document.querySelector('.icon-btn .badge');

    if (!badge) return;

    badge.textContent = count;

    badge.classList.toggle(
        'show',
        count > 0
    );
}