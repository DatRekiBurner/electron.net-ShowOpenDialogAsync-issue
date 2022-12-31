new Accordion('.accordion-container');

(function () {
    const { ipcRenderer } = require("electron");

    document.getElementById("select-directory").addEventListener("click", () => {
        ipcRenderer.send("select-directory");
    });

    ipcRenderer.on("select-directory-reply", (sender, path) => {
        document.getElementById("selected-directory-text").innerText = `${path}`;
    });
}());

function download() {
    const text = document.getElementById("selected-directory-text").innerText;
    document.getElementById("selected-directory").value = text;
    document.getElementById('download-form').submit();
}
