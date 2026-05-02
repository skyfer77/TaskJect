(function () {
    'use strict';

    //document.querySelectorAll('.slider-container').forEach((container, index) => {
    //    var swiperThumbs = new Swiper(container.querySelector(".swiper-view"), {
    //        spaceBetween: 10,
    //        slidesPerView: 4,
    //        freeMode: true,
    //        watchSlidesProgress: true,
    //    });

    //    var swiperMain = new Swiper(container.querySelector(".swiper-preview"), {
    //        spaceBetween: 10,
    //        navigation: {
    //            nextEl: container.querySelector(".swiper-button-next"),
    //            prevEl: container.querySelector(".swiper-button-prev"),
    //        },
    //        thumbs: {
    //            swiper: swiperThumbs,
    //        },
    //        loop: true,
    //        autoplay: {
    //            delay: 2500,
    //            disableOnInteraction: false
    //        }
    //    });
    //});

    //var swiper = new Swiper(".swiper-flip", {
    //    effect: "flip",
    //    grabCursor: true,
    //    pagination: {
    //        el: ".swiper-pagination",
    //    },
    //    navigation: {
    //        nextEl: ".swiper-button-next",
    //        prevEl: ".swiper-button-prev",
    //    },
    //    loop: true,
    //    autoplay: {
    //        delay: 2500,
    //        disableOnInteraction: false
    //    }
    //});

    // for testimonials
    var swiper = new Swiper(".pagination-dynamic", {
        pagination: {
            el: ".swiper-pagination",
            dynamicBullets: true,
            clickable: true,
        },
        slidesPerView: 1,
        loop: true,
        autoplay: {
            delay: 3000,
            disableOnInteraction: false,
        },
        breakpoints: {
            768: {
                slidesPerView: 2,
                spaceBetween: 40,
            },
            1024: {
                slidesPerView: 2,
                spaceBetween: 50,
            },
            1400: {
                slidesPerView: 3,
                spaceBetween: 50,
            },
        },
    });
})();