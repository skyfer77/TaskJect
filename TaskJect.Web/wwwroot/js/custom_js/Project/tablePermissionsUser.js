$(document).ready(function () {
    permissionsTable()
});
function permissionsTable() {
    let table = $('#permissionsTable').DataTable({
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
                "targets": [2, 3, 4, 5],
                "orderable": false
            }
        ],
        createdRow: function (row, data, dataIndex) {
            $('td', row).each(function () {
                this.style.setProperty('text-align', 'center', 'important');
            });
        },
        initComplete: function () {
            $('#permissionsTable thead th').each(function () {
                this.style.setProperty('text-align', 'center', 'important');
            });
        }
    });

    setupShortDataTableLocalization(table);
}

function loadPermissionsTable(teamId, projectId = null) {
    const dataToSend = { teamId: teamId };

    if (projectId) {
        dataToSend.projectId = projectId;
    }

    $.ajax({
        url: '/Project/LoadPermissionsTable',
        type: 'POST',
        data: dataToSend,
        success: function (html) {
            $('#permissionsContainer').html(html);
            permissionsTable()
        },
        error: function () {
            console.error("Failed to load the permissions table.");
        }
    });
}

function UpdatePermissions() {
    $.ajax({
        type: "POST",
        url: "/Project/UpdatePermissions",
        data: $("#UpdatePermissions").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                let teamId = document.getElementById("TeamId").value;
                let projectId = document.getElementById("ProjectId").value;
                document.getElementById("responseTextCon").innerHTML = response.message;
                $("#confirmed").modal('show');
                loadPermissionsTable(teamId, projectId) 
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        },
        error: function () {
            document.getElementById("responseTextWar").innerHTML = response.message;
            $("#warning").modal('show');
        }
    });
}