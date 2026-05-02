$(document).ready(function () {
    //Add style to priority status task and project
    $('span[name="Low"]').addClass('bg-info-transparent');
    $('span[name="Medium"]').addClass('bg-success-transparent');
    $('span[name="High"]').addClass('bg-danger-transparent');
    $('span[name="OnHold"]').addClass('bg-warning-transparent');
    $('span[name="NotStarted"]').addClass('bg-primary-transparent');
    $('span[name="InProgress"]').addClass('bg-info-transparent');
    $('span[name="OnReview"]').addClass('bg-danger-transparent');
    $('span[name="Done"]').addClass('bg-success-transparent');
    $('span[name="Archived"]').addClass('bg-warning-transparent');
    //Add style border status project
    $('[data-status="OnHold"]').addClass('border-warning');
    $('[data-status="NotStarted"]').addClass('border-primary');
    $('[data-status="InProgress"]').addClass('border-info');
    $('[data-status="OnReview"]').addClass('border-danger');
    $('[data-status="Done"]').addClass('border-success');
    $('[data-status="Archived"]').addClass('border-warning');

    // basic datatable
    let table = $('#datatable-basic').DataTable({
        searching: false,
        paging: false,
        info: false,
        language: {
            searchPlaceholder: `${window.translations.Search}...`,
            sSearch: '',
            info: formatString(window.translations.dataTableInfo, '_START_', '_END_', '_TOTAL_'),
            infoEmpty: window.translations.noEntriesToShow,
            zeroRecords: window.translations.noMatchingRecordsFound,
        },
        columnDefs: [
            {
                bSortable: false,
                aTargets: [-1]
            }
        ],
    });

    setupShortDataTableLocalization(table);

    loadBirthdaysContent();
});

var profileRequest;
function Overview(id, userId) {
    if (profileRequest) {
        profileRequest.abort();
    }

    modalWindowLoad("overview-task", window.translations.OverviewTask, ``, 'modal-lg')

    $("#overview-task").modal("show");

    profileRequest = $.ajax({
        type: "POST",
        url: "/Profile/OverviewTask/",
        data: { id: id, userId: userId },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#overview-task .modal-content").html(newContent);
            initStyle();
        },
        complete: function () {
            profileRequest = null;
        }
    });
    hiddenModalWindow('#overview-task')
}

function Edit(id) {
    if (profileRequest) {
        profileRequest.abort();
    }

    modalWindowLoad("edit-profile", window.translations.EditProfile)

    $("#edit-profile").modal("show");

    profileRequest = $.ajax({
        type: "POST",
        url: "/Profile/Edit/",
        data: { id: id },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#edit-profile .modal-content").html(newContent);
            flatpickr("#targetDateStart", {
                dateFormat: "Y-m-d",
                minDate: "1950-01",
                locale: window.currentCulture === "uk" ? "uk" : "en"
            });
        },
        complete: function () {
            profileRequest = null;
        }
    });
    hiddenModalWindow('#edit-profile')
}

function initStyle() {
    $('span[name="Low-over"]').addClass('badge bg-info-transparent');
    $('span[name="Medium-over"]').addClass('badge bg-success-transparent');
    $('span[name="High-over"]').addClass('badge bg-danger-transparent');
    $('span[name="OnHold-over"]').addClass('text-warning');
    $('span[name="NotStarted-over"]').addClass('text-primary');
    $('span[name="InProgress-over"]').addClass('text-info');
    $('span[name="OnReview-over"]').addClass('text-danger');
    $('span[name="Done-over"]').addClass('text-success');
    $('span[name="Archived-over"]').addClass('text-warning');
}
//Validation email phone card profile edit
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
//function validCardNamber(card) {
//    var error = document.getElementById("error-card");
//    var visaRegEx = /^([0-9]{4}[\s-][0-9]{4}[\s-][0-9]{4}[\s-][0-9]{4}|([0-9]{12}(?:[0-9]{3,4})?))$/
//    if (!visaRegEx.test(card)) {
//        $("#error-card").css("display", "block");
//        error.innerHTML = "Invalid card number, xxxx-xxxx-xxxx-xxxx";
//        return false;
//    }
//    $("#error-card").css("display", "none");
//    return true;
//}
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
//Set update profile in controller
function EditProfile() {
    var userName = document.getElementById("EditUserName");
    var userSurname = document.getElementById("EditUserSurname");
    var userEmile = document.getElementById("EditUserEmile");
    /*var userCard = document.getElementById("EditUserCard");*/
    var userPhone = document.getElementById("EditUserPhone");
    if (!userName.value == '' && !userSurname.value == '' && !userEmile.value == ''
        && validEmail(userEmile.value) /*&& validCardNamber(userCard.value)*/
        && validPhoneNumber(userPhone.value))
    {
        //if (/[\s-]/.test(userCard.value)) {
        //    userCard.value = userCard.value.replaceAll(/[\s-]/gi, '');
        //}
        $("#edit-profile").modal('toggle');
        $.ajax({
            type: "POST",
            url: "/Profile/EditProfile",
            data: $("#EditProfile").serialize(),
            success: function (response) {
                if(response.isSuccess) {
                    $("#user-card").load(window.location.href + " #user-card ");
                    document.getElementById("responseTextCon").innerHTML = response.message;
                    $("#confirmed").modal('show');
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
}

function loadBirthdaysContent()
{
    $.ajax({
        url: '/Profile/ColleaguesBirthdays',
        type: 'GET',
        success: function (data) {
            if (data.trim()) {
                $('#colleagues-birthdays-container').html(data).fadeIn();
            }
        }
    });
}

$('#confirmUnload').click(function () {
    var userId = $('#confirmModal').data('user-id');

    $.ajax({
        type: "POST",
        url: "/Profile/UnconnectTelegram",
        data: { id: userId },
        success: function (response) {
            if (response.isSuccess) {
                document.getElementById("responseTextCon").innerHTML = response.message;
                $("#confirmed").modal('show');
                $('#confirmed').on('hidden.bs.modal', function () {
                    location.reload();
                });
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }

            var modal = bootstrap.Modal.getInstance(document.getElementById('confirmModal'));
            modal.hide();
        }
    });
});

$('#confirmModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var userId = button.data('user-id');
    $(this).data('user-id', userId);
});

function deleteAccountInformation()
{
    document.body.focus();
    $("#delete-account").modal('hide');
    $.ajax({
        url: '/Profile/DeleteAccountInformation',
        type: 'GET',
        success: function (response) {
            if (response.isSuccess) {
                window.location.href = response.redirectUrl;
            } else {
                if (response.html && response.html.trim() !== "") {
                    const containerId = "dynamic-modal-container";

                    if (!document.getElementById(containerId)) {
                        $("body").append(`<div id="${containerId}"></div>`);
                    }

                    $("#" + containerId).html(response.html);

                    if (document.getElementById("select-user")) {
                        initSelect();
                    }

                    const modalId = $("#" + containerId).find(".modal").attr("id");
                    $("#" + modalId).modal('show');
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        }
    });
}

function confirmSwitchAndDelete() {
    const userId = $("#select-user").val();

    if (!userId) {
        return;
    }

    document.body.focus();
    $("#switch-delete-user-modal").modal('hide');
    $.ajax({
        url: '/Organization/SwitchDeleteUser',
        type: 'POST',
        data: { userId: userId},
        success: function (response) {
            if (response.isSuccess) {
                window.location.href = response.redirectUrl;
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        }
    });
}

function confirmDeleteOrganization() {
    document.body.focus();
    $("#delete-organiztion-modal").modal('hide');
    $.ajax({
        url: '/Organization/DeleteMyOrganization',
        type: 'GET',
        success: function (response) {
            if (response.isSuccess) {
                window.location.href = response.redirectUrl;
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        }
    });
}

function initSelect() {
    new Choices('#select-user', {
        searchPlaceholderValue: `${window.translations.Search}...`,
        noResultsText: window.translations.noResultsFound,
        noChoicesText: window.translations.noChoicesChooseFrom,
        itemSelectText: window.translations.pressSelect,
    });
}