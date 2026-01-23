window.selectImportJsonFile = async () => {
    return new Promise((resolve, reject) => {

        const inputElement = document.createElement('input');
        inputElement.type = 'file';
        inputElement.accept = '.json';

        if (!inputElement) {
            reject("Input element not found");
            return;
        }

        inputElement.addEventListener('change', (event) => {
            const file = event.target.files[0];
            if (!file) {
                reject("No file selected");
                return;
            }

            const reader = new FileReader();
            reader.onload = (e) => {
                resolve(e.target.result);
            };
            reader.onerror = () => {
                reject("Error reading file");
            };
            reader.readAsText(file);
        });

        inputElement.click();
    });
}