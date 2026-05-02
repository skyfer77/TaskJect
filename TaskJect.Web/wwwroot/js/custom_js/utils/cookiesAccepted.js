const CookieConsentType = {
    NecessaryOnly: 1,
    Functional: 2,
    Analytics: 4,
    Performance: 8,
    Advertisement: 16,
    All: 1 | 2 | 4 | 8 | 16,
};
const cookieDescriptions = {
    ".AspNetCore.Antiforgery.*": {
        description: window.translations.ProtectionAgainstCSRFAttacksASP,
        duration: "Session",
        category: "Necessary",
        appearsIf: () => true
    },
    ".AspNetCore.Identity.Application": {
        description: window.translations.ASPNETCoreIdentityAuthenticationCookie,
        duration: "Session",
        category: "Necessary",
        appearsIf: () => true
    },
    "cookieConsent": {
        description: window.translations.StoresUserCookiePreferences,
        duration: window.translations.months6,
        category: "Necessary",
        appearsIf: () => true
    },
    ".AspNetCore.Culture": {
        description: window.translations.SavesUsersLanguage,
        duration: window.translations.year1,
        category: "Functional",
        appearsIf: () => true
    }
};
function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + encodeURIComponent(value) + expires + "; path=/";
}

function getCookie(name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length);
    }
    return null;
}

function getAllCookies() {
    const rawCookies = document.cookie.split('; ');
    return rawCookies.map(entry => {
        const [name, value] = entry.split('=');
        return { name, value };
    });
}
function getCookieInfo() {
    const clientCookies = getAllCookies();

    if (clientCookies.some(c => c.name === 'cookieConsent') || clientCookies !== undefined) {
        return Promise.resolve(processCookies(clientCookies));
    }

    return $.ajax({
        url: '/Account/GetCookies',
        type: 'GET',
        dataType: 'json',
        xhrFields: { withCredentials: true }
    }).then(serverCookies => {
        const mergedCookiesMap = new Map();

        clientCookies.forEach(c => mergedCookiesMap.set(c.name, c));

        serverCookies.forEach(c => mergedCookiesMap.set(c.name, c));

        const allCookies = Array.from(mergedCookiesMap.values());

        processCookies(allCookies)
    });
}
function processCookies(allCookies) {
    const cookieInfos = [];

    allCookies.forEach(cookie => {
        const info = Object.entries(cookieDescriptions).find(([key]) => {
            if (key.endsWith('*')) {
                return cookie.name.startsWith(key.slice(0, -1));
            }
            return cookie.name === key;
        });

        const meta = info ? info[1] : {
            description: window.translations.DescriptionUnavailable,
            duration: window.translations.Unknown,
            category: "Uncategorized"
        };

        cookieInfos.push({
            name: cookie.name,
            description: meta.description,
            duration: meta.duration,
            category: meta.category
        });
    });

    Object.entries(cookieDescriptions).forEach(([key, meta]) => {
        const exists = allCookies.some(c => {
            if (key.endsWith('*')) {
                return c.name.startsWith(key.slice(0, -1));
            }
            return c.name === key;
        });

        if (!exists) {
            const appears = typeof meta.appearsIf === 'function' ? meta.appearsIf(allCookies) : true;
            if (appears) {
                cookieInfos.push({
                    name: key.endsWith('*') ? key.slice(0, -1) : key,
                    description: meta.description,
                    duration: meta.duration,
                    category: meta.category
                });
            }
        }
    });

    return cookieInfos;
}
function renderCookiesToAccordion() {
    getCookieInfo().then(cookies => {
        const necessaryContainer = document.getElementById("necessaryCookies");
        const functionalContainer = document.getElementById("functionalCookies");

        const necessaryCookies = cookies?.filter(c => c.category === "Necessary");
        const functionalCookies = cookies?.filter(c => c.category === "Functional");

        renderCookies(necessaryCookies, necessaryContainer);
        renderCookies(functionalCookies, functionalContainer);
    });
}

function renderCookies(cookieArray, container) {
    cookieArray?.forEach((cookie, index) => {
        const div = document.createElement("div");
        const isLast = index === cookieArray.length - 1;

        if (!isLast) {
            div.classList.add("mb-3");
        }

        div.innerHTML = `
          <div><strong>${window.translations.Title}:</strong> ${cookie.name}</div>
          <div><strong>${window.translations.Duration}:</strong> ${cookie.duration}</div>
          <div><strong>${window.translations.Description}:</strong> ${cookie.description}</div>
          ${!isLast ? "<hr/>" : ""}
        `;

        container?.appendChild(div);
    });
}

renderCookiesToAccordion();

const modal = document.getElementById('cookieModal');

function showBanner() {
    const banner = document.getElementById('cookieBanner');
    if (banner) {
        banner.style.display = 'block';
        document.body.classList.add('banner-visible');
    }
}

function hideBanner() {
    const banner = document.getElementById('cookieBanner');
    if (banner) {
        banner.style.display = 'none';
        document.body.classList.remove('banner-visible');
    }
}

function acceptCookies(fromModal = false) {
    setConsent(CookieConsentType.All, fromModal);
}

function rejectCookies(fromModal = false) {
    setConsent(CookieConsentType.NecessaryOnly, fromModal);
}

//TODO: Added new Cookies rework this
function customCookies() {
    const switches = document.getElementById('switch-lg').checked;
    if (switches) {
        setConsent(CookieConsentType.Functional, true);
    } else {
        rejectCookies(true);
    }
}

function setConsent(type, fromModal) {
    setCookie('cookieConsent', type, 365);
    hideBanner();
    if (fromModal) {
        const modalEl = document.getElementById('cookieModal');
        if (modalEl) {
            const modalInstance = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
            modalInstance.toggle();
        }
    }
}

function showModal() {
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
    hideBanner();
}

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form[action*="SetLanguage"]').forEach(form => {
        form.addEventListener('submit', function (e) {
            const consent = parseInt(getCookie('cookieConsent'), 10);
            const needsModal = isNaN(consent) || consent === CookieConsentType.NecessaryOnly;

            if (needsModal) {
                e.preventDefault();
                showModal();
            }
        });
    });
});


modal?.addEventListener('hidden.bs.modal', () => {
    if (!getCookie('cookieConsent')) {
        showBanner();
    }
});

window.addEventListener('load', () => {
    const consent = parseInt(getCookie('cookieConsent'), 10);
    const validValues = [CookieConsentType.All, CookieConsentType.NecessaryOnly, CookieConsentType.Functional];
    if (!validValues.includes(consent)) {
        showBanner();
    } else {
        hideBanner();
    }
});
