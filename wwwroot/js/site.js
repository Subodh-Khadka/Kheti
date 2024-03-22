
document.addEventListener("DOMContentLoaded", function () {
    var currentPageUrl = window.location.pathname;
    console.log(currentPageUrl);

    var navLinks = document.querySelectorAll('.nav-link')
    console.log(navLinks);

    navLinks.forEach(function (navLink) {
        var navLinkUrl = navLink.getAttribute('href');
        console.log(navLinkUrl);

        if (currentPageUrl == navLinkUrl) {
            navLink.classList.add('actives')
        }
    });
});