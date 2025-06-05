window.toggleDarkMode = function(){
    document.body.classList.toggle('dark');
}
//#region create post textbox
let lastHeight = 0;
const minHeight = 50;
/**
 * Dynamically adjusts the height of a textarea element to fit its content.
 *
 * Sets the textarea's height to 'auto' to allow shrinking, then expands it if the content exceeds the minimum height. Updates the global `lastHeight` variable with the current scroll height.
 *
 * @param {string} textareaId - The ID of the textarea element to resize.
 */
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

window.highlightCodeBlocks = () => {
    document.querySelectorAll('pre code').forEach((block) => {
        block.removeAttribute('data-highlighted');
        hljs.highlightElement(block);
    });
};

window.getValueById = function (id) {
    return document.getElementById(id).value;
};