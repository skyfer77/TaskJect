$(document).ready(function () {
    //Search project for title
    const searchInput = document.querySelector("[data-search]");
    const project = document.querySelectorAll("[data-project-container]");

    searchInput.addEventListener("input", e => {
        const value = e.target.value.toLowerCase()
        project.forEach((element) => {
            const titles = element.querySelector("[data-title]");
            var title = titles.innerText.toLowerCase().includes(value)
            element.classList.toggle("hide", !title)
        })
    })
    //Add style Priority and status
    updateStyles();
});
//Set value delete project id and title project
$(document).on("click", '[data-bs-target="#delete-project"]', function () {
    var item = $(this).data('todo');
    var obj = Object.getOwnPropertyDescriptors(item);
    document.getElementById('projectID').value = obj.idProject.value;
    document.getElementById('projectTitle').innerHTML = obj.titleProject.value;
});
//Delete project by id project
function DeleteProject() {
    $("#delete-project").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Project/Delete",
        data: $("#DeleteProject").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                $("#projectList").load(window.location.href + " #projectList", function (data) {
                    var scripts = $(data).find("script");

                    if (scripts.length) {
                        $(scripts).each(function () {
                            if ($(this).attr("src")) {
                                $.getScript($(this).attr("src"));
                            }
                            else {
                                eval($(this).html());
                            }
                        });
                    }
                    updateStyles()
                    sortProjects();
                });
                document.getElementById("responseTextCon").innerHTML = response.message;
                $("#confirmed").modal('show');
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        }
    });
}

//View modal edit project by id 
var projectRequest;
function Edit(id) {
    if (projectRequest) {
        projectRequest.abort();
    }

    modalWindowLoad("edit-project", window.translations.EditProject, ` style="--bs-modal-width: 950px;"`)

    $("#edit-project").modal("show");

    projectRequest = $.ajax({
        type: "POST",
        url: "/Project/EditProject/",
        data: { id: id },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#edit-project .modal-content").html(newContent);
            InitializeModalScripts();
        },
        complete: function () {
            projectRequest = null;
        }
    });
    hiddenModalWindow('#edit-project')
}
function updateStyles()
{
    $('span[name="Low"]').addClass('bg-info-transparent');
    $('span[name="Medium"]').addClass('bg-success-transparent');
    $('span[name="High"]').addClass('bg-danger-transparent');
    $('span[name="OnHold"]').addClass('bg-warning-transparent');
    $('span[name="NotStarted"]').addClass('bg-primary-transparent');
    $('span[name="InProgress"]').addClass('bg-info-transparent');
    $('span[name="OnReview"]').addClass('bg-danger-transparent');
    $('span[name="Done"]').addClass('bg-success-transparent');
    $('span[name="Archived"]').addClass('bg-warning-transparent');
}