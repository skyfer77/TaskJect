function modalWindowLoad(id, title, attributeStyle = '', classStyle = '') {
    $("#modal-window").html(`
        <div class="modal fade" id="${id}" tabindex="-1" aria-hidden="true" ${attributeStyle}>
            <div class="modal-dialog modal-dialog-centered ${classStyle}">
                <div class="modal-content">
                    <div class="modal-header">
                        <h6 class="modal-title">${title}</h6>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    ${spinnerLoad()}
                </div>
            </div>
        </div>
    `);
}
function spinnerLoad() {
    return `<div class="modal-body text-center">
                <div class="spinner-border text-primary" role="status">
                    <span class="sr-only">${window.translations.loading}...</span>
                </div>
            </div>`
}
function hiddenModalWindow(id) {
    $(id).on('hidden.bs.modal', function () {
        $('body').focus();
    });

    $(id).on('hide.bs.modal', function () {
        $(this).removeAttr('aria-hidden');
    });

    $(id).on('hide.bs.modal', function () {
        $(this).attr('inert', 'true');
    });

    $(id).on('hidden.bs.modal', function () {
        $(this).removeAttr('inert');
    });
}