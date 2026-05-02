///* options line with data labels */
//function createChart(selector, title, color) {
//    return new ApexCharts(document.querySelector(selector), {
//        series: [],
//        chart: {
//            height: 320,
//            type: 'line',
//            dropShadow: {
//                enabled: true,
//                color: '#000',
//                top: 18,
//                left: 7,
//                blur: 10,
//                opacity: 0.2
//            },
//            toolbar: { show: false },
//            zoom: { enabled: false }
//        },
//        colors: [color],
//        dataLabels: { enabled: true },
//        stroke: { curve: 'smooth' },
//        title: {
//            text: title,
//            align: 'left',
//            style: { fontSize: '13px', fontWeight: 'bold', color: '#8c9097' }
//        },
//        grid: { borderColor: '#f2f5f7' },
//        markers: { size: 1 },
//        xaxis: {
//            title: { fontSize: '13px', fontWeight: 'bold', style: { color: "#8c9097" } },
//            labels: { show: true, style: { colors: "#8c9097", fontSize: '11px', fontWeight: 600 } }
//        },
//        yaxis: {
//            labels: { show: true, style: { colors: "#8c9097", fontSize: '11px', fontWeight: 600 } },
//            min: 0,
//            max: 10,
//            tickAmount: 10,
//            forceNiceScale: true
//        },
//        legend: { position: 'top', horizontalAlign: 'right', offsetX: -10 }
//    });
//}

//var chart1 = createChart("#line-chart-datalabels", 'Count Task', '#845adf');
//var chart2 = createChart("#line-chart-datalabels1", 'Points', '#23b7e5');
//chart1.render();
//chart2.render();

//const month = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
//let colorsss = ["#12c2c2", "#4d5ddb", "#ffc102", "#8920ad", "#1dd871", "#d03d46", "#e791bc", "#ffa505"];
//let yaxisTitle = 'Count Task';
//let yaxisTitlePoint = 'Points';
//let taskData = [];
//let pointData = [];
//let userIds = [];
//let activeFilter = 'quickFilter';

//$(function (e) {
//    var DT1 = $('#datatable-basic-check1').DataTable({
//        columnDefs: [
//            {
//                orderable: false,
//                className: 'select-checkbox form-check-input',
//                targets: 0
//            },
//            {
//                bSortable: false,
//                aTargets: [-1]
//            },
//            {
//                targets: [2, 3, 4, 5],
//                className: 'dt-body-right'
//            },
//            {
//                targets: 5,
//                orderData: [5],
//                render: function (data, type, row) {
//                    return type === 'sort' ? $(data).data('order') : data;
//                }
//            }
//        ],
//        fixedColumns: {
//            left: 1,
//            right: 1
//        },
//        select: {
//            style: 'multi',
//            selector: 'td:first-child'
//        },
//        order: [[1, 'asc']],
//        language: {
//            searchPlaceholder: 'Search...',
//            sSearch: '',
//        },
//        paging: false,
//    });

//    DT1.rows().select();

//    function updateCharts() {
//        var arr = [];
//        DT1.rows({ selected: true }).every(function (rowIdx) {
//            arr.push(this.data());
//        });
//        var users = [];
        
//        for (var i = 0; i < arr.length; i++) {
//            arr[i][0] = arr[i][0].replace('<p style="display: none;">', '').replace('</p>', '');
//            var index = userIds.indexOf(arr[i][0]);
//            if (index !== -1) {
//                users.push({
//                    name: arr[i][1],
//                    data: taskData[index] || [],
//                    dataPoint: pointData[index] || [],
//                    color: colorsss[i]
//                });
//            }
//        }
//        updateChartsWithUserData(users);
//    }

//    function updateChartsWithUserData(user) {
//        var maxRow = taskData.length > 0 ? Math.max(...taskData.map(row => Math.max(...row))) : 10;
//        var minRow = taskData.length > 0 ? Math.min(...taskData.map(row => Math.min(...row))) : 0;
//        var maxRowPoint = pointData.length > 0 ? Math.max(...pointData.map(row => Math.max(...row))) : 10;
//        var minRowPoint = pointData.length > 0 ? Math.min(...pointData.map(row => Math.min(...row))) : 0;
//        if (user.length > 0) {
//            // Îíîâëþºìî ñåð³¿ äëÿ ïåðøîãî ãðàô³êà
//            var taskSeries = user.map(function (u) {
//                return {
//                    name: u.name,
//                    data: u.data,
//                    color: u.color
//                };
//            });
//            chart1.updateOptions({
//                title: { text: titleText },
//                xaxis: { categories: categoriesTime, title: { text: xaxisTitle } },
//                yaxis: {
//                    title: { text: yaxisTitle }, min: Math.floor(minRow / 2), max: maxRow, tickAmount: maxRow < 10 ? maxRow : 10,
//                    labels: {
//                        formatter: function (val) {
//                            return Math.round(val);
//                        }
//                    }
//                }
//            });
//            chart1.updateSeries(taskSeries);

//            // Îíîâëþºìî ñåð³¿ äëÿ äðóãîãî ãðàô³êà
//            var pointSeries = user.map(function (u) {
//                return {
//                    name: u.name,
//                    data: u.dataPoint,
//                    color: u.color
//                };
//            });
//            chart2.updateOptions({
//                title: { text: titleTextPoints },
//                xaxis: { categories: categoriesTime, title: { text: xaxisTitle } },
//                yaxis: {
//                    title: { text: yaxisTitlePoint }, min: Math.floor(minRowPoint / 2), max: maxRowPoint, tickAmount: maxRowPoint < 10 ? maxRowPoint : 10, labels: {
//                        formatter: function (val) {
//                            return Math.round(val);
//                        }
//                    }
//                }
//            });
//            chart2.updateSeries(pointSeries);
//        } else {
//            chart1.updateSeries([{ name: "", data: [] }]);
//            chart2.updateSeries([{ name: "", data: [] }]);
//        }
//    }

//    $("#selectAll").on("click", function (e) {
//        if ($(this).is(":checked")) {
//            DT1.rows().select();
//        } else {
//            DT1.rows().deselect();
//        }
//    });

//    DT1.on('select.dt deselect.dt', function () {
//        updateCharts();
//        updateSelectAllCheckbox();
//    });

//    function updateSelectAllCheckbox() {
//        var totalRows = DT1.rows().count();
//        var selectedRows = DT1.rows({ selected: true }).count();
//        $("#selectAll").prop("checked", totalRows === selectedRows);
//    }

//    let quickFilterValue = 0;
//    let selectedMode = 0;

//    $(document).ready(function () {
//        $(window).on("load", function () {
//            $("#selectAll").prop("checked", true);
//            getGraphTaskData(selectedMode);
//            getGraphPointData(selectedMode);
//        });

//        flatpickr("#dateRangePicker", {
//            mode: "range",
//            dateFormat: "Y-m-d",
//        });

//        $('#dateRangeSubmit').on('click', function (e) {
//            e.preventDefault();
//            activeFilter = 'dateRange';
//            $.ajax({
//                url: '/Analytics/GetTableData',
//                type: 'POST',
//                data: $('#dateRangeForm').serialize(),
//                success: function (response) {
//                    var dateTo = document.getElementById('dateRangePicker').value;
//                    updateTable(response, dateTo);
//                },
//                error: function (error) {
//                    console.log('Error sending date');
//                }
//            });

//            getGraphTaskData(selectedMode);
//            getGraphPointData(selectedMode)
//        });

//        $('.quick-filter').on('click', function (e) {
//            e.preventDefault();
//            quickFilterValue = $(this).data('value');
//            activeFilter = 'quickFilter';
//            $.ajax({
//                url: '/Analytics/GetTableData',
//                type: 'POST',
//                data: { quickFilter: quickFilterValue },
//                success: function (response) {
//                    updateTable(response);
//                },
//                error: function (error) {
//                    console.log('An error occurred when applying a quick filter');
//                }
//            });

//            getGraphTaskData(selectedMode);
//            getGraphPointData(selectedMode);
//        });
//    });

//    window.updateTable = function (data,dateTo) {
//        let selectedRows = DT1.rows({ selected: true }).indexes().toArray();
//        DT1.clear();
        
//        let rows = data.map(user => {
//            let totalMinutes = (user.actualHours || 0) * 60 + (user.actualMinutes || 0);
//            let hours = Math.floor(totalMinutes / 60);
//            let minutes = totalMinutes % 60;
//            let userDetailsLink = `/Analytics/UserDetails/${user.userId}`;
//            if (dateTo) {
//                userDetailsLink += `?dateTo=${dateTo}`;
//            }
//            return [
//                `<p style="display: none;">${user.userId}</p>`,
//                `${user.firstName} ${user.surname}`,
//                user.points || 0,
//                user.countTask || 0,
//                user.taskOverdue || 0,
//                `<span data-order="${totalMinutes}">${hours}h ${minutes}m</span>`,
//                `<div class="btn-list">
//                   <a href="javascript:void(0)" onclick="MoreDetails('${user.userId}')"
//                      <i class="ri-eye-line"></i></a> 
//                    <a href="${userDetailsLink}" role="button" class="btn btn-sm btn-info-light btn-icon">
//                       <i class="ri-user-3-line"></i></a>
//                 </div>`
//            ];
//        });

//        DT1.rows.add(rows).draw();
//        DT1.rows(selectedRows).select();

//        DT1.order([[1, 'asc']]).draw();
//        updateSelectAllCheckbox();
//    };

//    function updateChartTitlesAndXAxis(period, quickFilter, dateToRan) {
//        const thisMonth = month[new Date().getMonth()];
//        const quarter = Math.floor((new Date().getMonth() + 3) / 3);
//        const year = new Date().getFullYear();
//        let periodName = ["Week","Day", "Month", "Quarter", "Year"];

//        if (activeFilter === 'dateRange') {
//            titleText = 'Count Task ' + dateToRan + ' View mode ' + periodName[period];
//            titleTextPoints = 'Points ' + dateToRan + ' View mode ' + periodName[period];
//            xaxisTitle = periodName[period];
//        } else {
//            if (quickFilter === 1) {
//                titleText = 'Count Task This Week';
//                titleTextPoints = 'Points This Week';
//                xaxisTitle = 'Week';
//            } else if (quickFilter === 0) {
//                titleText = 'Count Task Month ' + thisMonth;
//                titleTextPoints = 'Points Month ' + thisMonth;
//                xaxisTitle = 'Sprint';
//            } else if (quickFilter === 2) {
//                titleText = 'Count Task ' + quarter + ' Quarter ' + year;
//                titleTextPoints = 'Points ' + quarter + ' Quarter ' + year;
//                xaxisTitle = 'Sprint';
//            } else if (quickFilter === 3) {
//                titleText = 'Count Task ' + year + ' Year';
//                titleTextPoints = 'Points ' + year + ' Year';
//                xaxisTitle = 'Month';
//            }
//        }
//    }

//    window.getGraphTaskData = function (period) {
//        let dateToValue = activeFilter === 'dateRange' ? $('#dateRangePicker').val() : null;
//        selectedMode = period || 0;
//        $.ajax({
//            type: "GET",
//            url: "/Analytics/GetTaskGraphData",
//            data: { quickFilter: quickFilterValue, dateTo: dateToValue, period: selectedMode },
//            success: function (response) {
//                categoriesTime = response.categories;
//                taskData = [];
//                userIds = [];

//                for (const userKey in response.stats) {
//                    if (response.stats.hasOwnProperty(userKey)) {
//                        userIds.push(userKey);
//                        taskData.push(response.stats[userKey]);
//                    }
//                }
                    
//                updateChartTitlesAndXAxis(selectedMode, quickFilterValue, dateToValue);
//                updateCharts();
//            }
//        });
//    };

//    window.getGraphPointData = function (period) {
//        let dateToValue = activeFilter === 'dateRange' ? $('#dateRangePicker').val() : null;
//        selectedMode = period || 0;
//        $.ajax({
//            type: "GET",
//            url: "/Analytics/GetPointGraphData",
//            data: { quickFilter: quickFilterValue, dateTo: dateToValue, period: selectedMode },
//            success: function (response) {
//                categoriesTime = response.categories;
//                pointData = [];

//                for (const userKey in response.stats) {
//                    if (response.stats.hasOwnProperty(userKey)) {
//                        let index = userIds.indexOf(userKey);
//                        if (index !== -1) {
//                            pointData[index] = response.stats[userKey];
//                        } else {
//                            userIds.push(userKey);
//                            pointData.push(response.stats[userKey]);
//                        }
//                    }
//                }

//                updateChartTitlesAndXAxis(selectedMode, quickFilterValue, dateToValue);
//                updateCharts();
//            }
//        });
//    };
//});

///* Task Summary chart */
//var options1 = {
//    series: [],
//    labels: [],
//    chart: {
//        height: 320,
//        type: 'donut',
//    },
//    dataLabels: {
//        enabled: false,
//    },

//    legend: {
//        position: "bottom",
//    },
//    stroke: {
//        show: true,
//        curve: 'smooth',
//        lineCap: 'round',
//        width: 0,
//    },
//    plotOptions: {

//        pie: {
//            expandOnClick: false,
//            donut: {
//                size: '70%',
//                background: 'transparent',
//                labels: {
//                    show: true,
//                    name: {
//                        show: true,
//                        fontSize: '20px',
//                        color: '#495057',
//                        offsetY: -4
//                    },
//                    value: {
//                        show: true,
//                        fontSize: '18px',
//                        color: undefined,
//                        offsetY: 8,
//                        formatter: function (val) {
//                            return val + "%"
//                        }
//                    },
//                    total: {
//                        show: true,
//                        showAlways: true,
//                        label: 'Week',
//                        fontSize: '22px',
//                        fontWeight: 600,
//                        color: '#495057',
//                    }

//                }
//            }
//        }
//    },
//    colors: ["#845adf", "#23b7e5", "#e6533c", "#26bf94", "#86858f", "#f5b849"],
//};
//document.querySelector("#task-summary").innerHTML = " ";
//var chart = new ApexCharts(document.querySelector("#task-summary"), options1);
//chart.render();

//var tasksPie = [];
//var labels = [];
////Set new value and labels point in pie apexchart 
//function getValuePieChart(tasks, points, period, quickFilter, dateToRang) {
//    tasksPie = JSON.parse(tasks);
//    var pointsPie = JSON.parse(points)
//    labels = ['Not Started (Points ' + pointsPie[0] + ')', 'In Progress (Points ' + pointsPie[1] + ')', 'On Review (Points ' + pointsPie[2] + ')', "Done (Points " + pointsPie[3] + ")", "Archived (Points " + pointsPie[4] + ")", "OnHold (Points " + pointsPie[5] + ")"];
//    var titlePeriod = 'Total task';
//    chart.updateOptions({
//        series: tasksPie,
//        labels: labels,
//        plotOptions: {
//            pie: {
//                donut: {
//                    labels: {
//                        total: {
//                            label: titlePeriod,
//                        }

//                    }
//                }
//            }
//        }
//    })
//}
////Overview analytics user task by id user 
//function MoreDetails(id, dateTo, period) {
//    chart.destroy()
//    $.ajax({
//        type: "POST",
//        url: "/Analytics/MoreDetails/",
//        data: { id: id, dateTo: dateTo, period: period, quickFilter: quickFilter },
//        success: function (response) {
//            $("#modal-window").html(response);
//            $('#offcanvasExample').offcanvas('show');
//            document.querySelector("#task-summary-" + id).innerHTML = " ";
//            chart = new ApexCharts(document.querySelector("#task-summary-" + id), options1);
//            chart.render();
//            chart.updateOptions({
//                series: tasksPie,
//                labels: labels,
//                plotOptions: {
//                    pie: {
//                        donut: {
//                            labels: {
//                                total: {
//                                    label: titlePeriod,
//                                }

//                            }
//                        }
//                    }
//                }
//            })
//        }
//    });
//}
///* Jobs Summary chart */
//$(function () {
//    flatpickr("#dateRangePicker", {
//        mode: "range",
//        //allowInput: true,
//        //static: true,
//        dateFormat: "Y-m-d",
//    });
//});
$(function () {
    flatpickr("#dateRangeFilter", {
        mode: "range",
        dateFormat: "Y-m-d",
        static: true,
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

});


$(function (e) {
    var DT1 = $('#datatable-basic-check1').DataTable({
        columnDefs: [
            {
                "targets": 0,
                "render": function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                bSortable: false,
                aTargets: [-1]
            },
        ],
        order: [[0, 'asc']],
        paging: false,
        language: {
            searchPlaceholder: `${window.translations.Search}...`,
            sSearch: '',
            info: formatString(window.translations.dataTableInfo, '_START_', '_END_', '_TOTAL_'),
            infoEmpty: window.translations.noEntriesToShow,
            zeroRecords: window.translations.noMatchingRecordsFound,
            lengthMenu: formatString(window.translations.lengthMenu, '_MENU_'),
            infoFiltered: formatString(window.translations.infoFiltered, '_MAX_'),
            loadingRecords: window.translations.loadingRecords,
            processing: window.translations.processing,
            emptyTable: window.translations.emptyTable,
        },
        dom: 'Bfrtip',
        buttons: DataTableExport.getExportButtons(),
    });
    setupDataTableLocalization(DT1);
    $(document).ready(function () {
        $('#datatable-basic-check1 thead th').each(function () {
            this.style.setProperty('text-align', 'center', 'important');
        });
    });
    let updateTable = function (data, dateTo = '') {
        DT1.clear();
        let rows = data.map(user => {
            let totalMinutes = (user.actualHours || 0) * 60 + (user.actualMinutes || 0);
            let hours = Math.floor(totalMinutes / 60);
            let minutes = totalMinutes % 60;
            let userDetailsLink = `/Analytics/UserDetails/${user.userId}`;
            if (dateTo) {
                userDetailsLink += `?dateTo=${dateTo}`;
            }
            return [
                '', 
                `${user.firstName} ${user.surname}`,
                user.points || 0,
                user.countTask || 0,
                user.taskOverdue || 0,
                `<span data-order="${totalMinutes}">${hours}${window.translations.h} ${minutes}${window.translations.m}</span>`,
                `<div class="btn-list">
                    <a href="${userDetailsLink}" role="button" class="btn btn-sm btn-info-light btn-icon">
                       <i class="ri-user-3-line"></i></a>
                </div>`
            ];
        });

        DT1.rows.add(rows).draw(); 

        setupDataTableLocalization(DT1);
    };

    $('#dateRangeSubmit').on('click', function (e) {
        e.preventDefault();
        activeFilter = 'dateRange';
        $.ajax({
            url: '/Analytics/GetTableData',
            type: 'POST',
            data: $('#dateRangeForm').serialize(),
            success: function (response) {
                var dateTo = document.getElementById('dateRangeFilter').value;
                updateTable(response, dateTo);
            },
            error: function (error) {
                console.error('Error sending date');
            }
        });
    });

    $('.quick-filter').on('click', function (e) {
        e.preventDefault();
        quickFilterValue = $(this).data('value');
        activeFilter = 'quickFilter';

        let text = $(this).text().trim()
        $('#dropdownCenterBtn')
            .data('selected', text)
            .attr('data-selected', text); 

        $.ajax({
            url: '/Analytics/GetTableData',
            type: 'POST',
            data: { quickFilter: quickFilterValue },
            success: function (response) {
                updateTable(response);
            },
            error: function (error) {
                console.error('An error occurred when applying a quick filter');
            }
        });
    });
});

