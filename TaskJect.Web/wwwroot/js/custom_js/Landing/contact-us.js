(function () {
    'use strict';
    $(document).ready(function () {
        $(".form-control").on("input", function () {
            $(this).removeClass("is-invalid");
        });

        $("#send-message").click(function (e) {
            sendMessage(e);
        });
    });

    let sendMessage = (e) => {
        e.preventDefault();

        let isValid = true

        let name = $("#contact-address-name").val().trim();
        let phone = $("#contact-address-phone").val().trim();
        let email = $("#contact-address-email").val().trim();
        let message = $("#contact-address-message").val().trim();

        let emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        isValid = validateField($("#contact-address-name"), name !== "", isValid);
        isValid = validateField($("#contact-address-phone"), phone !== "", isValid);
        isValid = validateField($("#contact-address-email"), emailRegex.test(email), isValid);
        isValid = validateField($("#contact-address-message"), message !== "", isValid);

        if (!isValid) return;

        $.ajax({
            url: "/ContactUs/Contact",
            type: "POST",
            data: {
                Name: name,
                Phone: phone,
                Email: email,
                Message: message
            },
            success: function (response) {
                if (response.isSuccess) {
                    $("#contact-address-name, #contact-address-phone, #contact-address-email, #contact-address-message").val("");
                    document.getElementById("responseTextCon").innerHTML = response.message;
                    $("#confirmed").modal('show');
                }
                else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }

    let validateField = (input, condition, isValid) => {
        if (condition) {
            input.removeClass("is-invalid");
        } else {
            input.addClass("is-invalid");
            isValid = false;
        }
        return isValid;
    }

})();