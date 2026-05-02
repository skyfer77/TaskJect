$(document).ready(function () {
    const teamButtons = document.querySelectorAll("[data-accordion-team]");
    const teamIdInput = document.getElementById("TeamId");
    const managerSelect = document.getElementById("ManagerID");
    const invalidFeedback = document.getElementById("ManagerID-error");

    if (teamButtons.length > 0) {
        const firstButton = teamButtons[0];
        firstButton.disabled = true;
    }
    
    if (invalidFeedback) {
        checkManagerSelection(managerSelect, invalidFeedback);
        managerSelect.addEventListener('change', function () {
            checkManagerSelection(managerSelect, invalidFeedback);
        });
    }

    initAccordionHandlers(teamButtons, teamIdInput, managerSelect);

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
});

let managerChoices;

function initAccordionHandlers(buttons, teamIdInput, managerSelect) {
    if (!managerSelect || buttons.length === 0) return;

    initializeChoices(managerSelect);
    buttons.forEach(button => {
        button.addEventListener('click', () => {
            handleAccordionClick(buttons, button, teamIdInput, managerSelect);
            const invalidFeedback = document.getElementById("ManagerID-error");
            if (invalidFeedback) {
                checkManagerSelection(managerSelect, invalidFeedback);
            }
        });
    });

    const firstButton = buttons[0];
    handleAccordionClick(buttons, firstButton, teamIdInput, managerSelect, true);
}

function initializeChoices(select) {
    if (managerChoices) {
        managerChoices.destroy();
    }
    managerChoices = new Choices(select, {
        shouldSort: false,
        placeholder: true,
        placeholderValue: window.translations?.selectManager,
        searchPlaceholderValue: window.translations?.Search,
        noResultsText: window.translations?.noResultsFound,
        noChoicesText: window.translations?.noChoicesChooseFrom,
        itemSelectText: window.translations?.pressSelect
    });
    const choicesContainer = select.closest('.choices');
    if (choicesContainer) {
        choicesContainer.classList.add('mb-0');
    }
}

function checkManagerSelection(managerSelect, invalidFeedback) {
    if (managerSelect.value === '') {
        invalidFeedback.style.display = 'block';
    } else {
        invalidFeedback.style.display = 'none';
    }
}

function handleAccordionClick(buttons, button, teamIdInput, managerSelect, force = false) {
    const currentId = button.id;
    const currentCollapse = document.getElementById(`collapsePrimary_${currentId}`);

    if (!force && currentCollapse.classList.contains('show')) {
        return;
    }

    closeAllAccordions();
    openAccordion(currentCollapse);

    teamIdInput.value = currentId;
    updateButtonStates(buttons, button);

    MembersByTeam(currentId, function (choices) {
        updateChoices(managerSelect, choices);
    });

    let tablePermissions = document.getElementById("permissionsContainer");
    if (tablePermissions != null) {
        loadPermissionsTable(currentId);
    }
}

function closeAllAccordions() {
    document.querySelectorAll('.accordion-collapse.show').forEach(open => {
        open.classList.remove('show');
        open.classList.add('collapsing');
        setTimeout(() => {
            open.classList.remove('collapsing');
            open.classList.add('collapse');
        }, 200);
    });
}

function openAccordion(element) {
    element.classList.remove('collapse');
    element.classList.add('show');
}

function updateButtonStates(buttons, activeButton) {
    buttons.forEach(b => b.disabled = false);
    activeButton.disabled = true;
}

function updateChoices(select, choices) {
    if (managerChoices) {
        managerChoices.clearStore();
        managerChoices.setChoices([{
            value: '',
            label: window.translations?.selectManager,
            selected: true,
            disabled: true
        }]);
        managerChoices.setChoices(choices, 'value', 'label', true);
    }
}

function MembersByTeam(id, callback) {
    $.ajax({
        url: '/Project/CreateTeamSelector',
        type: 'POST',
        dataType: 'JSON',
        data: { TeamId: id },
        success: function (data) {
            const newData = data.map(item => ({
                value: item.id,
                label: item.name + " " + item.surname
            }));
            callback(newData);
        }
    });
}

function CreateProject() {
    var title = document.getElementById("Title");
    var manager = document.getElementById("ManagerID");

    if (!(title.value == '' || title.value == title.defaultValue) && !manager.value == '') {

        const files = pond.getFiles();

        if (!valideteSizeFiles(files)) {
            return;
        }

        const formData = getFormDataWithFiles("CreateProject");

        $.ajax({
            type: "POST",
            url: "/Project/CreateProject",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.isSuccess) {

                    if (files.length === 0) {
                        console.log("ff")
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
                        document.getElementById("responseTextCon").innerHTML = response.message;
                        $("#confirmed").modal('show');
                        $('#confirmed').on('hidden.bs.modal', function () {
                            window.location.href = "/Project/Overview/" + response.projectId;
                        });
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
