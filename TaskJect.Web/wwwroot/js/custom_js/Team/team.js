(function () {
    'use strict';
    AdjustTeamCardMaxHeight();
})();
$(document).on("click", '[data-bs-target="#delete-team"]', function () {
    var item = $(this).data('todo');
    var obj = Object.getOwnPropertyDescriptors(item);
    document.getElementById('teamID').value = obj.idTeam.value;
    document.getElementById('teamName').innerHTML = obj.titleTeam.value;
});
//Set value delete member on team id team and id member and title team and member name surname
$(document).on("click", '[data-bs-target="#delete-member-on-team"]', function () {
    var item = $(this).data('todo-member');
    console.log(item)
    var obj = Object.getOwnPropertyDescriptors(item);
    console.log(obj.memberId.value, obj.teamID.value, obj.teamName.value, obj.memberName.value)
    document.getElementById('memberID').value = obj.memberId.value;
    document.getElementById('IDteam').value = obj.teamID.value;
    document.getElementById('nameTeam').innerHTML = obj.teamName.value + ": " + obj.memberName.value;
});

//Create team 
function CreateTeam() {
    var teamName = document.getElementById("CreateTeamName");
    if (!teamName.value == '') {
        $("#create-team").modal('toggle');
        $.ajax({
            type: "POST",
            url: "/Team/CreateTeam",
            data: $("#CreateTeam").serialize(),
            success: function (response) {
                if (response.isSuccess) {
                    var oldBar = SimpleBar.instances.get(document.getElementById('teams-nav'));
                    if (oldBar) {
                        oldBar.unMount();
                    }
                    $("#teams-nav").load(window.location.href + " #teams-nav > *", function () {
                        document.getElementById("responseTextCon").innerHTML = response.message;
                        $("#confirmed").modal('show');
                        new SimpleBar(document.getElementById('teams-nav'), { autoHide: true });
                    });
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
}
var teamRequest;
//View modal edit team by id
function Edit(id) {
    if (teamRequest) {
        teamRequest.abort();
    }

    modalWindowLoad("edit-team", window.translations.EditTeam)

    $("#edit-team").modal("show");

    teamRequest = $.ajax({
        type: "POST",
        url: "/Team/EditTeam/",
        data: { id: id },
        success: function (response) {
            let newContent = $(response).find(".modal-content").html();
            $("#edit-team .modal-content").html(newContent);
            $("#add-members-team .modal-content").html(newContent);
            new Choices('#choices-multiple-remove-button', {
                removeItemButton: true,
                searchPlaceholderValue: `${window.translations.Search}...`,
                noResultsText: window.translations.noResultsFound,
                noChoicesText: window.translations.noChoicesChooseFrom,
                itemSelectText: window.translations.pressSelect,
            });
        },
        complete: function () {
            teamRequest = null;
        }
    });
    hiddenModalWindow('#edit-team')
}
function ManageTeam() {
    $("#edit-team").modal('toggle');
    var teamName = document.getElementById("EditTeamName");
    if (!teamName.value == '') {
        $.ajax({
            url: '/Team/ManageTeam',
            type: 'POST',
            data: $("#EditTeam").serialize(),
            success: function (response) {
                if (response.isSuccess) {
                    var oldBar = SimpleBar.instances.get(document.getElementById('teams-nav'));
                    if (oldBar) {
                        oldBar.unMount();
                    }
                    $("#teams-nav").load(window.location.href + " #teams-nav > *", function () {
                        document.getElementById("responseTextCon").innerHTML = response.message;
                        $("#confirmed").modal('show');
                        new SimpleBar(document.getElementById('teams-nav'), { autoHide: true });
                    });
                } else {
                    document.getElementById("responseTextWar").innerHTML = response.message;
                    $("#warning").modal('show');
                }
            }
        });
    }
    return false;
}
//Delete team by id team
function DeleteTeam() {
    $("#delete-team").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Team/DeleteTeam",
        data: $("#DeleteTeam").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                var oldBar = SimpleBar.instances.get(document.getElementById('teams-nav'));
                if (oldBar) {
                    oldBar.unMount();
                }
                $("#teams-nav").load(window.location.href + " #teams-nav > *", function () {
                    document.getElementById("responseTextCon").innerHTML = response.message;
                    $("#confirmed").modal('show');
                    new SimpleBar(document.getElementById('teams-nav'), { autoHide: true });
                });
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        }
    });
}
//Delete member on team by id member and id team
function DeleteMemberOnTeam() {
    $("#delete-member-on-team").modal('toggle');
    $.ajax({
        type: "POST",
        url: "/Team/DeleteOnTeam",
        data: $("#DeleteOnTeam").serialize(),
        success: function (response) {
            if (response.isSuccess) {
                var oldBar = SimpleBar.instances.get(document.getElementById('teams-nav'));
                if (oldBar) {
                    oldBar.unMount();
                }
                $("#teams-nav").load(window.location.href + " #teams-nav > *", function () {
                    document.getElementById("responseTextCon").innerHTML = response.message;
                    $("#confirmed").modal('show');
                    new SimpleBar(document.getElementById('teams-nav'), { autoHide: true });
                });
            } else {
                document.getElementById("responseTextWar").innerHTML = response.message;
                $("#warning").modal('show');
            }
        }
    });
}
function AdjustTeamCardMaxHeight() {
    const cardHeight = 289.5; //user card height
    let countCards = document.getElementById("countCard").value;

    let teamNavHeight = Math.ceil(countCards / 3) * cardHeight;

    $('#teams-nav').css('min-height', '500px');
    $('#teams-nav').css('max-height', `${teamNavHeight}px`);
    $('#teams-nav').css('height', `${teamNavHeight}px`);
}