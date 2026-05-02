var taskRequest;

function Create(projectId, status) {
    if (taskRequest) {
        taskRequest.abort(); // Скасовуємо попередній запит, якщо він ще триває
    }
    modalWindowLoad("add-task", window.translations.AddTask, `style="--bs-modal-width: 880px;"`)

    $("#add-task").modal("show");

    taskRequest = $.ajax({
        type: "POST",
        url: "/Task/CreateTask/",
        data: { projectId: projectId, status: status },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#add-task .modal-content").html(newContent);
            initCreateTaskModalScripts();
        },
        complete: function () {
            taskRequest = null;
        }
    });
    hiddenModalWindow('#add-task')
}

function Edit(id, projectId) {
    if (taskRequest) {
        taskRequest.abort();
    }

    modalWindowLoad("details-task", window.translations.DetailsTask, `style="--bs-modal-width: 880px;"`)

    $("#details-task").modal("show");

    taskRequest = $.ajax({
        type: "POST",
        url: "/Task/DetailsTask",
        data: { id: id, projectId: projectId },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#details-task .modal-content").html(newContent);
            const taskModalOptions = {
                isAdminOrGod: JSON.parse($("#isAdminOrGod").val()),
                canEditTask: JSON.parse($("#canEditTask").val()),
                canSetAssignments: JSON.parse($("#canSetAssignments").val()),
                statusToCheck: JSON.parse($("#statusToCheck").val()),
                isStatus: JSON.parse($("#isStatus").val())
            };
            initTaskModalScripts(taskModalOptions);
        },
        complete: function () {
            taskRequest = null;
        }
    });
    hiddenModalWindow('#details-task')
}
function uploadSingleFile(file, taskId, projectId) {
    return new Promise((resolve, reject) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("taskId", taskId);
        formData.append("projectId", projectId);

        $.ajax({
            type: "POST",
            url: "/Task/UploadSingleFile",
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

function DeleteTask() {
    $("#delete-task").modal("toggle");
    const taskId = $("#taskID").val();
    let formData = $("#DeleteTask").serialize();

    $.ajax({
        url: "/Task/Delete",
        method: "POST",
        data: formData,
        success: function (res) {
            if (!res.isSuccess) {
                showWarning(res.message);
            } else {
                showConfirmed(res.message);

                const card = document.querySelector(`[data-id-task="${taskId}"]`);
                const column = card.closest("[data-status-task]");
                const status = column.dataset.statusTask;

                card.remove();
                countTask(column, status);
            }
        }
    });
}
//Set value delete task id task and title task
$(document).on("click", '[data-bs-target="#delete-task"]', function () {
    var item = $(this).data('todo');
    var obj = Object.getOwnPropertyDescriptors(item);
    document.getElementById('taskID').value = obj.idTask.value;
    document.getElementById('taskTitle').innerHTML = obj.titleTask.value;
});
function createTask() {
    var title = document.getElementById("title");
    if (!valideteTitle(title.value)) {
        return;
    }

    const files = pond.getFiles();

    if (!valideteSizeFiles(files)) {
        return;
    }

    const formData = getFormDataWithoutFiles("CreateTask");

    $("#add-task").modal('toggle');
    $.ajax({
        url: "/Task/Create",
        method: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            if (!res.isSuccess) {
                showWarning(res.message);
                return;
            }

            const column = document.querySelector(`[data-status-task="${res.taskStatus}"]`);
            column.insertAdjacentHTML("afterbegin", res.html);

            const newCard = column.firstElementChild;
            applyFiltersToCard(newCard);
            countTask(column, res.taskStatus);
            updatePageStyle();

            if (files.length === 0) {
                showConfirmed(res.message + "!");
                return;
            }

            const taskId = res.taskId;
            const projectId = res.projectId;

            uploadNewFiles(files, taskId, projectId)
                .then((result) => {
                    const { successCount, failedCount } = result;

                    if (failedCount === 0) {
                        showConfirmed(res.message + " " + window.translations.AddedFiles) + "!";
                    } else if (successCount === 0) {
                        showWarning(res.message + " " + window.translations.NoFilesAttached) + "!";
                    } else {
                        let message = res.message + " " + formatString(
                            window.translations.FilesPartiallyAdded,
                            successCount,
                            files.length
                        ) + "!";
                        showWarning(message);
                    }
                })
                .catch(err => showWarning(err));
        }
    });
}

function editTask() {
    var title = document.getElementById("title");
    if (!valideteTitle(title.value)) {
        return;
    }

    const files = pond.getFiles();
    if (!valideteSizeFiles(files)) {
        return;
    }

    const newFiles = getNewFiles(files);

    const formData = getFormDataWithoutFiles("EditTask");

    const filesToDeleteIds = document.getElementById("filesToDelete").value;
    formData.append("filesToDelete", filesToDeleteIds);

    const taskId = document.getElementById("ID").value;
    const projectId = document.getElementById("ProjectID").value;

    const issueInput = document.getElementById("GitHubIssueNumber");
    if (issueInput) {
        formData.append("linkGitHubIssueNumber", issueInput.value);
    }

    $("#details-task").modal("toggle");

    $.ajax({
        url: "/Task/Edit",
        method: "POST",
        data: formData,
        processData: false,
        contentType: false,

        success: function (res) {
            if (!res.isSuccess) {
                showWarning(res.message);
                return;
            }

            const oldCard = document.querySelector(`[data-id-task="${taskId}"]`);
            const oldColumn = oldCard.closest("[data-status-task]");

            oldCard.remove();

            const newColumn = document.querySelector(`[data-status-task="${res.taskStatus}"]`);
            newColumn.insertAdjacentHTML("afterbegin", res.html);
            updatePageStyle();

            const newCard = newColumn.querySelector(`[data-id-task="${taskId}"]`);

            applyFiltersToCard(newCard);
            countTask(oldColumn, oldColumn.dataset.statusTask);

            if (newColumn !== oldColumn) {
                countTask(newColumn, res.taskStatus);
            }

            if (newFiles.length === 0) {
                showConfirmed(res.message + "!");
                return;
            }

            uploadNewFiles(newFiles, taskId, projectId)
                .then((result) => {
                    const { successCount, failedCount } = result;

                    if (failedCount === 0) {
                        showConfirmed(res.message + " " + window.translations.AddedFiles + "!");
                    } else if (successCount === 0) {
                        showWarning(res.message + " " + window.translations.NoFilesAttached + "!");
                    } else {
                        let message = res.message + " " + formatString(
                            window.translations.FilesPartiallyAdded,
                            successCount,
                            files.length
                        ) + "!";
                        showWarning(message);
                    }
                })
                .catch(err => showWarning(err));
        }
    });
}

function getNewFiles(files) {
    return files.filter(f => f.file && f.origin !== FilePond.FileOrigin.LOCAL);
}

function uploadNewFiles(files, taskId, projectId) {
    let successCount = 0;
    let failedCount = 0;

    const uploadPromises = [];

    for (const f of files) {
        if (f.file && f.origin !== FilePond.FileOrigin.LOCAL) {
            const p = uploadSingleFile(f.file, taskId, projectId)
                .then(() => successCount++)
                .catch(() => failedCount++);

            uploadPromises.push(p);
        }
    }

    return Promise.all(uploadPromises)
        .then(() => ({ successCount, failedCount }));
}


function addHours() {
    $("#addHoursModal").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Task/AddHours",
        data: $("#AddHours").serialize(),
        success: function (res) {
            if (!res.isSuccess) {
                showWarning(res.message);
            } else {
                showConfirmed(res.message);
                const card = document.querySelector(`[data-id-task="${res.taskId}"]`);
                const hoursSpan = card.querySelector("[data-actual-hours]");

                if (hoursSpan) {
                    hoursSpan.textContent = res.html;
                }
            }

        }
    });
}

$(document).ready(function () {
    new Choices('#periodFilter', {
        classNames: {
            containerOuter: 'choices period-flex'
        },
        removeItemButton: false,
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
        shouldSort: false,
    });
    updatePageStyle();
});

$('[data-search]').on('input', applyAllFiltersAndSort);
$('#overdueTasks').on('change', applyAllFiltersAndSort);
$("#choices-single-default").on("change", applyAllFiltersAndSort);

$("#periodFilter").on("change", async function () {
    let period = $(this).val();
    let projectId = $(this).data("projectId");
    if (!period) {
        period = null;
    }
    await loadColumns([4], projectId, period);
    updatePageStyle();
});

$(document).on("click", '.add-hours-icon', function () {
    let taskId = $(this).data('task-id');
    $('#taskIdInput').val(taskId);
    $('#hoursInput').val('');
    $('#minutesInput').val('');
});

function updatePageStyle() {
    $('span[name="Low"]').addClass('bg-info-transparent');
    $('span[name="Medium"]').addClass('bg-success-transparent');
    $('span[name="High"]').addClass('bg-danger-transparent');
}

function showConfirmed(message) {
    $("#responseTextCon").text(message);
    $("#confirmed").modal('show');
}

function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}