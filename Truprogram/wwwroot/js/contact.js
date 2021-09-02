$('form').submit(function (event) {
    event.preventDefault();
    $.post('/OtherPages/Contact', $('form').serialize(),
        function (data) {
            $('.msg').html(data);
            $('form').css('padding-bottom', '33px');
        }
    );
});