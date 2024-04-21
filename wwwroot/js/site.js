
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



function ImagePreview(event) {
    var image = URL.createObjectURL(event.target.files[0]);
    var imagePreview = document.getElementById('imagePreview');

    imagePreview.innerHTML = '';

    var newImage = document.createElement('img');
    newImage.src = image;
    newImage.width = 100;
    newImage.height = 100;
    imagePreview.appendChild(newImage);
}
