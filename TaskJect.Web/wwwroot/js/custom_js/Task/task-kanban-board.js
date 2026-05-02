let dragulaInstance = null;

function initDragula() {
    const containers = Array.from(document.querySelectorAll("[data-status-task]"));
    dragulaInstance = dragula(containers);

    dragulaInstance.on("drop", function (el, target) {
        const id = el.dataset.idTask;
        const newStatus = parseInt(target.dataset.statusTask);

        $.post("/Task/EditStatus", { id, status: newStatus }, function () {
            applyAllFiltersAndSort();
        });
    });
}

initDragula();

let isColumnsLoading = false;

/**
 * Завантажує конкретні колонки за статусами
 * @param {Array} statuses - масив статусів колонок, наприклад [1,2,4]
 *  * @param {string} projectId - ід проекту
 * @param {string} period - період для фільтра
 */
async function loadColumns(statuses = [], projectId, period = null) {
    if (isColumnsLoading) {
        return;
    }
    if (!Array.isArray(statuses) || statuses.length === 0) {
        return;
    }

    const filter = $("#periodFilter");
    if (!period) {
        period = filter.val();
    }

    if (parseInt(period) === 0) {
        statuses.forEach(status => {
            const column = document.querySelector(`[data-status-task="${status}"]`);
            if (column) {
                column.innerHTML = "";
            }
        });
        updateAllCounters();
        return;
    }

    isColumnsLoading = true;

    filter.prop("disabled", true);

    const loadPromises = [];

    statuses.forEach(status => {
        const column = document.querySelector(`[data-status-task="${status}"]`);
        if (!column) {
            return;
        }

        loadPromises.push(loadSingleColumn(column, status, period, projectId));
    });

    await Promise.all(loadPromises);

    applyAllFiltersAndSort();
    updateEmptyColumns();
    updateAllCounters();

    filter.prop("disabled", false);
    isColumnsLoading = false;
}

const columnLoaderHtml = `
<div class="d-flex justify-content-center align-items-center"
     style="min-height: 12.5rem; background: white; border-radius: .5rem;">
    <div class="spinner-border text-primary"
         style="width: 3rem; height: 3rem;"
         role="status">
        <span class="visually-hidden">${window.translations.loading}...</span>
    </div>
</div>`;

async function loadSingleColumn(column, status, period, projectId) {
    if (!column) {
        return;
    }

    column.innerHTML = columnLoaderHtml;

    try {
        const result = await $.get("/Task/LoadColumn", { status, period, projectId });

        column.innerHTML = "";

        if (result && result.html) {
            column.insertAdjacentHTML("beforeend", result.html);
        }
    }
    catch (err) {
        console.error("Load error:", err);
    }
}

function updateEmptyColumns() {
    const columns = document.querySelectorAll("[data-status-task]");

    columns.forEach(col => {
        const cards = Array.from(col.children);
        const visibleCards = cards.filter(c => !c.classList.contains("hide"));
        const hasCustomCards = cards.some(c => c.classList.contains("card") && c.classList.contains("custom-card"));

        const btnId = col.getAttribute("data-view-btn");
        const nextEl = btnId ? document.querySelector(`#${btnId}`)?.nextElementSibling : null;

        if (cards.length === 0 || visibleCards.length === 0) {
            col.classList.add("task-Null");
            if (nextEl) {
                nextEl.classList.add("d-none");
            }
        } else {
            col.classList.remove("task-Null");
            if (nextEl) {
                nextEl.classList.remove("d-none");
            }
        }

        if (!hasCustomCards || visibleCards.length === 0) {
            col.classList.add("task-hide");
            if (nextEl) {
                nextEl.classList.add("d-none");
            }
        } else {
            col.classList.remove("task-hide");
            if (nextEl) {
                nextEl.classList.remove("d-none");
            }
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    setInterval(() => {
        updateEmptyColumns();
    }, 100);
});

function applyFiltersToCard(card, period) {
    if (!card) {
        return;
    }

    const searchValue = $('[data-search]').val().toLowerCase();
    const overdueChecked = $('#overdueTasks').is(':checked');

    const title = card.querySelector("[data-title]")?.innerText.toLowerCase() || "";
    const complexity = card.querySelector("[data-complexity-task]")?.innerText.toLowerCase() || "";
    const priority = card.querySelector("[data-priority]")?.innerText.toLowerCase() || "";
    const member = card.querySelector("[data-member-task]")?.innerText.toLowerCase() || "";

    const completed = card.querySelector("[complited-date]")?.value;
    const target = card.querySelector("[target-date]")?.value;

    const status = parseInt(card.dataset.taskCardStatus);

    let show = true;

    // ARCHIVED + period = None
    if (period === 0 && status === 4) {
        show = false;
    }

    if (searchValue) {
        show =
            title.includes(searchValue) ||
            complexity.includes(searchValue) ||
            priority.includes(searchValue) ||
            member.includes(searchValue);
    }

    if (overdueChecked && target) {
        const t = new Date(target);
        const now = new Date();

        if (completed) {
            show = show && (new Date(completed) > t);
        }
        else {
            show = show && now > t;
        } 
    }

    card.classList.toggle("hide", !show);
}

function compareTasks(a, b, sortType) {
    let av, bv;

    switch (sortType) {
        case 'Newest':
            av = new Date(a.querySelector("[create-date]")?.value);
            bv = new Date(b.querySelector("[create-date]")?.value);
            return bv - av;

        case 'Date Added':
            av = new Date(a.querySelector("[create-date]")?.value);
            bv = new Date(b.querySelector("[create-date]")?.value);
            return av - bv;

        case 'A - Z':
            av = a.querySelector("[data-title]")?.innerText.toLowerCase();
            bv = b.querySelector("[data-title]")?.innerText.toLowerCase();
            return av.localeCompare(bv);

        default:
            return 0;
    }
}

function sortVisibleTasks() {
    const sortType = document.getElementById('choices-single-default')?.value;
    if (!sortType) {
        return;
    }

    document.querySelectorAll("[data-status-task]").forEach(column => {
        const cards = Array.from(
            column.querySelectorAll("[data-task-card]:not(.hide)")
        );

        cards.sort((a, b) => compareTasks(a, b, sortType));

        cards.forEach(card => column.appendChild(card));
    });
}

function applyAllFiltersAndSort() {
    const period = parseInt($("#periodFilter").val());
    
    const cards = document.querySelectorAll("[data-task-card]");
    cards.forEach(card => applyFiltersToCard(card, period));

    sortVisibleTasks();
    updateEmptyColumns();
    updateAllCounters();
    toggleArchiveCompletedButton();
}

function countTask(column, status) {
    const count = column.querySelectorAll("[data-task-card]:not(.hide)").length;
    const badge = document.querySelector(`[data-count-badge="${status}"]`);

    if (badge) {
        badge.textContent = count;
    }
}

function updateAllCounters() {
    document.querySelectorAll("[data-status-task]").forEach(col => {
        const status = col.dataset.statusTask;
        countTask(col, status)
    });
}

document.querySelectorAll('.kanban-tasks').forEach(col => {
    new SimpleBar(col, { autoHide: true });
});

function toggleArchiveCompletedButton() {
    const doneColumn = document.querySelector('[data-status-task="3"]');
    const btn = document.getElementById("archiveCompletedBtn");

    if (!doneColumn || !btn) {
        return;
    }

    const visibleTasks = doneColumn.querySelectorAll('[data-task-card]:not(.hide)');

    if (visibleTasks.length > 0) {
        btn.classList.remove("d-none");
    } else {
        btn.classList.add("d-none");
    }
}


$("#archiveCompletedBtn").on("click", async function () {
    const projectId = $("#projectData").data("project-id");
    const period = parseInt($("#periodFilter").val());

    const doneColumn = document.querySelector('[data-status-task="3"]');

    if (!doneColumn || doneColumn.querySelectorAll('[data-task-card]:not(.hide)').length === 0) {
        showWarning(window.translations.NoCompletedTasksArchive);
        return;
    }

    $(this).prop("disabled", true);

    try {
        const res = await $.ajax({
            url: "/Task/ArchiveCompleted",
            type: "POST",
            data: { projectId },
        });

        if (!res.isSuccess) {
            showWarning(res.message);
            return;
        }

        const archiveColumn = document.querySelector('[data-status-task="4"]');

        const taskIds = Array.isArray(res.taskIds) ? res.taskIds : [];

        taskIds.forEach(id => {
            const card = doneColumn.querySelector(`[data-id-task="${id}"]`);
            if (!card) return;

            card.dataset.taskCardStatus = "4";

            if (period === 0) {
                card.classList.add("hide");
            }

            archiveColumn?.appendChild(card);
        });

        applyAllFiltersAndSort();
    }
    finally {
        $(this).prop("disabled", false);
    }
});