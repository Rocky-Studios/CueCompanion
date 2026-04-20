window.cueCompanionChat = window.cueCompanionChat || {};

window.cueCompanionChat.scrollToBottom = (element) => {
    if (!element) {
        return;
    }

    element.scrollTop = element.scrollHeight;
};

