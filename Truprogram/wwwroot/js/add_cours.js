$('form').submit(function (event) {
    event.preventDefault();
    var formNm = $('form')[0];
    var formData = new FormData(formNm);
    $.ajax({
        type: 'POST',
        url: '/Course/AddCourse',
        enctype: 'multipart/form-data',
        data: new FormData(this),
        processData: false,
        contentType: false,
        success: function (data) {
            $('.msg').html(data);
            $('form').css('padding-bottom', '33px');
        }
    });
});