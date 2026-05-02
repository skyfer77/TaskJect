function setupDataTableLocalization(table) {
    table.on('draw', function () {
        const pageInfo = table.page.info();

        const formatted = formatString(
            window.translations.dataTableInfo,
            pageInfo.recordsDisplay > 0 ? pageInfo.start + 1 : pageInfo.start,
            pageInfo.end,
            pageInfo.recordsDisplay
        );

        let fullInfo = formatted;

        // Додаємо '_MAX_'
        if (pageInfo.recordsDisplay !== pageInfo.recordsTotal) {
            const filtered = formatString(window.translations.infoFiltered, pageInfo.recordsTotal);
            fullInfo += ` <span class="info-filtered">${filtered}</span>`;
        }

        $('div.dataTables_info').html(fullInfo);

        // Оновлення '_MENU_'
        $('div.dataTables_length label').each(function () {
            const $select = $(this).find('select');
            const selectHtml = $select.prop('outerHTML');
            const replaced = formatString(window.translations.lengthMenu, selectHtml);
            $(this).html(replaced);
        });
    });
}

function setupShortDataTableLocalization(table) {
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
}