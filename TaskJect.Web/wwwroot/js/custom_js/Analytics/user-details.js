$(document).ready(function () {
    var table;
    
    $.fn.dataTable.ext.type.order['custom-datetime-pre'] = function (data) {
        if (!data) return 0;
        var parts = data.split(' ');
        var dateParts = parts[0].split('.');
        var timeParts = parts[1] ? parts[1].split(':') : ['00', '00', '00'];
        var dateTime = new Date(dateParts[2], dateParts[1] - 1, dateParts[0], timeParts[0], timeParts[1], timeParts[2]);
        return dateTime.getTime();
    };

    table = $('#datatable-user-tasks').DataTable({
        language: {
            searchPlaceholder: `${window.translations.Search}...`,
            sSearch: '',
            paginate: {
                first: window.translations.first,
                previous: window.translations.previous,
                next: window.translations.next,
                last: window.translations.last
            },
            info: formatString(window.translations.dataTableInfo, '_START_', '_END_', '_TOTAL_'),
            infoEmpty: window.translations.noEntriesToShow,
            zeroRecords: window.translations.noMatchingRecordsFound,
            lengthMenu: formatString(window.translations.lengthMenu, '_MENU_'),
            infoFiltered: formatString(window.translations.infoFiltered, '_MAX_'),
            loadingRecords: window.translations.loadingRecords,
            processing: window.translations.processing,
            emptyTable: window.translations.emptyTable,
        },
        pageLength: 20,
        "bLengthChange": false,
        pagingType: 'full_numbers',
        info: true,
        columnDefs: [
            { type: 'custom-datetime', targets: [5, 6] },
            { orderable: false, targets: [7] }
        ],
        dom: 'Bfrtip',
        buttons: DataTableExport.getExportButtons(),
    });
    table.on('draw', function () {
        const pageInfo = table.page.info();

        const formatted = formatString(
            window.translations.dataTableInfo,
            pageInfo.recordsDisplay > 0 ? pageInfo.start + 1 : pageInfo.start,
            pageInfo.end,
            pageInfo.recordsDisplay
        );

        $('div.dataTables_info').html(formatted);
    });

    var dates = dateRangeString.split(" to ");
    var startDate = dates[0];
    var endDate = dates[1];

    flatpickr("#dateRangeFilter", {
        mode: "range",
        dateFormat: "Y-m-d",
        static: true,
        defaultDate: [startDate, endDate],
        locale: window.currentCulture === "uk" ? "uk" : "en",
        onReady: function (selectedDates, dateStr, instance) {
            instance.calendarContainer.classList.add("no-tooltips");

            let saveButton = document.createElement("button");
            saveButton.textContent = window.translations.Save;
            saveButton.classList.add("btn", "btn-primary", "flatpickr-save-btn");
            saveButton.type = "button";

            saveButton.addEventListener("click", function () {
                if (instance.selectedDates.length > 0) {
                    instance.setDate(instance.selectedDates[0], true);
                    instance.close();
                }
            });

            let footer = instance.calendarContainer.querySelector(".flatpickr-footer");
            if (!footer) {
                footer = document.createElement("div");
                footer.classList.add("flatpickr-footer");
                footer.style.cssText = `
                    display: flex; 
                    justify-content: center; 
                    padding: 5px; 
                    position: relative; 
                    z-index: 10;
                    border-top: 1px solid #e6e6e6;
                `;
                instance.calendarContainer.appendChild(footer);
            }
            footer.appendChild(saveButton);
        }
    });

    const wrapperElement = document.querySelector('.flatpickr-wrapper');
    wrapperElement.classList.add('d-flex');

    $('#filterButton').click(function () {
        var dateToValue = $('#dateRangeFilter').val();

        $.ajax({
            url: '/Analytics/GetUserDetailsFiltered',
            type: 'POST',
            data: {
                userId: userId,
                dateTo: dateToValue,
            },
            success: function (partialView) {
                if ($.fn.DataTable.isDataTable('#datatable-user-tasks')) {
                    table.destroy();
                }

                $('#table-data-tasks').empty();

                $('#table-data-tasks').html(partialView);

                let totalTasks = $('#total-data').data('total-tasks');
                let totalPoints = $('#total-data').data('total-points');
                let totalTimes = $('#total-data').data('total-times');

                dateRangeString = $('#total-data').data('date-to');

                $('#total-tasks').text(totalTasks);
                $('#total-points').text(totalPoints);
                $('#total-times').text(totalTimes);

                rawTasks = JSON.parse($('#task-data').attr('data-tasks'));

                table = $('#datatable-user-tasks').DataTable({
                    language: {
                        searchPlaceholder: `${window.translations.Search}...`,
                        sSearch: '',
                        paginate: {
                            first: window.translations.first,
                            previous: window.translations.previous,
                            next: window.translations.next,
                            last: window.translations.last
                        },
                        info: formatString(window.translations.dataTableInfo, '_START_', '_END_', '_TOTAL_'),
                        infoEmpty: window.translations.noEntriesToShow,
                        zeroRecords: window.translations.noMatchingRecordsFound,
                        lengthMenu: formatString(window.translations.lengthMenu, '_MENU_'),
                        infoFiltered: formatString(window.translations.infoFiltered, '_MAX_'),
                        loadingRecords: window.translations.loadingRecords,
                        processing: window.translations.processing,
                        emptyTable: window.translations.emptyTable,
                    },
                    pageLength: 20,
                    "bLengthChange": false,
                    pagingType: 'full_numbers',
                    info: true,
                    columnDefs: [
                        { type: 'custom-datetime', targets: [5, 6] },
                        { orderable: false, targets: [7] }
                    ],
                    dom: 'Bfrtip',
                    buttons: DataTableExport.getExportButtons(),
                });

                setupDataTableLocalization(table);

                groupData(viewMode);
            },
            error: function (error) {
                console.error('Error updating data:', error);
            }
        });
    });


    function groupData(viewMode) {
        let groupedData = {};
        let sortingMap = {};

        rawTasks.forEach(task => {
            if (!task.complitedDate || isNaN(new Date(task.complitedDate))) return;
            let date = new Date(task.complitedDate);
            let key = getGroupingKey(date, viewMode);

            if (!groupedData[key]) {
                groupedData[key] = { tasks: 0, complexity: 0 };
                sortingMap[key] = date;
            }

            groupedData[key].tasks += 1;
            groupedData[key].complexity += task.complexity;
        });

        let sortedDates = Object.keys(groupedData).sort((a, b) => sortingMap[a] - sortingMap[b]);

        let tasks = sortedDates.map(date => groupedData[date].tasks);
        let points = sortedDates.map(date => groupedData[date].complexity);

        updateChart(sortedDates, tasks, points);
    }

    function getGroupingKey(date, viewMode) {
        let d = new Date(date);
        const locale = window.currentCulture || 'en';

        switch (viewMode) {
            case 1:
                return d.toLocaleDateString(locale); // Локалізована дата (день)
            case 0:
                let weekNumber = getWeekNumber(d);
                return `${window.translations.Week} ${weekNumber}, ${d.getFullYear()}`;
            case 2:
                return d.toLocaleDateString(locale, { month: 'long', year: 'numeric' }); // Місяць
            case 3:
                const quarter = Math.floor(d.getMonth() / 3) + 1;
                return `${window.translations.Quarter} ${quarter} ${d.getFullYear()}`;
            case 4:
                return d.getFullYear().toString(); // Рік
            default:
                return `${window.translations.Week} ${getWeekNumber(d)} ${d.getFullYear()}`;
        }
    }

    function getWeekNumber(d) {
        let date = new Date(d.getTime());
        date.setUTCHours(0, 0, 0, 0);
        date.setDate(date.getDate() + 3 - (date.getDay() || 7));
        let firstThursday = new Date(date.getFullYear(), 0, 4);
        let weekNumber = Math.ceil((((date - firstThursday) / 86400000) + firstThursday.getDay() + 1) / 7);
        return weekNumber;
    }

    function updateChart(dates, tasks, points) {
        chart.updateOptions({
            xaxis: { categories: dates }
        });

        chart.updateSeries([
            { name: window.translations.Tasks, data: tasks },
            { name: window.translations.Points, data: points }
        ]);
    }

    document.querySelectorAll('.view-mode-task').forEach(el => {
        el.addEventListener('click', function () {
            viewMode = parseInt(this.getAttribute('onclick').match(/\d+/)[0]);
            groupData(viewMode);
        });
    });

    let chart = new ApexCharts(document.querySelector("#line-chart-datalabels"), {
        chart: {
            type: 'line',
            height: 350,
            dropShadow: {
                enabled: true,
                color: '#000',
                top: 18,
                left: 7,
                blur: 10,
                opacity: 0.2
            },
        },
        series: [],
        xaxis: { categories: [] },
        colors: ["#ffc107", "#0d6efd"],
        dataLabels: { enabled: true },
        stroke: { curve: 'smooth' },
        toolbar: {
            show: true,
            tools: {
                download: true 
            }
        }
    });
    chart.render();

    groupData(viewMode);

});