$(document).ready(function () {
    $('span[name="Low"]').addClass('bg-info-transparent');
    $('span[name="Medium"]').addClass('bg-success-transparent');
    $('span[name="High"]').addClass('bg-danger-transparent');
    $('span[name="Completed"]').addClass('bg-info-transparent');
    $('span[name="In Progress"]').addClass('bg-primary-transparent');
    $('span[name="On-hold"]').addClass('bg-danger-transparent');
    var ol = document.querySelector("#desc ol")
    if (ol !== null) {
        ol.classList.add("task-details-key-tasks");
    }

    initializeFilepond("/Project/DownloadFile");

    const container = document.getElementById("select-repo");
    if (container) {
        const projectId = container.getAttribute('data-project-id');
        loadRepo(projectId, true);
    }
});

let deleteFileId = null;
let addProjectId = null;
// Під час відкриття модального вікна — встановлюємо дані
document.getElementById('delete-file-modal')
    .addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        if (!button) {
            return;
        }

        const fileId = button.getAttribute('data-file-id');
        const fileName = button.getAttribute('data-file-name');

        document.getElementById('deleteFileId').value = fileId;
        document.getElementById('deleteFileName').textContent = fileName;

        deleteFileId = fileId;
    });

document.getElementById('addFilesModal')
    .addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        if (!button) {
            return;
        }

        const projectId = button.getAttribute('data-project-id');

        document.getElementById('addFilesProjectId').value = projectId;

        addProjectId = projectId;
    });

document.getElementById('addFilesModal')
    .addEventListener('hidden.bs.modal', function () {
        if (window.pond) {
            pond.removeFiles();
        }

        addProjectId = null;
        document.getElementById('addFilesProjectId').value = '';
    });
// Видалення файлу по ID
function confirmDeleteFile() {
    if (!deleteFileId) {
        return;
    }

    $("#delete-file-modal").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Project/DeletedFile",
        data: { id: deleteFileId },
        success: function (response) {
            if (response.isSuccess) {
                const item = document.querySelector(`[data-file-id="${deleteFileId}"]`).closest('li');
                if (item) {
                    item.remove();
                }

                document.getElementById("responseTextCon").innerHTML = response.message;
                $("#confirmed").modal('show');
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }

        }
    });
}
function addedFile() {
    if (!addProjectId) {
        return;
    }

    const files = pond.getFiles();

    if (!valideteSizeFiles(files)) {
        return;
    }

    if (files.length === 0) {
        $("#addFilesModal").modal('toggle');
        return;
    }

    $("#addFilesModal").modal('toggle');

    try {
        const uploadPromises = [];
        for (const f of files) {
            if (f.file && f.origin !== FilePond.FileOrigin.LOCAL) {
                uploadPromises.push(uploadSingleFile(f.file, addProjectId));
            }
        }

        const currentProjectId = addProjectId; 
        Promise.all(uploadPromises).then(() => {
            loadFilesToView(currentProjectId);
        }).catch(err => {
            document.getElementById("responseTextWar").innerHTML = err;
            $("#warning").modal('show');
        });
        

    } catch (error) {
        document.getElementById("responseTextWar").innerHTML = error.message;
        $("#warning").modal('show');
    }
}

function loadFilesToView(projectId) {
    $.ajax({
        type: "GET",
        url: `/Project/GetFilesHtml?projectId=${projectId}`,
        success: function (response) {
            if (response.isSuccess) {
                document.getElementById("listFiles").innerHTML = response.html;
                document.getElementById("responseTextCon").innerHTML = response.message;
                $("#confirmed").modal('show');
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        }
    });
}

function uploadSingleFile(file, projectId) {
    return new Promise((resolve, reject) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("projectId", projectId);

        $.ajax({
            type: "POST",
            url: "/Project/UploadSingleFile",
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.isSuccess) {
                    resolve(res.message);
                } else {
                    reject(res.message);
                }
            },
            error: function (xhr) {
                console.error("Ajax error: " + xhr.status + " - " + xhr.statusText);
                reject(window.translations.FileUploadError);
            }
        });
    });
}