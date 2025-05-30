window.toggleDarkMode = function(){
    document.body.classList.toggle('dark');
}
//#region create post textbox
let lastHeight = 0;
const minHeight = 50;
function autoResizeTextAreaAndContainer(textarea) {
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
        post.style.height = lastHeight + 'px'; // Maintain last height if empty
}
//#endregion