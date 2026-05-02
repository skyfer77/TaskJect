function initCreateTaskModalScripts() {
    new Choices('#choices-remove-button-member', {
        removeItemButton: true,
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
    let pripritySelect = new Choices('#choices-remove-button-priority', {
        removeItemButton: true,
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
    pripritySelect.setChoiceByValue('1');

    flatpickr("#targetDateTarget", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });

    var toolbarOptionsCreate = [
        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
        [{ 'font': [] }],
        ['bold', 'italic', 'underline', 'strike'],
        ['blockquote', 'code-block'],
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

    var quill = new Quill('#task-descriptioin-editor-create', {
        modules: { toolbar: toolbarOptionsCreate },
        theme: 'snow'
    });

    quill.root.setAttribute("style", "min-height: 10.62rem!important;");

    const hiddenInput = document.getElementById("Description");
    hiddenInput.value = quill.root.innerHTML;

    quill.on('text-change', () => {
        hiddenInput.value = quill.root.innerHTML;
    });

    initializeFilepond("/Task/DownloadFile");

    initGitHubIntegration();
}

function initTaskModalScripts({
    isAdminOrGod,
    canEditTask,
    canSetAssignments,
    statusToCheck,
    isStatus
}) {
    if (isAdminOrGod || canEditTask) {
        initializeChoices(canSetAssignments);
        initializeDatePickers();
    } else if (canSetAssignments) {
        initializeChoicesAssignments();
        setBadges();
    } else {
        setBadges();
    }

    var quill1 = initializeQuill('#task-descriptioin-editor1', 'Description1');

    var quill2;
    if (isAdminOrGod) {
        quill2 = initializeQuill('#task-descriptioin-editor2', 'Description2');
    }

    initGitHubIntegration();
}

function initGitHubIntegration() {
    const isGitHubCheck = document.getElementById("isGitHubCheck");
    const githubOptions = document.getElementById("github-options");
    const titleInput = document.getElementById("title");
    const branchInput = document.getElementById("GitHubBranch");
    const createCheck = document.getElementById("createIssueCheck");
    const issueInput = document.getElementById("GitHubIssueNumber");

    if (isGitHubCheck) {
        isGitHubCheck.addEventListener("change", function () {
            githubOptions.style.display = this.checked ? "block" : "none";
        });
    }

    if (createCheck && issueInput) {
        // Коли вводимо в інпут скидаємо чекбокс
        issueInput.addEventListener("input", function () {
            if (this.value && this.value.trim() !== "") {
                createCheck.checked = false;
            }
        });

        // Коли ставимо чекбокс очищаємо інпут
        createCheck.addEventListener("change", function () {
            if (this.checked) {
                issueInput.value = "";
            }
        });
    }

    initBranchVisibility();
}

function generateBranchName(title) {
    let branchName = title
        .toLowerCase()
        .replace(/\s+/g, "-")
        .replace(/[^a-z0-9\u0400-\u04FF-_]/g, "")
        .replace(/-+/g, "-")
        .replace(/^-+|-+$/g, "");

    if (branchName.length > 128) {
        branchName = branchName.substring(0, 128);
    }

    return branchName;
}

function initBranchVisibility() {
    const statusSelect = document.getElementById("choices-multiple-remove-button4");
    const statusHidden = document.getElementById("Status");
    const wrapper = document.getElementById("branch-input-wrapper");
    const titleInput = document.getElementById("title");
    const branchInput = document.getElementById("GitHubBranch");

    if (!wrapper) {
        return;
    }

    function getCurrentStatus() {
        if (statusSelect) return statusSelect.value;
        if (statusHidden) return statusHidden.value;
        return null;
    }

    function updateBranchVisibility() {
        const currentStatus = getCurrentStatus();

        const badge = document.querySelector("#github-branch-container .badge");
        if (badge) {
            wrapper.style.display = "none";
            return;
        }

        if (currentStatus === "InProgress") {
            wrapper.style.display = "block";

            if (branchInput && titleInput) {
                branchInput.value = generateBranchName(titleInput.value);
            }
        } else {
            wrapper.style.display = "none";
        }
    }

    if (statusSelect) {
        statusSelect.addEventListener("change", updateBranchVisibility);
    }

    if (titleInput) {
        titleInput.addEventListener("input", updateBranchVisibility);
    }

    updateBranchVisibility();
}