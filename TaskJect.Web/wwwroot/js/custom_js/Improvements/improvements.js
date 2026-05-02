$(document).ready(function () {
    styleBadge() 
    organizationAppealTable()
    $('#datatable-basic thead th').each(function () {
        this.style.setProperty('text-align', 'center', 'important');
    });
});
function organizationAppealTable() {
    let table = $('table.text-nowrap.table-bordered').DataTable({
        language: {
            searchPlaceholder: `${window.translations.Search}...`,
            sSearch: '',
            paginate: {
                first: window.translations.first,
                previous: window.translations.previous,
                next: window.translations.next,
                last: window.translations.last
            },
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

function loadOverviewModal(id) {
    $.ajax({
        url: '/Improvements/GetImprovementsModal',
        type: 'GET',
        data: {
            idAppeal: id
        },
        success: function (response) {
            $('#modal-container').html(response);
            $('#detail-appeal').modal('show');
        }
    });
}

function styleBadge() {
    $('span[name="TakenToWork"]').addClass('bg-info-transparent');
    $('span[name="Rejected"]').addClass('bg-danger-transparent');
    $('span[name="Postponed"]').addClass('bg-warning-transparent');
    $('span[name="InProcessing"]').addClass('bg-primary-transparent');
    $('span[name="Done"]').addClass('bg-success-transparent');
}