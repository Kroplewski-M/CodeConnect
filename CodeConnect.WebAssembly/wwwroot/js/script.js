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
