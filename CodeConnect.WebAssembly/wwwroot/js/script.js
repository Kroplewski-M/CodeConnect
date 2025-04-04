window.toggleDarkMode = function(){
    document.body.classList.toggle('dark');
}

let lastHeight = 0;
function autoResizeTextAreaAndContainer(textarea) {
    //set to auto so it shrinks straight away when clearing
    textarea.style.height = 'auto';
    if(textarea.scrollHeight > 50){
        textarea.style.height = textarea.scrollHeight + 'px';
    }
    lastHeight = textarea.scrollHeight;
}
function postSizeOnBlur(elementId){
    let post = document.getElementById(elementId);
    post.style.Height = lastHeight + 'px'; // Maintain last height if empty
}
