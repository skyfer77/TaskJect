function formatString(template, ...args) {
    return template.replace(/{(\d+)}/g, (match, index) =>
        typeof args[index] !== 'undefined' ? args[index] : match
    );
}