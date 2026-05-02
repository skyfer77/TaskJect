(function () {
    "use strict"
    /* StartDate Picker */
    flatpickr("#startDate", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });

    /* EndDate Picker */
    flatpickr("#endDate", {
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });

    //Not use in view
    /* multi select with remove button */
    /* const multipleCancelButton = new Choices(
        '#assigned-team-members',
        {
            allowHTML: true,
            removeItemButton: true,
        }
    );* /

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
    var quill = new Quill('#project-descriptioin-editor', {
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

    /* passing unique values */
    //var textUniqueVals = new Choices('#choices-text-unique-values', {
    //    allowHTML: true,
    //    paste: false,
    //    duplicateItemsAllowed: false,
    //    editItems: true,
    //    searchPlaceholderValue: `${window.translations.Search}...`,
    //    noResultsText: window.translations.noResultsFound,
    //    noChoicesText: window.translations.noChoicesChooseFrom,
    //    itemSelectText: window.translations.pressSelect,
    //});

})();