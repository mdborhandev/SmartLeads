
async function HandleLogin() {
    var model = {
        UserName: $("#UserName").val(),
        Password: $("#Password").val(),
    };
    //console.log(model);
    try {
        var response = await postData("/app/login", model);
        localStorage.setItem('access_token', response.data.access_token);
        localStorage.setItem('refresh_token', response.data.refresh_token);
        window.location.href = response.data.redirectUrl
    }
    catch (error) {
        ErrorAlert("Login Failed", error.response.data.message);
        sweetalert(error.response.data.message);
    }
}
async function HandleRegister() {
    var model = {
        FullName: $("#FullName").val(),
        UserName: $("#UserName").val(),
        Email: $("#UserName").val(),
        Password: $("#Password").val(),
        ConfirmationPassword: $("#ConfirmPassword").val(),
        Roles: ["User"]
    };
    try {
        var response = await postData("/app/register", model);
        //if (response.redirectUrl != "") {
        //    window.location.href = response.redirectUrl;
        //}
        window.location.href = "/app/registerconfirmation";
    }
    catch (error) {
        ErrorAlert("Registration Failed", error.response.data.message);
    }
}

async function HandleRegisterConfirmation() {
    var model = {
        Email: $("#UserName").val(),
        OTP: $("#OTP").val()
    };
    try {
        var response = await postData("/app/registerConfirmation", model);
        window.location.href = response.redirectUrl
    }
    catch (error) {
        console.log(error)
    }
}
async function HandleForgetPassword() {
    var model = {
        UserName: $("#UserName").val()
        
    };
    try {
        var response = await postData(`${baseUrl}Authentication/GeneratePasswordResetToken`, model);
        console.log(response);
        window.location.href = '/App/ForgetPasswordConfirmation'
    }
    catch (error) {
        console.log(error)
    }
}
async function HandleResetPassword() {
    if (!Matchwithconfirmpassword()) {
        return;
    }
    var model = {
        UserName: $("#UserName").val(),
        Token: $("#Token").val(),
        Password: $("#Password").val(),
    };
    try {
        var response = await postData(`${baseUrl}Authentication/PasswordReset`, model);
        console.log(response);
        window.location.href = '/App/ResetConfirmation'
    }
    catch (error) {
        let massage = $("#validation-msg");
        let msgarr = JSON.parse(error.response.data.detail);
        let UL = '<ul>';
        for (var i = 0; i < msgarr.length; i++) {
            let li = `<li>${msgarr[i].Description}</li>`;
            UL += li;
        }
        UL += '</ul>';
        massage.html(UL);
        console.log(massage);
    }
}
function Matchwithconfirmpassword() {
    const password = $("#Password").val();
    const Confirmpassword = $("#ConfirmPassword").val();
    if (password !== Confirmpassword) {
        $("#validation-msg").text("Password and Confirm Password do not match.");
        return false;
    }
    $("#validation-msg").text("");
    return true;
}