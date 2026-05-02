function loadRepo(projectId, addButton = false, edit = false) {
    const $repoContainer = $("#repo-container");

    $repoContainer.html(`
        <div class="d-flex justify-content-center align-items-center form-group">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">${window.translations.loading}...</span>
            </div>
        </div>
    `);

    $.ajax({
        url: `/GitHub/repos?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            if (response.isSuccess && response.html) {
                $repoContainer.html(response.html);

                const select = document.getElementById("repoSelect");
                initChoices(select);

                if (addButton && !edit) {
                    $("#edit-select").remove();
                    $repoContainer.after(
                        `<button type="button" id="new-select" class="btn btn-primary mt-2 ms-auto" onclick="selectRepo()">${window.translations.LinkGit}</button>`
                    );
                }
                else if (edit) {
                    $("#new-select").remove();
                    $repoContainer.after(
                        `<button type="button" id="edit-select" class="btn btn-primary mt-2 ms-auto" data-bs-toggle="modal" data-bs-target="#unlink-github-repo">${window.translations.LinkGit}</button>`
                    );
                }
            }
        },
        error: function (xhr) {
            console.error(xhr.responseText);
            $repoContainer.html(`<div class="text-danger">${window.translations.ErrorLoadingRepository}</div>`);
        }
    });
}

function selectRepo() {
    const form = $("#SelectRepo");
    const projectId = form.find("input[name=ProjectId]").val();
    const selected = $("#repoSelect").val();

    const $modal = $("#unlink-github-repo");

    if ($modal.hasClass("show")) {
        $modal.modal("toggle");
    }

    if (!selected) {
        unselectRepo(projectId);
        return;
    }

    $.ajax({
        url: `/GitHub/linkRepoToProject`,
        type: 'POST',
        data: form.serialize(),
        success: function (html) {
            $("#new-select").remove();
            $("#repo-container").html(html);
            let message = window.translations.RepositoryBindingSuccessful;
            showConfirmed(message)
        },
        error: function (xhr) {
            console.error(xhr.responseText);
        }
    });
}

function unselectRepo(projectId) {
    $.ajax({
        url: `/GitHub/unlinkRepoToProject`,
        type: 'POST',
        data: { projectId },
        success: function (html) {
            $("#edit-select").remove();
            $("#repo-container").html(html);
            const container = document.getElementById("select-repo");
            if (container) {
                const projectId = container.getAttribute('data-project-id');
                loadRepo(projectId, true);
            }
            let message = window.translations.RepositoryUnbindingSuccessful;
            showConfirmed(message)
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    showWarning(response.value);
                } catch {
                    showWarning(xhr.responseText);
                }
            }
            else if (xhr.status === 404) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    console.error(response.value);
                } catch {
                    console.error(xhr.responseText);
                }
            }
            else {
                showWarning(window.translations.ErrorUnlinkingRepository);
            }
        }
    });
}

function initChoices(select) {
    new Choices(select, {
        shouldSort: false,
        placeholder: true,
        //placeholderValue: window.translations?.selectManager,
        searchPlaceholderValue: window.translations?.Search,
        noResultsText: window.translations?.noResultsFound,
        noChoicesText: window.translations?.noChoicesChooseFrom,
        itemSelectText: window.translations?.pressSelect
    });
    const choicesContainer = select.closest('.choices');
    if (choicesContainer) {
        choicesContainer.classList.add('mb-0');
        const dropdown = choicesContainer?.querySelector('.choices__list--dropdown');
        if (dropdown) {
            dropdown.classList.add('z-index-1050');
        }
    }
}

function showConfirmed(message) {
    $("#responseTextCon").text(message);
    $("#confirmed").modal('show');
}

function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}