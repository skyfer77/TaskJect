$(document).ready(function () {
    organizationTable()

    $(document).on("click", '[data-bs-target="#lockout-organization-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('IdOrganization').value = item.id;
    });

    $(document).on("click", '[data-bs-target="#unlockout-organization-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('IdOrganizationUnlock').value = item.id;
    });

    $(document).on("click", '[data-bs-target="#delete-organization-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('OrganizationIdDelete').value = item.id;
        document.getElementById('organizationNameDelete').textContent = item.name;
    });
    $('#datatable-basic thead th').each(function () {
        this.style.setProperty('text-align', 'center', 'important');
    });
});

function organizationTable() {
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
    $('#datatable-basic thead th').each(function () {
        this.style.setProperty('text-align', 'center', 'important');
    });
}


function LockoutOrganization() {
    $("#lockout-organization-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/Lockout",
        data: $("#LockoutOrganizationRow").serialize(),
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

function UnlockOrganization() {
    $("#unlockout-organization-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/Unlockout",
        data: $("#UnlockOrganizationRow").serialize(),
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
function initializeDatePickers() {
    flatpickr("#tariffDateToOrganization", {
        allowInput: true,
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });
}

function DeleteOrganization() {
    $("#delete-organization-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/DeleteOrganization",
        data: $("#DeleteOrganizationRow").serialize(),
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
function loadTariffModal(orgCode, dateTo, tariffName) {
    $.ajax({
        url: '/Moderator/GetTariffModal',
        type: 'GET',
        data: {
            organizationCode: orgCode,
            dateTo: dateTo,
            tariffName: tariffName
        },
        success: function (response) {
            $('#modal-container').html(response);

            new Choices('#tariffSelect', {
                searchEnabled: false,
                placeholderValue: window.translations.SelectRole,
                shouldSort: false,
                position: 'auto',
                searchPlaceholderValue: `${window.translations.Search}...`,
                noResultsText: window.translations.noResultsFound,
                noChoicesText: window.translations.noChoicesChooseFrom,
                itemSelectText: window.translations.pressSelect,
            });

            $('#tariff-modal').modal('show');

            changeDatePicker();
            initializeDatePickers();
        }
    });
}
function editTariffDate() {
    document.getElementById("hiddenTariffDateTo").value = document.getElementById("tariffDateToOrganization").value;
    $("#tariff-modal").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/UpdateTariffDate",
        dataType: 'JSON',
        data: $("#UpdateTariffForm").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                reloadTable();
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        },
        error: function () {
            showWarning(window.translations.errorOccurredWhileUpdatingTariff);
        }
    });
}
function changeDatePicker() {
    var tariffSelect = document.getElementById("tariffSelect");
    var dateInput = document.getElementById("tariffDateToOrganization");
    var dateTo = document.getElementById("dateToTariff").value;
    var currentTariff = document.getElementById("currentTariff");
    if (tariffSelect.value === "Default") {
        var dateTo = new Date(9999, 11, 31);
        dateInput.value = formatDateTime(dateTo);
        dateInput.disabled = true;
    }
    else if (tariffSelect.value === currentTariff) {
        var endDate = new Date(dateTo);
        endDate.setMonth(endDate.getMonth() + 1);
        dateInput.value = formatDateTime(endDate);
        dateInput.disabled = false;
    }
    else
    {
        var endDate = new Date();
        endDate.setMonth(endDate.getMonth() + 1);
        dateInput.value = formatDateTime(endDate);
        dateInput.disabled = false;
    }
}

function formatDateTime(date) {
    return date.getFullYear() + "-" +
        ("0" + (date.getMonth() + 1)).slice(-2) + "-" +
        ("0" + date.getDate()).slice(-2);
}
function reloadTable() {
    $("#organization-table").load(window.location.href + " #organization-table > *", function () {
        organizationTable()
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