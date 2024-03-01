
document.addEventListener('DOMContentLoaded', function () {

        //for rating
        const stars = document.querySelectorAll('.bi-star-fill');

        stars.forEach(star => {
            star.addEventListener('click', function () {
                const rating = parseInt(this.getAttribute('data-rating'));

                stars.forEach(s => s.classList.remove('rated'));

                for (let i = 0; i < rating; i++) {
                    stars[i].classList.add('rated');
                }

                document.getElementById('rating').value = rating;
                console.log(rating);
            });
        });

    //clearing the existing value of rating when the close button is pressed
    const closeButton = document.getElementById('close_btn');
    closeButton.addEventListener('click', function () {
        document.getElementById('rating').value = 0;
        console.log(rating);
    });

        //getting the productId for review
        const reviewButtons = document.querySelectorAll('.review-btn');

        reviewButtons.forEach(button => {
            button.addEventListener('click', function () {
                const productId = this.getAttribute('data-product-id');
                document.getElementById('productId').value = productId;
            });
        });
    });



