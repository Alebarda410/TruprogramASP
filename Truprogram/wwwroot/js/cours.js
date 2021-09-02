$('form').submit(function (event) {
    event.preventDefault();
    $.post('/Course/CoursePage', $('form').serialize(),
        function (data) {
            if (data === 'zap') {
                $('#bt').html('Отписаться');
                $('input[value="zap"]').attr('value', 'otp');
                $('.msg').html("Вы успешно записались на этот курс!");
            }
            else if (data === 'otp') {
                $('#bt').html('Записаться');
                $('input[value="otp"]').attr('value', 'zap');
                $('.msg').html("Вы успешно отписались от этого курса!");
            }
            else if (data === 'del') {
                history.back();
            } else {
                $('.msg').html(data);
            }
        }
    );
});