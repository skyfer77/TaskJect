function initEditOrganizationAppeal() {
    new Choices('#choices-multiple-remove-button1', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
    let quill = initializeQuill();
    toggleRejectedDescription();

    $('#choices-multiple-remove-button1').change(function () {
        toggleRejectedDescription();
    });

    const hiddenInput = document.getElementById("Description");
    if (quill) {
        hiddenInput.value = quill.root.innerHTML;
    }

    quill.on('text-change', () => {
        hiddenInput.value = quill.root.innerHTML;
    });
}