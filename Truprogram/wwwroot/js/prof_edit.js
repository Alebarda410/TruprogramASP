function del() {
    if (confirm('Подтвердите удаление')) {
        window.location.href = '/User/DelProfile';
    }
}

var pas1 = '0 0';
var regExPass = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{6,}/;
function acept_pasFun(password) {
    if (password.value.search(regExPass) != -1) {
        password.style = 'border-bottom: 2px solid #9ACD32;';
    }
    else {
        password.style = 'border-bottom: 2px solid #B22222;';
    }
    pas1 = password.value;
}

function acept_pas2Fun(password2) {
    if (password2.value === pas1) {
        password2.style = 'border-bottom: 2px solid #9ACD32;';
    }
    else {
        password2.style = 'border-bottom: 2px solid #B22222;';
    }
}

var regExName = /^[А-Я][а-я]{1,11}$/u;
function nameFun(name) {
    if (name.value.search(regExName) != -1) {
        name.style = 'border-bottom: 2px solid #9ACD32;';
    }
    else {
        name.style = 'border-bottom: 2px solid #B22222;';
    }
}

var regExEmail = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]+)$/;
function emailFun(email) {
    if (email.value.search(regExEmail) != -1) {
        email.style = 'border-bottom: 2px solid #9ACD32;';
    }
    else {
        email.style = 'border-bottom: 2px solid #B22222;';
    }
}

$('form').submit(function (event) {
    event.preventDefault();
    var formNm = $('form')[0];
    var formData = new FormData(formNm);
    $.ajax({
        type: 'POST',
        url: '/User/Profile',
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

var ff = -1;
$('.ch_ps').click(function () {
    event.preventDefault();
    ff *= -1;
    $('#pas1').val('');
    $('#pas2').val('');
    $('.wrap_pas_change').toggle(200);
    if (ff == 1) {
        $('#up_down').attr('src', '/img/up.svg');
    } else {
        $('#up_down').attr('src', '/img/down.svg');
    }
});