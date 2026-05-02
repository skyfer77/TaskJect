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
    const email = document.getElementById("AddUserEmail").value;
    const firstName = document.getElementById("AddFirstName").value;
    const surname = document.getElementById("AddSurname").value;

    const isEmailValid = validEmail(email);
    const isFirstNameValid = validValue(firstName, "error-firstName", window.translations.FirstName);
    const isSurnameValid = validValue(surname, "error-surname", window.translations.Surname);

    return isEmailValid && isFirstNameValid && isSurnameValid;
}

function AddUser() {
    if (validateForm()) {
        $.ajax({
            type: "POST",
            url: "/Organization/AddUser",
            data: $("#AddUser").serialize(),
            success: function (response) {
                $("#add-user").modal('toggle');
                document.getElementById("AddUserEmail").value = '';
                document.getElementById("AddFirstName").value = '';
                document.getElementById("AddSurname").value = '';
                if (response.isSuccess) {
                    $("#table-organization-user").load(window.location.href + " #table-organization-user > *", function () {
                        showConfirmed(response.message);
                        document.querySelectorAll("#choices-user-oraganization-role").forEach(element => {
                            new Choices(element, {
                                searchEnabled: false,
                                placeholderValue: window.translations.SelectRole,
                                shouldSort: false,
                                allowHTML: true,
                                position: 'auto',
                                searchPlaceholderValue: `${window.translations.Search}...`,
                                noResultsText: window.translations.noResultsFound,
                                noChoicesText: window.translations.noChoicesChooseFrom,
                                itemSelectText: window.translations.pressSelect,
                            });
                        });
                    });
                    countUser++;
                    updateProgressBar("userDisplay", "userProgressBar", countUser, maxUser, "{0}/{1}");
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
}