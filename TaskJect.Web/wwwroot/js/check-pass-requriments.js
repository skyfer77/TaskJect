document.addEventListener("DOMContentLoaded", function () {
    const newPassword = document.getElementById('newPassword');
    const confirmPassword = document.getElementById('confirmPassword');
    const passwordMatchError = document.getElementById('passwordMatchError');
    const form = document.getElementById('changePasswordForm');

    const lengthReq = document.getElementById('length');
    const digitReq = document.getElementById('digit');
    const upperReq = document.getElementById('uppercase');
    const specialReq = document.getElementById('special');

    function validatePassword(value) {
        lengthReq.className = value.length >= 6 ? 'valid' : 'invalid';
        digitReq.className = /\d/.test(value) ? 'valid' : 'invalid';
        upperReq.className = /[A-Z]/.test(value) ? 'valid' : 'invalid';
        specialReq.className = /[!_#$%^&*(),.?":{}|<>]/.test(value) ? 'valid' : 'invalid';

        return (
            value.length >= 6 &&
            /\d/.test(value) &&
            /[A-Z]/.test(value) &&
            /[!_#$%^&*(),.?":{}|<>]/.test(value)
        );
    }

    function checkPasswordsMatch() {
        const match = newPassword.value === confirmPassword.value;
        passwordMatchError.classList.toggle('d-none', match);
        return match;
    }

    newPassword.addEventListener('input', () => {
        validatePassword(newPassword.value);
        checkPasswordsMatch();
    });

    confirmPassword.addEventListener('input', checkPasswordsMatch);

    form.addEventListener('submit', function (e) {
        const passwordValid = validatePassword(newPassword.value);
        const passwordsMatch = checkPasswordsMatch();

        if (!passwordValid || !passwordsMatch) {
            e.preventDefault();
        }
    });
});

function changeUserPassword() {
    $.ajax({
        type: "POST",
        url: "/Account/ChangePassword",
        data: $("#changePasswordForm").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                showConfirmed(response.message);
                clearPasswordInputs();
                const changeModal = bootstrap.Modal.getInstance(document.getElementById('changePasswordModal'));
                changeModal.hide();
            } else {
                showWarning(response.message);
            }
        },
        error: function () {
            console.error("An error occurred while changing the password.");
        }
    });
}

document.getElementById('changePasswordForm').addEventListener('submit', function (e) {
    e.preventDefault();

    changeUserPassword();
});

function clearPasswordInputs() {
    document.getElementById('currentPassword').value = '';
    document.getElementById('newPassword').value = '';
    document.getElementById('confirmPassword').value = '';
    document.getElementById('passwordMatchError').classList.add('d-none');

    ['length', 'digit', 'uppercase', 'special'].forEach(id => {
        document.getElementById(id).className = 'invalid';
    });
}

document.getElementById('changePasswordModal').addEventListener('hidden.bs.modal', clearPasswordInputs);

function showConfirmed(message) {
    $("#responseTextCon").text(message);
    $("#confirmed").modal('show');
}

function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}

