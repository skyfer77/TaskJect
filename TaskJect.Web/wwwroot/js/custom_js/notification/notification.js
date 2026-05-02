$(function () {
    "use strict"
    loadUserNotifications();
});
/* signalR */
const logLevel = window.location.hostname === "localhost"
    ? signalR.LogLevel.Information
    : signalR.LogLevel.Error;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")// cookie auth працює автоматично
    .configureLogging(logLevel)
    .build();

const container = document.getElementById("header-notification-scroll");
const simpleBarInstance = new SimpleBar(container, { autoHide: true });

//Слухаємо повідомлення від серверу
connection.on("ReceiveNotification", function (html) {
    if (container) {
        container.querySelector('.simplebar-content').insertAdjacentHTML("afterbegin", html);
        updateNotificationCount();
        updateTotlaNotificationCount();
        updateAllTimes();
        simpleBarInstance.recalculate();
    }
});

connection.start()
    .catch(err => console.error(err));
/* signalR */
/* load, edit, delete Notification  */
function loadUserNotifications() {
    $.ajax({
        url: '/Notification/GetUserNotifications',
        type: 'GET',
        success: function (html) {
            if (html.trim()) {
                container.querySelector('.simplebar-content').insertAdjacentHTML("beforeend", html);
                updateNotificationCount();
                updateTotlaNotificationCount();
                updateAllTimes();
                simpleBarInstance.recalculate();
            }
        }
    });
}
function reviewed(el, id) {
    if (!el.classList.contains("not-reviewed")) {
        return;
    }
    $.ajax({
        url: '/Notification/Reviewed',
        type: 'POST',
        data: { id },
        success: function (response) {
            if (response.isSuccess) {
                el.classList.remove("not-reviewed");
                updateNotificationCount();
            }
        }
    });
}

function reviewedAll() {
    const items = document.querySelectorAll('#header-notification-scroll li.not-reviewed');
    if (items.length === 0) {
        return;
    }

    $.ajax({
        url: '/Notification/ReviewedAll',
        type: 'GET',
        success: function (response) {
            if (response.isSuccess) {
                items.forEach(li => li.classList.remove('not-reviewed'));
                updateNotificationCount();
            }
        }
    });
}

function deleteNotification(el, id) {
    $.ajax({
        url: '/Notification/Delete',
        type: 'POST',
        data: { id },
        success: function (response) {
            if (response.isSuccess) {
                el.remove();
                simpleBarInstance.recalculate();
                updateNotificationCount();
                updateTotlaNotificationCount();
            }
        }
    });
}
/* load Notification */
/* for notifications dropdown */
const dropdown = document.querySelector('.notifications-dropdown');

dropdown.addEventListener('show.bs.dropdown', () => {
    const container = document.getElementById("header-notification-scroll");
    const simpleBarInstance = SimpleBar.instances.get(container);
    if (simpleBarInstance) {
        //dropdown повністю відображено ставимо скрол вгору
        setTimeout(() => {
            simpleBarInstance.getScrollElement().scrollTop = 0;
        }, 0);
    }
});

container.addEventListener("click", function (e) {
    const button = e.target.closest(".item-notification-close");
    if (button) {
        e.preventDefault();
        e.stopPropagation();
        button.closest("li").remove();
        updateNotificationCount();
    }
});
function updateNotificationCount() {
    const container = document.getElementById("header-notification-scroll");
    const unreadItems = container.querySelectorAll("li.not-reviewed");
    const allItems = container.querySelectorAll("li");
    const totla = allItems.length;
    const count = unreadItems.length;

    const notificationBadge = document.getElementById("notification-icon-badge");
    if (notificationBadge) {
        notificationBadge.innerText = `${count}`
    };

    updateReviewedTooltip(count);

    const emptyBlock = document.querySelector(".empty-item");
    const badge = document.getElementById("notification-icon-badge");
    const containerBtn = document.getElementById("container-btn-reviewed");

    toggleHidden(emptyBlock, totla !== 0);
    toggleHidden(badge, count === 0);
    toggleHidden(containerBtn, count === 0);
}

function toggleHidden(el, condition) {
    el?.classList.toggle("d-none", condition);
}

function updateTotlaNotificationCount() {
    const container = document.getElementById("header-notification-scroll");
    const allItem = container.querySelectorAll("li");
    const total = allItem.length;
    const notificationData = document.getElementById("notifiation-data");
    if (notificationData) {
        const baseText = notificationData.innerText.split('(')[0].trim();
        notificationData.innerText = total > 0 ? `${baseText} (${total})` : baseText;
    }
}

const reviewedBtn = document.getElementById('reviewed-all-btn');
const tooltip = document.getElementById('reviewed-tooltip');
if (reviewedBtn && tooltip) {
    reviewedBtn.addEventListener('mouseenter', () => tooltip.classList.add('show'));
    reviewedBtn.addEventListener('focus', () => tooltip.classList.add('show'));
    reviewedBtn.addEventListener('mouseleave', () => tooltip.classList.remove('show'));
    reviewedBtn.addEventListener('blur', () => tooltip.classList.remove('show'));
}

function updateReviewedTooltip(count) {
    const tooltipText = tooltip.querySelector('.tooltip-text');
    if (tooltipText) {
        tooltipText.textContent = `${count} ${window.translations.Unread}`;
    }
}
/* for notifications dropdown */
/* set time ago */
// Підключення плагіна dayjs
dayjs.extend(dayjs_plugin_relativeTime);
dayjs.extend(dayjs_plugin_localizedFormat);
dayjs.extend(dayjs_plugin_utc);
dayjs.extend(dayjs_plugin_timezone);
dayjs.locale(window.currentCulture);

function updateAllTimes() {
    document.querySelectorAll(".time-ago").forEach(el => {
        const createdStr = el.getAttribute("data-created");
        if (createdStr) {
            const date = dayjs.utc(createdStr).local();
            el.innerText = date.fromNow();
        }
    });
}

// Оновлення кожну хвилину
setInterval(updateAllTimes, 60000);

/* set time ago */