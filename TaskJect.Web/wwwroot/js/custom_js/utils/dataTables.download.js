/**
 * HTML-хелпери для експорту:
 * - data-export-include="true|false" : колонка не включається в експорт, якщо значення false
 * - data-export-value="..." : значення для експорту береться з цього атрибута; якщо відсутнє — використовується текст клітинки
 * 
 * Примітки щодо HTML:
 * 1. Використовується rowData = this.data() для отримання всіх значень рядка.
 * 2. Колонки, у яких <th> має data-export-include="false", колонка не включається в експорт.
 * 3. Для кожної клітинки:
 *    - створюється тимчасовий <div> і вставляється HTML клітинки;
 *    - якщо **перший дочірній елемент** має data-export-value, його значення використовується для експорту;
 *    - якщо data-export-value відсутній — береться весь текст клітинки (textContent.trim());
 * 4. У результат додаються лише ті рядки, які проходять фільтр пошуку (search: 'applied') і всі сторінки (page: 'all').
 * 5. getExportButtons() Отрмати налаштування кнопок для завантаження DataTable
 */
(function ($) {
    $.fn.dataTable.ext.buttons.download = {
        text: window.translations.Download,
        action: function (e, dt, button, config) {
            const exportType = config.exportType || 'csv';
            const endpoint = config.endpoint || '/api/export/csv';
            handleDownload(exportType, endpoint, dt);
        }
    };

    function handleDownload(exportType, endpoint, dt) {
        const exportPayload = {
            headers: getTableHeaders(dt),
            rows: getFilteredDataFromDataTable(dt),
            exportType,
            period: getExportPeriodText(),
            user: getUserValue('user')
        };

        fetch(endpoint, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(exportPayload)
        })
            .then(async res => {
                if (!res.ok) {
                    if(contentType && contentType.includes("application/json")) {
                        const errorData = await res.json();
                        showWarning(errorData.message);
                    } else {
                        showWarning(window.translations.ExportError);
                    }
                    throw new Error("Export failed");
                }
                return res.blob();
            })
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `export_${exportType}_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}`;
                document.body.appendChild(a);
                a.click();
                a.remove();
            })
            .catch (err => console.error(err));
    }

    function getTableHeaders(table) {
        const headers = [];

        $(table.table().header()).find('th:visible').each(function (index) {
            const $th = $(this);

            // читаємо data-export-include, дефолт — true
            const include = $th.data('export-include');
            if (include === false || include === "false") {
                return;
            }

            let headerText = $th.text().trim();
            // якщо колонка нумерації
            if (index === 0 && !headerText) { 
                headerText = "#"; 
            } 
            headers.push(headerText);
        });

        return headers;
    }

    function getFilteredDataFromDataTable(table) {
        const rows = [];

        table.rows({ search: 'applied', page: 'all' }).every(function (rowIdx, tableLoop, rowLoop) {
            const rowData = this.data();
            const row = [];

            $(table.table().header()).find('th').each(function (index) {
                const include = $(this).data('export-include');
                if (include === false || include === "false") {
                    return;
                }

                let cellContent = rowData[index];
                let value = '';

                if (index === 0) {
                    value = rowLoop + 1;
                }

                if (cellContent) {
                    const temp = document.createElement('div');
                    temp.innerHTML = cellContent;

                    const el = temp.firstElementChild;

                    // якщо в елементі є data-export-value — беремо його
                    if (el && el.dataset && el.dataset.exportValue !== undefined) {
                        value = el.dataset.exportValue;
                    } else {
                        value = temp.textContent.trim();
                    }
                }
                
                row.push(String(value));
            });

            rows.push(row);
        });

        return rows;
    }

    // Отримати період
    function getExportPeriodText() {
        const rangeInput = $('#dateRangeFilter');
        if (rangeInput.length) {
            const rangeValue = getDateRangeValue('dateRangeFilter');
            if (rangeValue) {
                return rangeValue;
            }
        }

        const quickActive = $('#dropdownCenterBtn');
        if (quickActive.length) {
            const val = quickActive.data('selected');
            if (val) {
                return val;
            }
        }

        return '';
    }

    function getDateRangeValue(inputId) {
        const el = document.getElementById(inputId);
        return el ? el.value.trim() : '';
    }

    function getUserValue(id) {
        const el = document.getElementById(id);
        return el ? el.textContent.trim() : '';
    }

    /**
     * Повертає налаштування кнопок для експорту DataTable.
     * 
     * Деталі:
     * - Кнопка верхнього рівня створює колекцію (випадаючий список) з текстом "Завантажити".
     * - Колекція містить кнопки для завантаження CSV та Excel.
     * - Під час ініціалізації:
     *    - видаляється другий span (▼), який додає DataTables для колекції;
     *    - додається класи стилів до контейнера колекції (.dt-button-collection) для відображення як меню з padding.
     * - Кнопки CSV та Excel:
     *    - мають власний текст, клас для стилю Bootstrap;
     *    - відправляють запит на відповідний endpoint для експорту;
     *    - під час init видаляють клас dt-button, щоб застосувати кастомні стилі.
     * 
     * Використовується для передачі кнопок у DataTable через опцію buttons.
     * 
     * @returns {Array} Масив конфігурацій кнопок для DataTable
     */
    function getExportButtons() {
        return [
            {
                extend: 'collection',
                text: window.translations.Download,
                className: 'dropdown-toggle',
                init: function (api, node, config) {
                    const $spans = $(node).children('span');
                    if ($spans.length > 1) {
                        $spans.eq(1).remove();
                    }
                    $(node).on('click', function () {
                        setTimeout(() => {
                            const $collection = $(node).siblings('.dt-button-collection');
                            if ($collection.length) {
                                $collection.addClass('bg-light p-1');
                            }
                        }, 0);
                    });
                },
                buttons: [
                    {
                        extend: 'download',
                        text: 'CSV',
                        exportType: 'csv',
                        className: 'btn btn-light btn-sm dropdown-item mb-1',
                        endpoint: '/api/export/csv',
                        init: function (api, node, config) {
                            $(node).removeClass('dt-button');
                        },
                    },
                    {
                        extend: 'download',
                        text: 'Excel',
                        exportType: 'xlsx',
                        className: 'btn btn-light btn-sm dropdown-item',
                        endpoint: '/api/export/excel',
                        init: function (api, node, config) {
                            $(node).removeClass('dt-button');
                        },
                    },
                ]
            }
        ];
    }

    function showWarning(message) {
        $("#responseTextWar").text(message);
        $("#warning").modal('show');
    }

    window.DataTableExport = {
        getExportButtons,
    };
})(jQuery);

