$(document).ready(function () {
    var toolbarOptionsCreate = [
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
    window.quill = new Quill('#organization-contact-descriptioin', {
        modules: {
            toolbar: toolbarOptionsCreate
        },
        theme: 'snow'
    });

    //Set value description
    quill.root.setAttribute("style", "min-height: 10.62rem!important;");

    const hiddenInput = document.getElementById("descriptionSendToUs");
    hiddenInput.value = quill.root.innerHTML;

    quill.on('text-change', () => {
        hiddenInput.value = quill.root.innerHTML;
    });

    $(document).on("click", '[data-bs-target="#delete-user-row"]', function () {
        var item = $(this).data('todo');
        document.getElementById('userIdDelete').value = item.id;
        document.getElementById('userNameDelete').textContent = item.name + " " + item.surname;
    });
});

var organizationRequest;
function Edit(id) {
    if (organizationRequest) {
        organizationRequest.abort();
    }

    modalWindowLoad("edit-organization", window.translations.EditOrganization)

    $("#edit-organization").modal("show");

    organizationRequest = $.ajax({
        type: "POST",
        url: "/Organization/Edit/",
        data: { id: id },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#edit-organization .modal-content").html(newContent);
        },
        complete: function () {
            organizationRequest = null;
        }
    });
    hiddenModalWindow('#edit-organization')
}

function validEmail(email) {
    var error = document.getElementById("error-email");
    const regex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!regex.test(email)) {
        $("#error-email").css("display", "block");
        error.innerHTML = window.translations.invalidEmailAddress;
        return false;
    }
    $("#error-email").css("display", "none");
    return true;
}
function validPhoneNumber(phone) {
    var error = document.getElementById("error-phone");
    var regex = /^\+?3?8?(0[\s\.-]\d{2}[\s\.-]\d{3}[\s\.-]\d{2}[\s\.-]\d{2})$/
    if (!regex.test(phone)) {
        $("#error-phone").css("display", "block");
        error.innerHTML = window.translations.invalidPhoneNumber;
        return false;
    }
    $("#error-phone").css("display", "none");
    return true;
}

function EditOrganization() {
    var organizationName = document.getElementById("EditOrganizationName");
    var organizationEmail = document.getElementById("EditOrganizationEmail");
    var organizationPhone = document.getElementById("EditOrganizationPhone");
    if (organizationName.value.trim() === '') {
        alert(window.translations.organizationNameRequired);
        return;
    }

    var isEmailValid = true;
    if (organizationEmail.value.trim() !== '') {
        isEmailValid = validEmail(organizationEmail.value);
    }

    var isPhoneValid = true;
    if (organizationPhone.value.trim() !== '') {
        isPhoneValid = validPhoneNumber(organizationPhone.value);
    }

    if (!isEmailValid || !isPhoneValid) {
        return;
    }

    $("#edit-organization").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Organization/EditOrganization",
        data: $("#EditOrganization").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                $("#organization-card").load(window.location.href + " #organization-card > *", function () {
                    showConfirmed(response.message);
                })
                    
            } else {
                showWarning(response.message);
            }
        }
    });
}


function DeleteUser() {
    $("#delete-user-row").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Organization/DeleteUser",
        data: $("#DeleteUserRow").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                $("#table-organization-user").load(window.location.href + " #table-organization-user > *", function () {
                    showConfirmed(response.message);
                    document.querySelectorAll("#choices-user-oraganization-role").forEach(element => {
                        new Choices(element, {
                            searchEnabled: false,
                            placeholderValue: window.translations.SelectRole,
                            shouldSort: false,
                            allowHTML: true,
                            position: 'auto',
                            searchPlaceholderValue: `${window.translations.Search}...`,
                            noResultsText: window.translations.noResultsFound,
                            noChoicesText: window.translations.noChoicesChooseFrom,
                            itemSelectText: window.translations.pressSelect,
                        });
                    });   
                });
                countUser--;
                updateProgressBar("userDisplay", "userProgressBar", countUser, maxUser, "{0}/{1}");
            } else {
                showWarning(response.message);
            }
        }
    });
}
let isSending = false;
function sendToUs() {
    if (isSending) return;
    var title = document.getElementById("Title");
    if (title.value !== '') {
        isSending = true; 

        $.ajax({
            type: "POST",
            url: "/Organization/SendToUs",
            data: $("#SendToUs").serialize(),
            success: function (response) {
                if (response.isSuccess) {
                    $("#contact-us").load(window.location.href + " #contact-us > *", function () {
                        showConfirmed(response.message);
                    });
                    showConfirmed(response.message);
                    $("#contact-form").modal('toggle');
                    $("#confirmed").on('hidden.bs.modal', function () {
                        document.getElementById("SendToUs").reset();
                        $("#descriptionSendToUs").val("");
                        window.quill.setContents([]);
                        location.reload();
                    });
                } else {
                    $("#contact-form").modal('toggle');
                    showWarning(response.message);
                }
            },
            complete: function () {
                isSending = false;
            }
        });
    }
}
function ChangePlan() {
    const btn = document.querySelector('[data-can-change-plan="true"]');
    if (!btn) return;

    modalWindowLoad("change-modal", window.translations.ChangePlan, `style="--bs-modal-width: 880px;"`, "");

    $("#change-modal").modal("show");
    $.ajax({
        type: "POST",
        url: "/Organization/ChangePlan/",
        success: function (response) {
            $("#change-modal .modal-content").replaceWith(response);
            $("#change-modal").modal("show");
            initChoicesForChangeModal();
        }
    });

    hiddenModalWindow('#change-modal');
}

function initChoicesForChangeModal() {
    document.querySelectorAll('#change-modal .choices-period').forEach(select => {
        if (select.choicesInstance) {
            select.choicesInstance.destroy();
        }

        select.choicesInstance = new Choices(select, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false
        });

        select.value = '1';
        select.dispatchEvent(new Event('change'));
    });

    document.querySelectorAll('#change-modal .choices-period').forEach(select => {
        select.addEventListener('change', function () {
            const planCode = this.dataset.planCode;
            const periodValue = parseInt(this.value, 10);
            const btn = document.querySelector(`#subscribeBtn-${planCode}`);

            if (btn) {
                btn.dataset.period = periodValue;
            }
        });
    });
}

function Unsubscribe() {
    let organizationCode = $("#unsubscribe").data("organization");
    $("#change-modal").modal("toggle");
    $.ajax({
        type: "POST",
        url: "/Organization/Unsubscribe/",
        data: { id: organizationCode },
        success: function (response) {
            if (response.isSuccess) {
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        }
    });
}

function initializeChoicesInContainer(containerSelector = document) {

    const selects = (containerSelector instanceof Element || typeof containerSelector === 'string')
        ? (typeof containerSelector === 'string'
            ? document.querySelectorAll(`${containerSelector} .choices-multiple-remove-button4`)
            : containerSelector.querySelectorAll('.choices-multiple-remove-button4'))
        : document.querySelectorAll('.choices-multiple-remove-button4');

    selects.forEach(select => {
        try {
            if (select.choicesInstance && typeof select.choicesInstance.destroy === 'function') {
                select.choicesInstance.destroy();
            }

            const choicesInstance = new Choices(select, {
                searchEnabled: false,
                placeholderValue: window.translations.SelectRole,
                shouldSort: false,
                allowHTML: true,
                position: 'auto',
                searchPlaceholderValue: `${window.translations.Search}...`,
                noResultsText: window.translations.noResultsFound,
                noChoicesText: window.translations.noChoicesChooseFrom,
                itemSelectText: window.translations.pressSelect,
            });

            select.choicesInstance = choicesInstance;

            requestAnimationFrame(() => {
                select.style.visibility = 'visible';
                select.style.opacity = '1';
            });
        } catch (e) {
            console.error('Failed to init Choices for select', select, e);
        }
    });
}

document.addEventListener('DOMContentLoaded', () => initializeChoicesInContainer());

function UnlinkGitHub() {
    $("#unlink-github").modal("toggle");
    $.ajax({
        type: "POST",
        url: "/GitHub/unlink",
        success: function (response) {
            if (response.isSuccess) {
                showConfirmed(response.message);
            } else if (response.message) {
                showWarning(response.message);
            }
        }
    });
}

function handleSubscribeClick(btn) {
    handleSubscriptionRequest('/wayforpay/subscription', btn);
}

function handleChangeClick(btn) {
    handleSubscriptionRequest('/wayforpay/change-subscription', btn);
}

async function handleSubscriptionRequest(url, btn) {
    const planCode = btn.dataset.planCode;
    const planName = btn.dataset.planName;
    const period = parseInt(btn.dataset.period, 10);

    const w = window.open('', '_blank');

    try {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ PlanCode: planCode, PlanName: planName, PeriodType: period })
        });

        const contentType = res.headers.get('content-type') || '';

        if (contentType.includes('application/json')) {
            const data = await res.json();

            if (!data.success) {
                showWarning(data.message);
                w.close();
                return;
            }
        }

        const html = await res.text();

        w.document.write(html);
        w.document.close();
    } catch (err) {
        console.error('Subscription request failed:', err);
        showWarning(window.translations.AnErrorOccurredWhileProcessing);
        w.close();
    }
}

function unsubscribe() {
    $("#change-modal").modal("toggle");
    $.ajax({
        type: "GET",
        url: "/wayforpay/unsubscription",
        success: function (response) {
            if (response.isSuccess) {
                showConfirmed(response.message);
            } else {
                showWarning(response.message);
            }
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    const params = new URLSearchParams(window.location.search);
    const payment = params.get('payment');
    const reason = params.get('reason');

    if (payment === 'success') {
        showConfirmed(window.translations.PaymentWasSuccessful);
    }
    else if (payment === 'fail') {
        const message = reason ? decodeURIComponent(reason) : window.translations.ThePaymentFailed;
        showWarning(message);
    }
});

function showConfirmed(message) {
    $("#responseTextCon").text(message);
    $("#confirmed").modal('show');
}

function showWarning(message) {
    $("#responseTextWar").text(message);
    $("#warning").modal('show');
}