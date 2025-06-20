window.toggleDarkMode = function(){
    document.body.classList.toggle('dark');
}
//#region create post textbox
let lastHeight = 0;
const minHeight = 50;
function autoResizeTextAreaAndContainer(textareaId) {
    let textarea = document.getElementById(textareaId);
    //set to auto so it shrinks straight away when clearing
    textarea.style.height = 'auto';
    if(textarea.scrollHeight > minHeight){
        textarea.style.height = textarea.scrollHeight + 'px';
    }
    lastHeight = textarea.scrollHeight;
}
function postSizeOnBlur(elementId){
    let post = document.getElementById(elementId);
    if(lastHeight > minHeight)
        post.style.height = lastHeight + 'px'; // Maintain last height 
}
//#endregion

//#region image overlay
window.showOverlayImg = (imageUrl) => {
    // Remove existing overlay if its somehow there
    const existing = document.getElementById("imageOverlay");
    if (existing) {
        existing.remove();
    }
    const overlayDiv = document.createElement("div");
    overlayDiv.id = "imageOverlay";
    overlayDiv.className = "fixed top-0 left-0 w-full h-full bg-black/70 backdrop-blur-sm flex items-center justify-center p-4";
    overlayDiv.onclick = () => {
        overlayDiv.remove();
    };
    const img = document.createElement("img");
    img.src = imageUrl;
    img.alt = "";
    img.className = "w-full h-full object-contain cursor-pointer rounded-lg";

    overlayDiv.appendChild(img);
    
    const container = document.getElementById("overlay") || document.body;
    container.appendChild(overlayDiv);
};
//#endregion

window.highlightCodeBlocks = (containerId) => {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.querySelectorAll('pre code').forEach((block) => {
        block.removeAttribute('data-highlighted');
        const code = block.textContent;
        const lang = (block.className.match(/language-(\w+)/) || [])[1] || 'plaintext';

        const result = hljs.highlight(code, { language: lang, ignoreIllegals: true });
        block.innerHTML = result.value;
        block.classList.add('hljs');
        block.dataset.highlighted = 'yes';
    });
};

window.getValueById = function (id) {
    return document.getElementById(id).value;
};