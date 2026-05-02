function initializeChoices(canSetAssignments = true) {
    new Choices('#choices-multiple-remove-button4', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
    if (canSetAssignments) {
        initializeChoicesAssignments();
    }
    new Choices('#choices-multiple-remove-button6', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
    initializeFilepond("/Task/DownloadFile")
}

function initializeChoicesAssignments() {
    new Choices('#choices-multiple-remove-button5', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
}

function initializeDatePickers() {
    flatpickr("#targetDateStart", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });
    flatpickr("#targetDateEnd", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });
}

function setBadges() {
    $('span[name="Low-over"]').addClass('badge bg-info-transparent');
    $('span[name="Medium-over"]').addClass('badge bg-success-transparent');
    $('span[name="High-over"]').addClass('badge bg-danger-transparent');
    $('span[name="OnHold"]').addClass('bg-warning-transparent');
    $('span[name="NotStarted"]').addClass('bg-primary-transparent');
    $('span[name="InProgress"]').addClass('bg-info-transparent');
    $('span[name="OnReview"]').addClass('bg-danger-transparent');
    $('span[name="Done"]').addClass('bg-success-transparent');
    $('span[name="Archived"]').addClass('bg-warning-transparent');
}

function getToolbarOptions() {
    return [
        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
        [{ 'font': [] }],
        ['bold', 'italic', 'underline', 'strike'],
        ['blockquote', 'code-block'],
        [{ 'header': 1 }, { 'header': 2 }],
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
}

function initializeQuill(selector, descriptionId) {
    var toolbarOptions = getToolbarOptions();

    var quill = new Quill(selector, {
        modules: {
            toolbar: toolbarOptions
        },
        theme: 'snow'
    });

    quill.root.setAttribute("style", "min-height: 10.62rem!important;");

    const hiddenInput = document.getElementById(descriptionId);
    hiddenInput.value = quill.root.innerHTML;

    quill.on('text-change', () => {
        hiddenInput.value = quill.root.innerHTML;
    });

    return quill;
}
