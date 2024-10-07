window.toggleDarkMode = function(){
    document.body.classList.toggle('dark');
}
function PreviewImg(){
    var uploadImg = document.querySelector("#uploadImg");
    var uploadedImgPreview = document.querySelector("#uploadedImgPreview");
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

function autoResizeTextAreaAndContainer(textarea,defaultHeight) {
    //set to auto so it shrinks straight away when clearing
    textarea.style.height = 'auto';
    const textareaHeight = parseInt(textarea.scrollHeight);

    textarea.style.height = textarea.scrollHeight + 'px';
    
    const containerId = textarea.getAttribute('data-resizeContainer');
    if(containerId != null){
        const container = document.getElementById(containerId);
        //not changing height if textbox hasn't gained height
        if(defaultHeight < (textareaHeight)){
            container.style.height = textarea.scrollHeight + 50 + 'px';
        }else{
            let containerDefault = container.getAttribute('data-defaultHeight');
            container.style.height = containerDefault + 'px';
        }
    }
}