var request;
function Overview(taskId) {
    if (request) {
        request.abort();
    }

    modalWindowLoad("overview-task", window.translations.OverviewTask, ``, 'modal-lg')

    $("#overview-task").modal("show");

    request = $.ajax({
        type: "POST",
        url: "/PersonalTodo/OverviewTask",
        data: { id: taskId },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#overview-task .modal-content").html(newContent);
            initStyle();
        },
        complete: function () {
            request = null;
        }
    });
    hiddenModalWindow('#overview-task')
}

function initStyle() {
    $('span[name="Low-over"]').addClass('badge bg-info-transparent');
    $('span[name="Medium-over"]').addClass('badge bg-success-transparent');
    $('span[name="High-over"]').addClass('badge bg-danger-transparent');
}