var multipleEdit2;
function InitializeModalScripts() {
    //Select
    const multipleEdit = new Choices('#choices-multiple-button1', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
    const multipleEdit1 = new Choices('#choices-multiple-button2', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });

    //Date picker
    flatpickr("#targetDateStart", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });
    flatpickr("#targetDateEnd", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });
    /* quill snow editor */
    var toolbarOptions = [
        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
        [{ 'font': [] }],
        ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
        ['blockquote', 'code-block'],

        [{ 'header': 1 }, { 'header': 2 }],               // custom button values
        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
        [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
        [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
        [{ 'direction': 'rtl' }],                         // text direction

        [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown

        [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
        [{ 'align': [] }],

        ['image', 'video'],
        ['clean']                                         // remove formatting button
    ];
    let quill = new Quill('#project-descriptioin-editor', {
        modules: {
            toolbar: toolbarOptions
        },
        theme: 'snow'
    });
    const hiddenInput = document.getElementById("Description");
    hiddenInput.value = quill.root.innerHTML;

    quill.on('text-change', () => {
        hiddenInput.value = quill.root.innerHTML;
    });

    //Acardion
    const accValue = document.querySelectorAll("[data-accordion-team]");
    accValue.forEach((e) => {
        if (e.ariaExpanded == 'true') {
            document.getElementById("TeamId").value = e.name;
            MembersByTeam(e.name);
            e.disabled = true;
        }
    })

    multipleEdit2 = new Choices('#ManagerID', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });

    if (typeof permissionsTable === "function") {
        permissionsTable()
    }
    initializeFilepond("/Project/DownloadFile");

    const container = document.getElementById("select-repo");
    if (container) {
        const projectId = container.getAttribute('data-project-id');
        loadRepo(projectId);
    }


    /* tooltip */
    const tooltipTrigger = document.getElementById('git-tooltip');
    if (tooltipTrigger) {
        new bootstrap.Tooltip(tooltipTrigger);
    }
}

function addInfo() {
    const accValue = document.querySelectorAll("[data-accordion-team]");

    accValue.forEach((element) => {
        if (element.ariaExpanded === "true") {
            document.getElementById("TeamId").value = element.name;
            MembersByTeam(element.name);
            let projectId = document.getElementById("ProjectId").value;
            let tablePermissions = document.getElementById("permissionsContainer");
            if (tablePermissions != null) {
                loadPermissionsTable(element.name, projectId);
            }
            element.attributes[4].value = "collapse in";
            element.disabled = true;
        }
        else {
            element.attributes[4].value = "collapse";
            element.disabled = false;
        }
    })
}

function MembersByTeam(id) {
    $.ajax({
        url: '/Project/CreateTeamSelector',
        type: 'POST',
        dataType: 'JSON',
        data: { TeamId: id },
        success: function (data) {
            multipleEdit2.clearChoices();
            var id = document.getElementById('ManagerID').attributes[4].value;
            var newData = [];
            //Create new array on choices select
            $.each(data, function (i, item) {
                if (item.id == id) {
                    newData = [...newData, { value: item.id, label: item.name + " " + item.surname, selected: true }]
                }
                else {
                    newData = [...newData, { value: item.id, label: item.name + " " + item.surname }]
                }

            })
            multipleEdit2.setChoices(
                newData,
                'value',
                'label',
                false,
            );
        }
    });
}

//Set update  project in controller
function editProject() {
    var title = document.getElementById("Title");

    if (!title.value == '') {
        const files = pond.getFiles();

        if (!valideteSizeFiles(files)) {
            return;
        }

        const formData = getFormDataWithFiles("EditProject");

        const filesToDeleteIds = document.getElementById("filesToDelete").value;
        formData.append('filesToDelete', filesToDeleteIds);

        $("#edit-project").modal('toggle');
        $.ajax({
            type: "POST",
            url: "/Project/Edit",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.isSuccess) {

                    if (files.length === 0) {
                        document.getElementById("responseTextCon").innerHTML = response.message;
                        $("#confirmed").modal('show');
                        $('#confirmed').on('hidden.bs.modal', function () {
                            window.location.href = "/Project/Overview/" + response.projectId;
                        });
                        return;
                    }

                    const uploadPromises = [];
                    for (const f of files) {
                        if (f.file && f.origin !== FilePond.FileOrigin.LOCAL) {
                            uploadPromises.push(uploadSingleFile(f.file, response.projectId));
                        }
                    }

                    // Дочекайся завершення всіх завантажень
                    Promise.all(uploadPromises).then(() => {
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
                    }).catch(err => {
                        document.getElementById("responseTextWar").innerHTML = err;
                        $("#warning").modal('show');
                    });
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
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