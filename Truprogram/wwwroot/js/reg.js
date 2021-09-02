var pas1 = '0 0';
var regExPass = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{6,}/;
function acept_pasFun(password) {
    if (password.value.search(regExPass) != -1) {
        password.style = 'border-bottom: 2px solid #9ACD32;';
        document.getElementsByClassName('tooltip')[0].hidden = true;
    }
    else {
        password.style = 'border-bottom: 2px solid #B22222;';
        document.getElementsByClassName('tooltip')[0].hidden = false;
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
var regExEmail = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]+)$/;
function emailFun(email) {
    if (email.value.search(regExEmail) != -1) {
        email.style = 'border-bottom: 2px solid #9ACD32;';
    }
    else {
        email.style = 'border-bottom: 2px solid #B22222;';
    }
}
let regExName = /^[А-Я][а-я]{1,11}$/u;
function nameFun(name) {
    if (name.value.search(regExName) != -1) {
        name.style = 'border-bottom: 2px solid #9ACD32;';
    }
    else {
        name.style = 'border-bottom: 2px solid #B22222;';
    }
}
var tooltipElem;
function tooltip(elem) {
    elem.removeAttribute('readonly');
    tooltipElem = document.createElement('div');
    tooltipElem.className = 'tooltip';
    tooltipElem.innerHTML = elem.getAttribute('data-tooltip');
    let coords = elem.getBoundingClientRect();
    let top = elem.offsetHeight + coords.top + 2 + pageYOffset;
    let left = coords.left;
    tooltipElem.style.top = top + 'px';
    tooltipElem.style.left = left + 'px';
    document.body.append(tooltipElem);
}

function remove() {
    if (tooltipElem) {
        tooltipElem.remove();
        tooltipElem = null;
    }
}