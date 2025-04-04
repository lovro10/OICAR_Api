function Logout() {
    sessionStorage.removeItem("JWT");
    window.location.href = "loginpage.html";
}