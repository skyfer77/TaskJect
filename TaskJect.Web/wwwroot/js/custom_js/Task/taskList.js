$(function () {
    "use strict"

    let statusChoices;

    window.getLast6Months = () => {
        const months = [];
        const now = new Date();
        for (let i = 5; i >= 0; i--) {
            const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
            months.push(date.toLocaleString(window.currentCulture, { month: 'short' }));
        }
        return months;
    }

    window.countTasksByMonth = (tasks, type) => {
        const result = new Array(6).fill(0);
        const now = new Date();

        tasks.forEach(task => {
            const dateStr = type === 'new' ? task.StartDate : task.CompletedDate;
            if (!dateStr) return;
            const taskDate = new Date(dateStr);
            const monthDiff = (now.getFullYear() - taskDate.getFullYear()) * 12 + (now.getMonth() - taskDate.getMonth());
            if (monthDiff >= 0 && monthDiff < 6) {
                result[5 - monthDiff]++;
            }
        });

        return result;
    }

    window.last6Months = getLast6Months();
    window.newTasksCount = countTasksByMonth(tasks, 'new');
    window.completedTasksCount = countTasksByMonth(tasks, 'completed');

    let chart;
    let table;

    window.initializeCharts = () => {
        if (chart) {
            chart.destroy();
        }

        var options = {
            series: [
                { name: window.translations.New, data: newTasksCount },
                { name: window.translations.Completed, data: completedTasksCount }
            ],
            chart: {
                type: 'bar',
                height: 210,
                stacked: true
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    columnWidth: '25%',
                    endingShape: 'rounded',
                },
            },
            grid: {
                borderColor: '#f2f5f7',
            },
            dataLabels: {
                enabled: false
            },
            colors: ["#845adf", "#28d193", "#ffbe14", "#23b7e5"],
            stroke: {
                show: true,
                colors: ['transparent']
            },
            xaxis: {
                categories: last6Months,
                labels: {
                    show: true,
                    style: {
                        colors: "#8c9097",
                        fontSize: '11px',
                        fontWeight: 600,
                        cssClass: 'apexcharts-xaxis-label',
                    },
                }
            },
            yaxis: {
                title: {
                    style: {
                        color: "#8c9097",
                    }
                },
                labels: {
                    show: true,
                    style: {
                        colors: "#8c9097",
                        fontSize: '11px',
                        fontWeight: 600,
                        cssClass: 'apexcharts-xaxis-label',
                    },
                }
            },
            fill: {
                opacity: 1
            },
        };

        chart = new ApexCharts(document.querySelector("#task-list-stats"), options);
        chart.render();
    }

    initializeCharts()

    /* AssignedDate Picker */
    flatpickr("#assignedDate", {
        enableTime: false,
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });

    /* DueDate Picker */
    flatpickr("#dueDate", {
        enableTime: false,
        dateFormat: "Y-m-d",
        locale: window.currentCulture === "uk" ? "uk" : "en",
    });

    window.setBadges = () => {
        $('span[name="Low"]').addClass('badge bg-info-transparent');
        $('span[name="Medium"]').addClass('badge bg-success-transparent');
        $('span[name="High"]').addClass('badge bg-danger-transparent');
        $('span[name="OnHold"]').addClass('bg-warning-transparent');
        $('span[name="NotStarted"]').addClass('bg-primary-transparent');
        $('span[name="InProgress"]').addClass('bg-info-transparent');
        $('span[name="OnReview"]').addClass('bg-danger-transparent');
        $('span[name="Done"]').addClass('bg-success-transparent');
        $('span[name="Archived"]').addClass('bg-warning-transparent');
    }

    setBadges();

    window.initializeDataTable = () => {
        if ($.fn.DataTable.isDataTable('#datatable-task-list')) {
            table.destroy();
        }
        //DataTable '_START_', '_END_', '_TOTAL_', '_MENU_', '_MAX_' standart placeholders
        table = $('#datatable-task-list').DataTable({
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
            bLengthChange: false,
            pagingType: 'full_numbers',
            info: true,
            columnDefs: [
                {
                    targets: 0,
                    render: function (data, type, row, meta) {
                        return meta.row + meta.settings._iDisplayStart + 1;
                    }
                },
                {
                    targets: 3,
                    type: 'date'
                },
                { orderable: false, targets: [6] }
            ],
        });

        filterByStatus();
    }
    function filterByStatus() {
        const selectedStatuses = statusChoices.getValue(true);

        $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn => fn.name !== "statusFilter");

        $.fn.dataTable.ext.search.push(function statusFilter(settings, data, dataIndex) {
            const statusText = $(table.row(dataIndex).node()).find('td:eq(2) span').attr('name');
            return selectedStatuses.includes(statusText);
        });

        table.draw();
    }

    window.initializeStatusChoices = () => {
        if (statusChoices) {
            statusChoices.destroy();
        }

        statusChoices = new Choices('#statusFilter', {
            removeItemButton: true,
            placeholder: true,
            placeholderValue: `${window.translations.Statuses}...`,
            position: 'bottom'
        });

        document.getElementById('statusFilter').addEventListener('change', function () {
            filterByStatus();
        });
    }

    initializeStatusChoices()
    initializeDataTable()
    setupDataTableLocalization(table);
    if (statusChoices.getValue(true).length > 0) {
        filterByStatus();
    }
});
