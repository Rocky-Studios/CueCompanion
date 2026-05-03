window.cueCompanionChat = window.cueCompanionChat || {};

window.cueCompanionChat.scrollToBottom = (element) => {
    if (!element) {
        return;
    }

    element.scrollTop = element.scrollHeight;
};

window.cueCompanionShowInfo = window.cueCompanionShowInfo || {};

window.cueCompanionShowInfo.openImportPicker = (inputId) => {
    const element = document.getElementById(inputId);
    element.accept = ".json";
    if (!element) {
        return;
    }

    element.click();
};

window.cueCompanionShowInfo.downloadJsonFile = (fileName, content) => {
    const blob = new Blob([content], {type: 'application/json;charset=utf-8'});
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');

    anchor.href = url;
    anchor.download = fileName || 'show.json';
    anchor.style.display = 'none';

    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();

    URL.revokeObjectURL(url);
};

