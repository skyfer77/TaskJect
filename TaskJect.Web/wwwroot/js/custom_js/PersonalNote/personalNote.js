$(function () {
    "use strict"
    if (document.querySelector('#sort-note')) {
        new Choices('#sort-note', {
            shouldSort: false,
            searchEnabled: false,
            noResultsText: window.translations.noResultsFound,
            noChoicesText: window.translations.noChoicesChooseFrom,
            itemSelectText: window.translations.pressSelect,
        });
    }
});

function initAccordionEvents() {
    document.querySelectorAll('.accordion-item').forEach(item => {
        if (item.dataset.listenersAttached) {
            return;
        }
        item.dataset.listenersAttached = true;

        let shortTitle = item.querySelector('.title.short');
        let fullTitle = item.querySelector('.title.full');
        let previewSpan = item.querySelector('.body-preview');

        item.addEventListener('show.bs.collapse', () => {
            shortTitle.style.display = 'none';
            fullTitle.style.display = 'inline';
            previewSpan.style.display = 'none';
        });

        item.addEventListener('hide.bs.collapse', () => {
            shortTitle.style.display = 'inline';
            fullTitle.style.display = 'none';
            previewSpan.style.display = 'inline';
        });
    });
}

initAccordionEvents();

//Create note
let unsavedNoteExists = false;

function addNewNoteCard() {
    if (unsavedNoteExists) {
        return;
    }

    const container = document.getElementById("note-cards-container");
    if (container.length === 0) {
        return;
    }

    $.ajax({
        url: '/PersonalNote/Create',
        method: 'GET',
        success: function (response) {
            if (response.isSuccess && response.html) {
                const temp = document.createElement('div');
                temp.innerHTML = response.html.trim();
                const newCard = temp.firstElementChild;

                container.insertBefore(newCard, container.firstChild);

                attachAutoResize();

                unsavedNoteExists = true;
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading note card:', error);
        }
    });
}
function removeUnsavedNote(button) {
    const card = button.closest('#unsaved-note');
    if (card) {
        card.remove();
        unsavedNoteExists = false;
    }
}

function saveNewNoteButton(button) {
    const card = button.closest('#unsaved-note');
    const titleTextarea = card.querySelector('.new-title-input');
    const textTextarea = card.querySelector('.new-text-input');
    saveNewNote(titleTextarea, textTextarea);
}

function saveNewNoteOnEnter(event, textarea) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        const card = textarea.closest('#unsaved-note');
        const titleTextarea = card.querySelector('.new-title-input');
        const textTextarea = card.querySelector('.new-text-input');

        if (titleTextarea.value.trim() !== '') {
            saveNewNote(titleTextarea, textTextarea);
        }
    }
}

function saveNewNote(titleTextarea, textTextarea) {
    const title = titleTextarea.value.trim();
    const text = textTextarea.value.trim();
    if (!title) {
        return;
    }

    const tempCard = $(titleTextarea).closest('#unsaved-note');

    $.ajax({
        url: '/PersonalNote/CreateNote',
        method: 'POST',
        data: { Title: title, Text: text},
        success: function (response) {
            if (response.isSuccess && response.html) {
                tempCard.replaceWith(response.html);
                initAccordionEvents();
                unsavedNoteExists = false;

                sortNote();
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading note card:', error);
        }
    });
}

// Title edit 
function editTitle(itemId) {
    const collapseId = `collapse_${itemId}`;
    const headingId = `heading_${itemId}`;

    // Відкриваємо акордеон
    const collapseElement = document.getElementById(collapseId);
    const bsCollapse = bootstrap.Collapse.getOrCreateInstance(collapseElement);
    bsCollapse.show();

    const heading = document.getElementById(headingId);
    const previewBtn = heading.querySelector('.preview-btn');
    const accordionTextareaDiv = heading.querySelector('.accordion-textarea');
    const textarea = accordionTextareaDiv.querySelector('textarea.title-input');

    if (previewBtn && accordionTextareaDiv && textarea) {
        previewBtn.style.display = 'none';
        accordionTextareaDiv.classList.remove('d-none');
        autoResize(textarea);
        textarea.focus();
        textarea.selectionStart = textarea.value.length;
        textarea.setSelectionRange(textarea.value.length, textarea.value.length);
    }
}

let saveTriggeredByEnter = false;

function saveTitleEditOnEnter(event, textarea) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();

        if (saveTriggeredByEnter) {
            return;
        }

        saveTriggeredByEnter = true;

        setTimeout(() => {
            saveTriggeredByEnter = false;
        }, 500);

        saveTitleEdit(textarea);
    }
}

function saveTitleEdit(textarea) {
    const card = textarea.closest('.accordion-item');
    const shortTitle = card.querySelector('.title.short');
    const fullTitle = card.querySelector('.title.full');
    const heading = card.querySelector('.accordion-header');
    const previewBtn = heading.querySelector('.preview-btn');
    const accordionTextareaDiv = heading.querySelector('.accordion-textarea');

    const noteId = card.getAttribute('data-note-id');
    const newTitle = textarea.value.trim();

    if (newTitle === fullTitle.textContent.trim()) {
        accordionTextareaDiv.classList.add('d-none');
        previewBtn.style.display = '';
        return;
    }

    $.ajax({
        url: '/PersonalNote/UpdateNoteTitle',
        method: 'POST',
        data: {
            Id: noteId,
            Title: newTitle,
        },
        success: function (response) {
            if (response.isSuccess) {
                fullTitle.textContent = newTitle;
                shortTitle.textContent = truncate(newTitle, 128);

                sortNote();
            }
            else {
                textarea.value = fullTitle.textContent;
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error("Помилка при оновленні статусу:", error);
        }
    });

    // Приховуємо textarea і показуємо кнопку з текстом
    accordionTextareaDiv.classList.add('d-none');
    previewBtn.style.display = '';

}

// Text edit

function editText(id) {
    const container = document.getElementById(`collapse_${id}`);
    const displayWrapper = container.querySelector('.text-with-btn-wrapper');
    const editWrapper = container.querySelector('.edit-text-wrapper');
    const textarea = editWrapper.querySelector('textarea');

    if (displayWrapper && editWrapper && textarea) {
        displayWrapper.style.display = 'none';
        editWrapper.classList.remove('d-none');
        autoResize(textarea);
        textarea.focus();
        textarea.selectionStart = textarea.value.length;
        textarea.setSelectionRange(textarea.value.length, textarea.value.length);
    }
}

let saveNewTextTriggeredByEnter = false;

function saveTextEditOnEnter(event, textarea) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();

        if (saveNewTextTriggeredByEnter) {
            return;
        }

        saveNewTextTriggeredByEnter = true;

        setTimeout(() => {
            saveNewTextTriggeredByEnter = false;
        }, 500);

        saveTextEdit(textarea);
    }
}
function saveTextEdit(textarea) {
    const editWrapper = textarea.closest('.edit-text-wrapper');
    const card = textarea.closest('.accordion-item');
    const shortText = card.querySelector('.body-preview');
    const displayWrapper = card.querySelector('.text-with-btn-wrapper');
    const fullText = displayWrapper.querySelector('[note-text]');

    const noteId = card.getAttribute('data-note-id');
    const newText = textarea.value.trim();

    if (newText === fullText.textContent.trim()) {
        editWrapper.classList.add('d-none');
        displayWrapper.style.display = '';  
        return;
    }

    $.ajax({
        url: '/PersonalNote/UpdateNoteText',
        method: 'POST',
        data: {
            Id: noteId,
            Text: newText,
        },
        success: function (response) {
            if (response.isSuccess) {
                fullText.textContent = newText;
                shortText.textContent = truncate(newText, 128);

                if (response.updatedAt) {
                    updateDateTodo(card);
                }

                sortNote();
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error("Помилка при оновленні статусу:", error);
        }
    });

    // Приховуємо textarea якщо є текст, показуємо текст якщо є
    if (newText === "") {
        displayWrapper.style.display = 'none';
        editWrapper.classList.remove('d-none');
        autoResize(textarea);
    }
    else {
        editWrapper.classList.add('d-none');
        displayWrapper.style.display = '';  
    }
}

//Delete note
$(document).on("click", '[data-bs-target="#delete-note"]', function () {
    const card = $(this).closest('[data-note-id]');
    const noteId = card.data("note-id");
    const shorTitleNote = card.find(".title.short").first().text().trim();

    $("#noteIdDelete").val(noteId);
    $("#noteTitleDelete").text(shorTitleNote);
});
function deleteNote() {
    const noteId = $("#noteIdDelete").val();

    document.activeElement.blur();
    $("#delete-note").modal('toggle');
    $.ajax({
        url: '/PersonalNote/DeleteNote',
        method: 'POST',
        data: { noteId: noteId },
        success: function (response) {
            if (response.isSuccess) {
                const card = document.querySelector(`[data-note-id="${noteId}"]`);
                if (card) {
                    card.remove();
                }
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading note card:', error);
        }
    });
}

//Sorted 
function sortNote() {
    const selectElement = document.getElementById('sort-note');
    if (!selectElement) {
        return;
    }

    const value = selectElement.value;
    const container = document.getElementById('note-cards-container');
    const cards = Array.from(container.querySelectorAll('.accordion-item'));

    const unsavedCard = document.getElementById('unsaved-note');

    const getDate = (el, attr) => new Date(el.getAttribute(attr));

    const sorted = cards.sort((a, b) => {
        switch (value) {
            case 'new-created':
                return getDate(b, 'data-created-at') - getDate(a, 'data-created-at');
            case 'old-created':
                return getDate(a, 'data-created-at') - getDate(b, 'data-created-at');
            case 'new-updated':
                return getDate(b, 'data-updated-at') - getDate(a, 'data-updated-at');
            case 'old-updated':
                return getDate(a, 'data-updated-at') - getDate(b, 'data-updated-at');
            default:
                return 0;
        }
    });

    container.innerHTML = '';

    if (unsavedCard) {
        container.appendChild(unsavedCard);
    }

    sorted.forEach(card => container.appendChild(card));
}

//additional features

function updateDateTodo(card) {
    const now = new Date();
    const locale = window.currentCulture || 'en';
    const formatted = now.toLocaleDateString(locale);

    const updatedAtContainer = card.querySelector(".updated-at");
    if (updatedAtContainer) {
        updatedAtContainer.textContent = formatted;
    }
    card.setAttribute("data-updated-at", now.toISOString());
}
function autoResize(textArea) {
    textArea.style.height = "auto";
    textArea.style.height = textArea.scrollHeight + "px";
}

function attachAutoResize() {
    document.querySelectorAll(".title-input, .text-input, .accordion-textarea, .new-text-input, .new-title-input").forEach(textarea => {
        textarea.removeEventListener("input", handleInput);
        textarea.addEventListener("input", handleInput);
    });
    function handleInput(e) {
        const textarea = e.target;

        // Автоматичне змінення висоти textarea
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';
    }
}

function handleInput(e) {
    autoResize(e.target);
}

attachAutoResize();

function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}

function truncate(str, n) {
    return str.length > n ? str.slice(0, n) + "..." : str;
}