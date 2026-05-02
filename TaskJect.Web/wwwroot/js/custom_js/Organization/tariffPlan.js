function updateProgressBar(textId, progressBarId, currentValue, maxValue, formatText) {

    const percentage = (currentValue / maxValue) * 100;

    // Оновлюємо текст у відповідному елементі
    const textElement = document.getElementById(textId);
    if (textElement) {
        textElement.textContent = formatText
            .replace("{0}", currentValue)
            .replace("{1}", maxValue);
    }

    const progressBar = document.getElementById(progressBarId);
    if (progressBar) {
        progressBar.style.width = percentage + '%';
    }
}
