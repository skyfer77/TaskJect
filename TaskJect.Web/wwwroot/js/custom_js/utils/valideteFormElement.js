function valideteSizeFiles(files) {
    var error = document.getElementById("error-files");
    // Підрахунок сумарного розміру файлів у байтах
    let totalSize = 0;
    for (const f of files) {
        if (f.file) {
            totalSize += f.file.size;
        }
    }

    const maxSize = 30 * 1024 * 1024; // 30 МБ в байтах
    if (totalSize > maxSize) {
        $("#error-files").css("display", "block");
        error.innerHTML = window.translations.TotalFileSizeNotExceed30MB;
        return false;
    }

    $("#error-files").css("display", "none");
    return true;
}

function valideteTitle(title) {
    var error = document.getElementById("error-title");
    if (title === '') {
        $("#error-title").css("display", "block");
        error.innerHTML = window.translations.pleaseEnterTitle;
        return false;
    }
    if (title.length > 128) {
        $("#error-title").css("display", "block");
        error.innerHTML = window.translations.maxLengthChars;
        return false;
    }
    $("#error-title").css("display", "none");
    return true;
}

function validateMinutes(input) {
    input.value = input.value.replace(/\D/g, '');

    if (input.value.length > 2) {
        input.value = input.value.slice(0, 2);
    }

    if (input.value.length > 1 && input.value[0] === '0') {
        input.value = input.value.slice(1);
    }

    if (parseInt(input.value, 10) > 60) {
        input.value = 59;
    }
}

function validateHours(input) {
    input.value = input.value.replace(/\D/g, '');
    if (input.value.length > 1 && input.value[0] === '0') {
        input.value = input.value.slice(1);
    }
}
function validateComplexity() {
    document.getElementById("input-complexity").addEventListener("input", function (event) {
        let value = event.target.value;
        value = value.replace(/\D/g, '');
        if (value.length > 1 && value.startsWith('0')) {
            value = value.slice(0, 1);
        }
        if (value.length > 9) {
            value = value.slice(0, 9);
        }
        event.target.value = value;
    });
}