$(document).ready(function () {
    $(document).on("change", ".choices-multiple-remove-button4", function () {
        var userId = $(this).closest("tr").find('[data-todo]').data('todo').id;
        var organizationRole = parseInt($(this).val());
        updateOrganizationRole(userId, organizationRole);
    });
});

function updateOrganizationRole(userId, organizationRole) {
    $.ajax({
        type: "POST",
        url: "/Organization/UpdateOrganizationRole",
        data: {
            userId: userId,
            organizationRole: organizationRole 
        },
        success: function (response) {
            if (response.isSuccess) {
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        },
        error: function () {
            showWarning(window.translations.anErrorOccurredWhileUpdatingRole);
        }
    });
}
function showConfirmed(message) {
    $("#responseTextCon").text(message);
    $("#confirmed").modal('show');
}

function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}