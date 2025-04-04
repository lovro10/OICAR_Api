function Login() {
    let loginUrl = "http://localhost:5194/api/Korisnik/login"
    let loginData = {
        "username": $("#username").val(),
        "password": $("#password").val()
    }
    $.ajax({
        method: "POST",
        url: loginUrl,
        data: JSON.stringify(loginData),
        contentType: 'application/json'
    }).done(function (tokenData) {

        sessionStorage.setItem("JWT", tokenData.token);
        window.location.href = "logspage.html";
    }).fail(function (err) {
        alert(err.responseText);
        sessionStorage.removeItem("JWT");
    });
}