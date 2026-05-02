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
    const name = document.getElementById("CreateOrganizationName").value;
    const email = document.getElementById("CreateTeamLeadEmail").value;
    const firstName = document.getElementById("CreateFirstName").value;
    const surname = document.getElementById("CreateSurname").value;

    const isNameValid = validValue(name, "error-name", window.translations.Name);
    const isEmailValid = validEmail(email);
    const isFirstNameValid = validValue(firstName, "error-firstName", window.translations.FirstName);
    const isSurnameValid = validValue(surname, "error-surname", window.translations.Surname);

    return isNameValid && isEmailValid && isFirstNameValid && isSurnameValid;
}

function CreateOrganization() {
    if (validateForm()) {
        $.ajax({
            type: "POST",
            url: "/Moderator/CreateOrganization",
            data: $("#CreateOrganization").serialize(),
            success: function (response) {
                $("#create-organization").modal('toggle');
                document.getElementById("CreateOrganizationName").value = '';
                document.getElementById("CreateTeamLeadEmail").value = '';
                document.getElementById("CreateFirstName").value = '';
                document.getElementById("CreateSurname").value = '';
                if (response.isSuccess) {
                    document.getElementById("responseTextCon").innerHTML = response.message;
                    reloadTable()
                    $("#confirmed").modal('show'); 
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
}
function reloadTable() {
    $("#organization-table").load(window.location.href + " #organization-table > *", function () {
        organizationTable()
    });
    $('#datatable-basic thead th').each(function () {
        this.style.setProperty('text-align', 'center', 'important');
    });
}