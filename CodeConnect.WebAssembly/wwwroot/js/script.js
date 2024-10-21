window.toggleDarkMode = function(){
    document.body.classList.toggle('dark');
}
function PreviewImg(inputId,previewId){
    var uploadImg = document.querySelector(`#${inputId}`);
    var uploadedImgPreview = document.querySelector(`#${previewId}`);
    uploadImg.addEventListener("change", (event) => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.addEventListener("load", (event) => {
                uploadedImgPreview.src = event.target.result;
            });
            reader.readAsDataURL(file);
        }
    });
}
let lastHeight = 0;
let first = true;
function autoResizeTextAreaAndContainer(textarea) {
    //set to auto so it shrinks straight away when clearing
    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight === 0 ? lastHeight + 'px' : textarea.scrollHeight + 'px';
    lastHeight = textarea.scrollHeight;
}
function postSizeOnBlur(elementId){
    let post = document.getElementById(elementId);
    post.style.minHeight = lastHeight + 'px'; // Maintain last height if empty
}
