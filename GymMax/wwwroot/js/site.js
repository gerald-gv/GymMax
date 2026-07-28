const mobileMenuButton = document.getElementById("mobileMenuButton");
const mobileMenu = document.getElementById("mobileMenu");
const mobileMenuIcon = document.getElementById("mobileMenuIcon");

if (mobileMenuButton && mobileMenu && mobileMenuIcon) {

    mobileMenuButton.addEventListener("click", () => {

        const isOpen = mobileMenuButton.getAttribute("aria-expanded") === "true";

        mobileMenu.classList.toggle("hidden");

        mobileMenuButton.setAttribute( "aria-expanded", String(!isOpen));

        mobileMenuButton.setAttribute( "aria-label", isOpen ? "Abrir menú" : "Cerrar menú");

        mobileMenuIcon.classList.toggle("fa-bars", isOpen);
        mobileMenuIcon.classList.toggle("fa-xmark", !isOpen);

    });


    mobileMenu.querySelectorAll("a").forEach(link => {

        link.addEventListener("click", () => {

            mobileMenu.classList.add("hidden");

            mobileMenuButton.setAttribute( "aria-expanded", "false");

            mobileMenuButton.setAttribute( "aria-label", "Abrir menú");

            mobileMenuIcon.classList.remove("fa-xmark");
            mobileMenuIcon.classList.add("fa-bars");

        });

    });


    document.addEventListener("keydown", event => {

        if (event.key === "Escape") {

            mobileMenu.classList.add("hidden");

            mobileMenuButton.setAttribute("aria-expanded","false");

            mobileMenuButton.setAttribute("aria-label","Abrir menú");

            mobileMenuIcon.classList.remove("fa-xmark");
            mobileMenuIcon.classList.add("fa-bars");

        }

    });

}