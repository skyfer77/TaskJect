function showError(elementId, message) {
    const error = document.getElementById(elementId);
    $("#" + elementId).css("display", "block");
    error.innerHTML = message;
}

function hideError(elementId) {
    $("#" + elementId).css("display", "none");
}

function validEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!regex.test(email)) {
        showError("error-email", window.translations.invalidEmailAddress);
        return false;
    }
    hideError("error-email");
    return true;
}

function validValue(value, idError, textError) {
    if (value === '') {
        showError(idError, `${textError} ${window.translations.cannotBeEmpty}.`);
        return false;
    }
    hideError(idError);
    return true;
}

function validateForm() {
    const email = document.getElementById("CreateUserEmail").value;
    const firstName = document.getElementById("CreateFirstName").value;
    const surname = document.getElementById("CreateSurname").value;

    const isEmailValid = validEmail(email);
    const isFirstNameValid = validValue(firstName, "error-firstName", window.translations.FirstName);
    const isSurnameValid = validValue(surname, "error-surname", window.translations.Surname);

    return isEmailValid && isFirstNameValid && isSurnameValid;
}

function CreateMember() {
    if (validateForm()) {
        $.ajax({
            type: "POST",
            url: "/Team/CreateUser",
            data: $("#CreateUser").serialize(),
            success: function (response) {
                $("#create-user").modal('toggle');
                document.getElementById("CreateUserEmail").value = '';
                document.getElementById("CreateFirstName").value = '';
                document.getElementById("CreateSurname").value = '';
                if (response.isSuccess) {
                    $("#members-view").load(window.location.href + " #members-view > *", function () {
                        document.getElementById("responseTextCon").innerHTML = response.message;
                        $("#confirmed").modal('show');
                        AdjustTeamCardMaxHeight();
                    });
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
}