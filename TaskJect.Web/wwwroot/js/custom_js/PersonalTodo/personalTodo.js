$(function () {
    "use strict"
    if (document.querySelector('#sort-todo')) {
        new Choices('#sort-todo', {
            shouldSort: false,
            searchEnabled: false,
            noResultsText: window.translations.noResultsFound,
            noChoicesText: window.translations.noChoicesChooseFrom,
            itemSelectText: window.translations.pressSelect,
        });
    }
});
//Masonry
const containerMasonry = document.querySelector('#todo-cards-container');

const msnry = new Masonry(containerMasonry, {
    itemSelector: '.col-xxl-4',
    columnWidth: '.col-xxl-4',
    percentPosition: true,
    gutter: 0,
    horizontalOrder: true
});

let todoMasonry = msnry;

function updateMasonryReloadItems() {
    if (todoMasonry) {
        todoMasonry.reloadItems();
    }
}

function updateMasonryLayout() {
    if (todoMasonry) {
        todoMasonry.layout();
    }
}

//Create, edit, delete Todo task

function editTaskText(element) {
    const listItem = element.closest('li');
    const span = listItem.querySelector('.task-text');
    const formCheck = listItem.querySelector('.form-check');
    const textarea = listItem.querySelector('.task-input');

    textarea.value = span.textContent;
    textarea.setAttribute('data-original-text', span.textContent.trim());
    formCheck.classList.add('d-none');
    textarea.classList.remove('d-none');

    autoResize(textarea);
    textarea.focus();
    textarea.setSelectionRange(textarea.value.length, textarea.value.length);
}

function toggleTaskStatus(checkbox) {
    const li = checkbox.closest('li');
    const taskId = li.getAttribute('data-task-id');
    const isDone = checkbox.checked;

    const card = li.closest('[data-todo-id]');
    const todoId = card?.getAttribute('data-todo-id');

    $.ajax({
        url: '/PersonalTodo/ToggleTaskStatus',
        method: 'POST',
        data: {
            Id: taskId,
            IsDone: isDone,
            TodoId: todoId,
        },
        success: function (response) {
            if (response.isSuccess && response.updatedAt) {
                updateDateTodo(card);
                sortTodo();
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error("Помилка при оновленні статусу:", error);
        }
    });
}

function saveTaskEditOnEnter(event, textarea) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        saveTaskEdit(textarea);
    }
}

function saveTaskEdit(textarea) {
    const listItem = textarea.closest('li');
    const span = listItem.querySelector('.task-text');
    const formCheck = listItem.querySelector('.form-check');
    const newText = textarea.value;
    const originalText = textarea.getAttribute('data-original-text')?.trim();

    if (newText === originalText) {
        textarea.classList.add('d-none');
        formCheck.classList.remove('d-none');
        textarea.removeAttribute('data-original-text');
        return;
    }

    if (newText !== "") {
        span.textContent = newText;
        const taskId = listItem.getAttribute('data-task-id');
        const card = listItem.closest('[data-todo-id]');
        const todoId = card?.getAttribute('data-todo-id');

        $.ajax({
            url: '/PersonalTodo/UpdateTodoTask',
            method: 'POST',
            data: {
                Text: newText,
                Id: taskId,
                TodoId: todoId,
            },
            success: function (response) {
                if (response.isSuccess) {
                    span.textContent = newText;
                    span.classList.remove("d-none");
                    textarea.classList.add("d-none");
                    textarea.removeAttribute('data-original-text');

                    if (response.updatedAt) {
                        updateDateTodo(card);
                        sortTodo();
                    }

                    updateMasonryLayout()
                }
                else {
                    showWarning(response.message);
                }
            },
            error: function (error) {
                console.error('Error loading todo card:', error);
            }
        });
    }

    textarea.classList.add('d-none');
    formCheck.classList.remove('d-none');
    textarea.removeAttribute('data-original-text');
}

function deleteTask(taskId) {
    const taskEl = document.querySelector(`[data-task-id="${taskId}"]`);
    const card = taskEl.closest('[data-todo-id]');
    const todoId = card?.getAttribute('data-todo-id');

    $.ajax({
        url: '/PersonalTodo/DeleteTodoTask',
        method: 'POST',
        data: {
            taskId: taskId,
            todoId: todoId,
        },
        success: function (response) {
            if (response.isSuccess) {
                taskEl.remove();
                updateTaskCounters(card);

                if (response.updatedAt) {
                    updateDateTodo(card);
                    sortTodo();
                }

                updateMasonryLayout();
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading todo card:', error);
        }
    });
}

let saveNewTaskTriggeredByEnter = false;

function addNewTaskOnEnter(event, textarea, todoId) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();

        if (saveNewTaskTriggeredByEnter) {
            return;
        }

        saveNewTaskTriggeredByEnter = true;
        addNewTask(textarea, todoId);

        // Через 500ms скидаємо флаг, щоб можна було знову зберігати
        setTimeout(() => {
            saveNewTaskTriggeredByEnter = false;
        }, 500);

        return;
    }
}

function addNewTask(element, todoId) {
    const li = element.closest('li');
    const textarea = li.querySelector('.new-task-input');
    const text = textarea.value.trim();
    if (!text) {
        return;
    }

    const ul = li.parentElement;
    const taskLis = ul.querySelectorAll('li[data-task-id]');

    let maxSortOrder = 0;
    taskLis.forEach(task => {
        const sortOrder = parseInt(task.dataset.sortOrder || "0", 10);
        if (!isNaN(sortOrder) && sortOrder > maxSortOrder) {
            maxSortOrder = sortOrder;
        }
    });

    const newSortOrder = maxSortOrder + 1;

    $.ajax({
        url: '/PersonalTodo/CreateTodoTask',
        method: 'POST',
        data: {
            Text: text,
            TodoId: todoId,
            SortOrder: newSortOrder
        },
        success: function (response) {
            if (response.isSuccess && response.html) {
                //Треба, щоб з рядка зробити реальний DOM елемент
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = response.html.trim();
                const newLi = tempDiv.firstElementChild;
                ul.insertBefore(newLi, li);
                textarea.value = "";

                const card = document.querySelector(`[data-todo-id="${todoId}"]`);
                updateTaskCounters(card);

                if (response.updatedAt) {
                    updateDateTodo(card);
                    sortTodo();
                }

                let addNewTask = card.querySelector('.new-task-input')
                addNewTask.focus();

                autoResize(textarea);

                updateMasonryLayout();
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (err) {
            console.error("Error loading todo task:", err);
        }
    });
}

//Create, edit, delete Todo
let unsavedTodoExists = false;

function addNewTodoCard() {
    if (unsavedTodoExists) {
        return;
    }

    const container = document.getElementById("todo-cards-container");
    if (container.length === 0) {
        return;
    }

    $.ajax({
        url: '/PersonalTodo/Create',
        method: 'GET',
        success: function (response) {
            if (response.isSuccess && response.html) {
                const temp = document.createElement('div');
                temp.innerHTML = response.html.trim();
                const newCard = temp.firstElementChild;

                container.insertBefore(newCard, container.firstChild);

                attachAutoResize();
                
                sortTodo();
                updateMasonryReloadItems();
                updateMasonryLayout();

                unsavedTodoExists = true;
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading todo card:', error);
        }
    });
}

function removeUnsavedTodo(button) {
    const card = button.closest('.unsaved-todo');
    if (card) {
        card.remove();
        unsavedTodoExists = false;
        updateMasonryLayout();
    }
}

function saveNewTodoTitleButton(button) {
    const listItem = button.closest('.unsaved-todo');
    const textarea = listItem.querySelector('.new-title-input');
    saveNewTodoTitle(textarea);
}

function saveNewTodoTitleOnEnter(event, textarea) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        saveNewTodoTitle(textarea);
        return;
    }
}

function saveNewTodoTitle(textarea) {
    const title = textarea.value.trim();
    if (!title) {
        return;
    }

    const tempCard = $(textarea).closest('.unsaved-todo');

    $.ajax({
        url: '/PersonalTodo/CreateTodo',
        method: 'POST',
        data: { Title: title },
        success: function (response) {
            if (response.isSuccess && response.html) {
                tempCard.replaceWith(response.html);
                unsavedTodoExists = false;

                sortTodo();

                updateMasonryReloadItems();
                updateMasonryLayout();
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading todo card:', error);
        }
    });
}

$(document).on("click", '[data-bs-target="#delete-todo"]', function () {
    const card = $(this).closest('[data-todo-id]');
    const todoId = card.data("todo-id");
    const todoName = card.find(".todo-title[data-title]").first().text().trim();

    $("#todoIdDelete").val(todoId);
    $("#todoTitleDelete").text(todoName);
});

function deleteTodo() {
    const todoId = $("#todoIdDelete").val();

    $("#delete-todo").modal('toggle');
    $.ajax({
        url: '/PersonalTodo/DeleteTodo',
        method: 'POST',
        data: { todoId: todoId },
        success: function (response) {
            if (response.isSuccess) {
                const card = document.querySelector(`[data-todo-id="${todoId}"]`);
                if (card) {
                    todoMasonry.remove(card);
                    card.remove();
                    updateMasonryLayout();
                }
            }
            else {
                showWarning(response.message);
            }
        },
        error: function (error) {
            console.error('Error loading todo card:', error);
        }
    });
}

function editTitleText(span) {
    const container = span.closest(".flex-fill");
    const textarea = container.querySelector(".title-input");

    if (span && textarea) {
        textarea.setAttribute('data-original-text', span.textContent.trim());
        span.classList.add("d-none");
        textarea.classList.remove("d-none");
        autoResize(textarea);
        textarea.focus();
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
        saveTitleEdit(textarea);

        // Через 500ms скидаємо флаг, щоб можна було знову зберігати
        setTimeout(() => {
            saveTriggeredByEnter = false;
        }, 500);

        return;
    }
}
function saveTitleEdit(textarea) {
    const cardHeader = textarea.closest(".card-header");
    const span = cardHeader.querySelector("p[data-title]");
    const card = textarea.closest("[data-todo-id]");
    const todoId = card?.getAttribute("data-todo-id");

    const newText = textarea.value.trim();
    const originalText = textarea.getAttribute('data-original-text')?.trim();

    if (newText === originalText) {
        span.classList.remove("d-none");
        textarea.classList.add('d-none');
        textarea.removeAttribute('data-original-text');
        return;
    }

    if (newText !== "" && todoId) {
        $.ajax({
            url: '/PersonalTodo/UpdateTodo',
            method: 'POST',
            data: { Id: todoId, Title: newText },
            success: function (response) {
                if (response.isSuccess) {
                    span.textContent = newText;
                    span.classList.remove("d-none");
                    textarea.classList.add("d-none");
                    textarea.removeAttribute('data-original-text');

                    if (response.updatedAt) {
                        updateDateTodo(card);
                        sortTodo();
                    }

                    updateMasonryReloadItems();
                    updateMasonryLayout();
                }
                else {
                    showWarning(response.message);
                }
            },
            error: function (error) {
                console.error('Error loading todo card:', error);
            }
        });
    }
}

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

//Sorted 
function sortTodo() {
    const selectElement = document.getElementById('sort-todo');
    if (!selectElement) {
        return;
    }

    const value = selectElement.value;
    const container = document.getElementById('todo-cards-container');
    const allCards = Array.from(container.querySelectorAll('.col-xxl-4'));

    const unsavedCard = allCards.find(card => card.classList.contains('unsaved-todo'));

    const cards = allCards.filter(card => !card.classList.contains('unsaved-todo'));


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

    updateMasonryReloadItems(); 
    updateMasonryLayout();
}


//additional features
function autoResize(textArea) {
    textArea.style.height = "auto";
    textArea.style.height = textArea.scrollHeight + "px";
}

function attachAutoResize() {
    document.querySelectorAll(".title-input, .task-input, .new-task-input, .new-title-input").forEach(textarea => {
        textarea.removeEventListener("input", handleInput);
        textarea.addEventListener("input", handleInput);
    });
    function handleInput(e) {
        const textarea = e.target;

        // Автоматичне змінення висоти textarea
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';

        updateMasonryLayout();
    }
}

function handleInput(e) {
    autoResize(e.target);
}

attachAutoResize();

function updateTaskCounters(todoCardElement) {
    const completed = todoCardElement.querySelectorAll('.form-check-input:checked').length;
    const total = todoCardElement.querySelectorAll('.form-check-input').length;

    updateCountDisplay(todoCardElement, completed, total);
    updateTitleClass(todoCardElement, completed, total);
    updateProgressBar(todoCardElement, completed, total);
    updateCompletionBadge(todoCardElement, completed, total);
}

function updateCountDisplay(element, completed, total) {
    element.querySelector('.completed-count').textContent = completed;
    element.querySelector('.total-count').textContent = total;
}

function updateTitleClass(element, completed, total) {
    const titleElement = element.querySelector('[data-title]');
    if (completed === total && total !== 0) {
        titleElement.classList.add('completed-title-todo');
    } else {
        titleElement.classList.remove('completed-title-todo');
    }
}

function updateProgressBar(element, completed, total) {
    const progressBar = element.querySelector('.progress-bar-todo');
    if (!progressBar) {
        return;
    }

    const percent = total > 0 ? Math.round((completed / total) * 100) : 0;
    progressBar.style.width = `${percent}%`;
    progressBar.setAttribute('aria-valuenow', completed);
}

function updateCompletionBadge(element, completed, total) {
    const badgeContainer = element.querySelector('.todo-badge-container');
    if (!badgeContainer) {
        return;
    }

    const existingBadge = badgeContainer.querySelector('.badge');

    if (completed === total && total !== 0) {
        if (!existingBadge) {
            const badge = document.createElement('span');
            badge.className = 'badge bg-success-transparent ms-1';
            badge.textContent = window.translations.Completed;
            badgeContainer.appendChild(badge);
        }
    } else {
        if (existingBadge) {
            existingBadge.remove();
        }
    }
}

document.addEventListener('change', function (e) {
    if (e.target.classList.contains('form-check-input')) {
        const card = e.target.closest('[data-todo-id]');
        updateTaskCounters(card);
    }
}); 
    
function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}