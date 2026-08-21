document.addEventListener("DOMContentLoaded", () => {

    initAccountMenu();
    initMobileDrawer();
    initReveal();

});


/* ============================================================
   ACCOUNT DROPDOWN
   ============================================================ */

function initAccountMenu() {

    const account = document.getElementById("account");
    const toggle = document.getElementById("account-toggle");

    if (!account || !toggle) {
        return;
    }


    toggle.addEventListener("click", event => {

        event.stopPropagation();

        account.classList.toggle("open");

    });


    document.addEventListener("click", event => {

        if (!account.contains(event.target)) {

            account.classList.remove("open");

        }

    });

}



/* ============================================================
   MOBILE DRAWER
   ============================================================ */

function initMobileDrawer() {

    const drawer = document.getElementById("drawer");

    const openButton =
        document.getElementById("drawer-open");

    const closeButton =
        document.getElementById("drawer-close");

    const background =
        document.getElementById("drawer-bg");


    if (!drawer) {
        return;
    }


    const open = () => {

        drawer.classList.add("open");

    };


    const close = () => {

        drawer.classList.remove("open");

    };


    openButton?.addEventListener(
        "click",
        open
    );


    closeButton?.addEventListener(
        "click",
        close
    );


    background?.addEventListener(
        "click",
        close
    );


    drawer
        .querySelectorAll("a:not(.future-link)")
        .forEach(link => {

            link.addEventListener(
                "click",
                close
            );

        });

}



/* ============================================================
   SCROLL REVEAL
   ============================================================ */

function initReveal() {

    const elements =
        document.querySelectorAll(".reveal");


    if (!elements.length) {
        return;
    }


    if (!("IntersectionObserver" in window)) {

        elements.forEach(element => {

            element.classList.add("in");

        });

        return;
    }


    const observer =
        new IntersectionObserver(

            entries => {

                entries.forEach(entry => {

                    if (!entry.isIntersecting) {
                        return;
                    }


                    entry.target.classList.add("in");

                    observer.unobserve(
                        entry.target
                    );

                });

            },

            {
                threshold: 0.12
            }

        );


    elements.forEach(element => {

        observer.observe(element);

    });

}



/* ============================================================
   GLOBAL TOAST
   Feature scripts can call:

       toast("Product added!");
       toast("Profile updated", "Success");

   ============================================================ */

window.toast = function (title, sub = "") {

    let wrapper =
        document.querySelector(".toast-wrap");


    if (!wrapper) {

        wrapper =
            document.createElement("div");

        wrapper.className =
            "toast-wrap";

        document.body.appendChild(
            wrapper
        );

    }


    const element =
        document.createElement("div");

    element.className =
        "toast";


    const titleElement =
        document.createElement("span");

    titleElement.textContent =
        title;

    element.appendChild(
        titleElement
    );


    if (sub) {

        const subElement =
            document.createElement("span");

        subElement.textContent =
            `· ${sub}`;

        subElement.style.opacity =
            ".7";

        subElement.style.fontWeight =
            "600";

        element.appendChild(
            subElement
        );

    }


    wrapper.appendChild(
        element
    );


    setTimeout(() => {

        element.style.transition =
            "opacity .3s, transform .3s";

        element.style.opacity =
            "0";

        element.style.transform =
            "translateY(10px)";


        setTimeout(
            () => element.remove(),
            300
        );

    }, 2200);

};



/* ============================================================
   SMALL UI ANIMATION HELPER
   ============================================================ */

window.bump = function (element) {

    if (!element ||
        typeof element.animate !== "function") {

        return;

    }


    element.animate(

        [
            {
                transform: "scale(1)"
            },
            {
                transform: "scale(1.3)"
            },
            {
                transform: "scale(1)"
            }
        ],

        {
            duration: 300,

            easing:
                "cubic-bezier(.34,1.56,.64,1)"
        }

    );

};