(function () {
    'use strict'

    FilePond.registerPlugin(
        /*FilePondPluginImagePreview,*/
        FilePondPluginImageExifOrientation,
        FilePondPluginFileValidateSize,
        FilePondPluginFileEncode,
        FilePondPluginImageEdit,
        FilePondPluginFileValidateType,
        FilePondPluginImageCrop,
        FilePondPluginImageResize,
        FilePondPluginImageTransform,
        FilePondPluginGetFile
    );
})();

function setlocalizerOptions() {
    FilePond.setOptions({
        labelIdle: window.translations.FilePond_LabelIdle,
        labelInvalidField: window.translations.FilePond_LabelInvalidField,
        labelFileWaitingForSize: window.translations.FilePond_LabelFileWaitingForSize,
        labelFileSizeNotAvailable: window.translations.FilePond_LabelFileSizeNotAvailable,
        labelFileLoading: window.translations.FilePond_LabelFileLoading,
        labelFileLoadError: window.translations.FilePond_LabelFileLoadError,
        labelFileProcessing: window.translations.FilePond_LabelFileProcessing,
        labelFileProcessingComplete: window.translations.FilePond_LabelFileProcessingComplete,
        labelFileProcessingAborted: window.translations.FilePond_LabelFileProcessingAborted,
        labelFileProcessingError: window.translations.FilePond_LabelFileProcessingError,
        labelTapToCancel: window.translations.FilePond_LabelTapToCancel,
        labelTapToRetry: window.translations.FilePond_LabelTapToRetry,
        labelTapToUndo: window.translations.FilePond_LabelTapToUndo,
        labelButtonRemoveItem: window.translations.FilePond_LabelButtonRemoveItem,
        labelButtonAbortItemLoad: window.translations.FilePond_LabelButtonAbortItemLoad,
        labelButtonRetryItemLoad: window.translations.FilePond_LabelButtonRetryItemLoad,
        labelButtonAbortItemProcessing: window.translations.FilePond_LabelButtonAbortItemLoad,
        labelButtonUndoItemProcessing: window.translations.FilePond_LabelButtonUndoItemProcessing,
        labelButtonRetryItemProcessing: window.translations.FilePond_LabelButtonRetryItemLoad,
        labelButtonProcessItem: window.translations.FilePond_LabelButtonProcessItem,
        labelMaxFileSizeExceeded: window.translations.FilePond_LabelMaxFileSizeExceeded,
        labelMaxFileSize: window.translations.FilePond_LabelMaxFileSize,
    });
}

function initializeFilepond(downloadUrl) {
    setlocalizerOptions() 

    const MultipleElement = document.querySelector('.multiple-filepond');
    const preloadedFilesJson = MultipleElement.getAttribute('data-preloaded-files');
    const preloadedFiles = preloadedFilesJson ? JSON.parse(preloadedFilesJson) : [];

    var pond = FilePond.create(MultipleElement);

    preloadedFiles.forEach(file => {
        pond.addFile(file.source, {
            type: 'local',
            file: file.options.file,
            metadata: { isStub: true }
        });
    });

    pond.on('addfile', (error, file) => {
        if (error) {
            return;
        }

        //Якщо файл доданий користувачем вручну
        const userAddedOrigins = [
            FilePond.FileOrigin.INPUT,
            FilePond.FileOrigin.DROP,
            FilePond.FileOrigin.BROWSED
        ];
        if (userAddedOrigins.includes(file.origin)) {
            // Приховати дефолтну кнопку
            const item = findFileItem(file);
            if (item) {
                const defaultBtn = item.querySelector('.filepond--download-icon');
                if (defaultBtn) {
                    defaultBtn.style.display = 'none';
                }
            }
            return;
        }

        const item = findFileItem(file);
        if (!item) {
            return;
        }

        const infoMainContainer = item.querySelector('.filepond--file-info-main-container');
        if (!infoMainContainer) {
            return;
        }

        const existing = infoMainContainer.querySelector('.filepond--download-icon');
        if (existing) {
            existing.remove();
        }

        const downloadBtn = document.createElement('span');
        downloadBtn.className = 'filepond--download-icon custom-download';
        downloadBtn.title = window.translations.DownloadFile;
        downloadBtn.style.cursor = 'pointer';

        downloadBtn.onclick = () => {
            const fileId = file.serverId || file.source;
            window.location.href = `${downloadUrl}/${fileId}`;
        };

        const filenameEl = infoMainContainer.querySelector('.filepond--file-info-main');
        if (filenameEl) {
            infoMainContainer.insertBefore(downloadBtn, filenameEl);
        } else {
            infoMainContainer.appendChild(downloadBtn);
        }
    });

    let filesToDeleteIds = [];

    pond.on('removefile', (error, file) => {
        if (error) {
            return;
        }

        const fileId = file.serverId || file.id || file.source;
        if (fileId) {
            filesToDeleteIds.push(fileId);
        }

        const filesToDeleteInput = document.getElementById('filesToDelete');
        if (filesToDeleteInput) {
            filesToDeleteInput.value = JSON.stringify(filesToDeleteIds);
        }
    });

    window.pond = pond;
}

//Знаходимо файл
function findFileItem(file) {

    let item = document.querySelector(`.filepond--item[data-filepond-file-id="${file.id}"]`);
    if (item) {
        return item;
    }

    const domId = 'filepond--item-' + file.id.replace(/[^a-z0-9_-]/gi, '');
    item = document.getElementById(domId);
    if (item) {
        return item;
    }

    const items = document.querySelectorAll('.filepond--item');
    for (const el of items) {
        const nameEl = el.querySelector('.filepond--file-info-main');
        if (nameEl && nameEl.textContent === file.filename && file.source) {
            return el;
        }
    }

    return null;
}

function getFormDataWithoutFiles(formId) {
    const form = document.getElementById(formId);
    const formData = new FormData(form);

    formData.delete('files');

    return formData;
}