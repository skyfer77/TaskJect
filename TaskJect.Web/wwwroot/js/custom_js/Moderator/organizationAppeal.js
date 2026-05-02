$(document).ready(function () {
    styleBadge()
    organizationAppealTable()

    $(document).on("click", '[data-bs-target="#delete-appeal-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('appealIdDelete').value = item.id;
        document.getElementById('appealNameDelete').textContent = item.name;
    });
});

function organizationAppealTable() {
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

function styleBadge() {
    $('span[name="TakenToWork"]').addClass('bg-info-transparent');
    $('span[name="Rejected"]').addClass('bg-danger-transparent');
    $('span[name="Postponed"]').addClass('bg-warning-transparent');
    $('span[name="InProcessing"]').addClass('bg-primary-transparent');
    $('span[name="Done"]').addClass('bg-success-transparent');
}

function toggleRejectedDescription() {
    var statusSelect = document.getElementById("choices-multiple-remove-button1");
    var selectedStatus = statusSelect.value;
    var rejectedDescriptionDiv = document.getElementById("rejectedDescriptionDiv");

    if (selectedStatus === "Rejected") {
        rejectedDescriptionDiv.style.display = "block";
    } else {
        rejectedDescriptionDiv.style.display = "none";
    }
}

function getToolbarOptions() {
    return [
        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
        [{ 'font': [] }],
        ['bold', 'italic', 'underline', 'strike'],
        ['blockquote', 'code-block'],
        [{ 'header': 1 }, { 'header': 2 }],
        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
        [{ 'script': 'sub' }, { 'script': 'super' }],
        [{ 'indent': '-1' }, { 'indent': '+1' }],
        [{ 'direction': 'rtl' }],
        [{ 'size': ['small', false, 'large', 'huge'] }],
        [{ 'color': [] }, { 'background': [] }],
        [{ 'align': [] }],
        ['image', 'video'],
        ['clean']
    ];
}

function initializeQuill() {
    var toolbarOptions = getToolbarOptions();

    var quill = new Quill('#appeal-descriptioin-editor', {
        modules: {
            toolbar: toolbarOptions
        },
        theme: 'snow'
    });

    return quill;
}

var organizationAppealRequest;
function Edit(appealId, organizationId) {
    if (organizationAppealRequest) {
        organizationAppealRequest.abort();
    }

    modalWindowLoad("edit-appeal", window.translations.EditAppeal, '', "modal-lg")

    $("#edit-appeal").modal("show");

    organizationAppealRequest = $.ajax({
        type: "POST",
        url: "/Moderator/ModalWindowEditAppeal/",
        data: { appealId, organizationId },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#edit-appeal .modal-content").html(newContent);
            initEditOrganizationAppeal();
        },
        complete: function () {
            organizationAppealRequest = null;
        }
    });
    hiddenModalWindow('#edit-appeal')
}

function EditAppeal() {
    $("#edit-appeal").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/EditAppeal/",
        data: $("#EditAppeal").serialize(),
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

function DeleteAppeal() {
    $("#delete-appeal-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Moderator/DeleteAppeal",
        data: $("#DeleteAppealRow").serialize(),
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
    $("#organization-appeal-table").load(window.location.href + " #organization-appeal-table > *", function () {
        organizationAppealTable()
        styleBadge()
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

