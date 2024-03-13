$(function () {
    //https://ezbox.idv.tw/131/back-to-top-button-without-images/
    $('#BackTop').click(function () {
        $('html,body').animate({ scrollTop: 0 }, 333);
    });
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('#BackTop').fadeIn(222);
        } else {
            $('#BackTop').stop().fadeOut(222);
        }
    }).scroll();

    $(".custom-file-input").on("change", function () {
        let fileName = $(this).val().split("\\").pop();
        $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
    });
});

/**
 * 判斷回傳值是否為錯誤碼
 * @param {Array<String>} errCodeArr 錯誤碼(字串陣列)
 * @param {String} data 資料(字串)
 */
function isError(errCodeArr, data) {
    return errCodeArr.filter(function (obj) { if (data === obj) return obj; }).length > 0;
}

/**
 * 上傳圖檔顯示預覽
 * @param {Array<File>} files input[type="file"]
 * @param {String} preview_id div的id屬性
 */
function previewImgs(files, preview_id) {
    let preview = document.getElementById(preview_id);
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        if (!file.type.startsWith('image/')) { continue }

        const img = document.createElement("img");
        img.classList.add("obj");
        img.file = file;
        preview.appendChild(img);

        const reader = new FileReader();
        reader.onload = (function (aImg) { return function (e) { aImg.src = e.target.result; }; })(img);
        reader.readAsDataURL(file);
    }
}