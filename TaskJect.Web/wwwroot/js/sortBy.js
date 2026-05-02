
function sortProjects() {
    var projectContainers = $('[data-project-container]');
    var sortType = document.getElementById('choices-single-default').value;

    var sortedProjects = projectContainers.toArray();
    sortedProjects.sort(function (firstProject, secondProject) {
        var firstProjectValue, secondProjectValue;

        switch (sortType) {
            case 'Newest':
                firstProjectValue = new Date($(firstProject).find('input[type="hidden"][start-date]').val());
                secondProjectValue = new Date($(secondProject).find('input[type="hidden"][start-date]').val());
                return secondProjectValue - firstProjectValue;
            case 'Date Added':
                firstProjectValue = new Date($(firstProject).find('input[type="hidden"][start-date]').val());
                secondProjectValue = new Date($(secondProject).find('input[type="hidden"][start-date]').val());
                return firstProjectValue - secondProjectValue;
            case 'A - Z':
                firstProjectValue = $(firstProject).find("[data-title]").text().trim().toLowerCase();
                secondProjectValue = $(secondProject).find("[data-title]").text().trim().toLowerCase();
                return firstProjectValue.localeCompare(secondProjectValue);
            default:
                return 0;
        }
    });

    $('#projectList').empty();
    sortedProjects.forEach(function (project) {
        var targetContainer = $('#projectList');
        targetContainer.append(project);
    });
}

window.sortProjects = sortProjects;

