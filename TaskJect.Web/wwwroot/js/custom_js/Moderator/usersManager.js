$(document).ready(function () {
    usersTable();
    selectRole();

    $(document).on("click", '[data-bs-target="#lockout-user-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('UserIdLockout').value = item.id;
        document.getElementById('lockoutUserText').textContent = `${window.translations.doYouWantBlockUser} ` + item.name + " " + item.surname + "?";
    });

    $(document).on("click", '[data-bs-target="#unlockout-user-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('IdUserUnlock').value = item.id;
        document.getElementById('unlockUserText').textContent = `${window.translations.areYouSureWantUnblockUser} ` + item.name + " " + item.surname + "?";
    });

    $(document).on("click", '[data-bs-target="#delete-user-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('UserIdDelete').value = item.id;
        document.getElementById('userNameDelete').textContent = item.name + " " + item.surname;
    });

    $(document).on("change", ".role-select", function () {
        var userId = $(this).closest("tr").find('[data-todo]').data('todo').id;
        var roleId = $(this).val();
        SwitchRole(userId, roleId);
    });
});

function usersTable() {
    let table = $('#datatable-basic').DataTable({
        language: {
            searchPlaceholder: `${window.translations.Search}...`,
            sSearch: '',
            info: formatString(window.translations.dataTableInfo, '_START_', '_END_', '_TOTAL_'),
            infoEmpty: window.translations.noEntriesToShow,
            zeroRecords: window.translations.noMatchingRecordsFound,
        },
        info: false,
        paging: false,
        columnDefs: [
            {
                "targets": 0,
                "render": function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                "targets": -1,
                "orderable": false
            }
        ]
    });
    setupShortDataTableLocalization(table);
}

function selectRole() {
    $('.role-select').each(function () {
        $(this).css('visibility', 'hidden');
        new Choices(this, {
            searchPlaceholderValue: `${window.translations.Search}...`,
            noResultsText: window.translations.noResultsFound,
            noChoicesText: window.translations.noChoicesChooseFrom,
            itemSelectText: window.translations.pressSelect,
        });
        $(this).css('visibility', 'visible');
    });
}

function SwitchRole(userId, roleId) {
    $.ajax({
        type: "POST",
        url: "/Moderator/SetNewRoleForUser",
        data: { userId: userId, roleId: roleId },
        success: function (response) {
            if (response.isSuccess) {
                reloadTable();
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        }
    });
}

function LockoutUser() {
    $("#lockout-user-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/LockoutUser",
        data: $("#LockoutUserRow").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                reloadTable();
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        }
    });
}

function UnlockUser() {
    $("#unlockout-user-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/UnlockUser",
        data: $("#UnlockUserRow").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                reloadTable();
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        }
    });
}

function DeleteUser() {
    $("#delete-user-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/DeleteUser",
        data: $("#DeleteUserRow").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                reloadTable();
                showConfirmed(response.message)
            } else {
                showWarning(response.message);
            }
        }
    });
}

function reloadTable() {
    $("#users-table").load(window.location.href + " #users-table > *", function () {
        usersTable();
        selectRole();
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
