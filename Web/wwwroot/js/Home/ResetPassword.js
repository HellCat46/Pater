const alert = document.querySelector("#alert");
const alertMessage = document.querySelector("#alertMessage");

const resetPass = document.querySelector("#resetPassword");
resetPass.addEventListener("submit", async (e) => {
    e.preventDefault();

    const newPassword = document.querySelector("#newPass");
    const newPasswordConfirm = document.querySelector("#newPassConfirm");

    if(newPassword.value !== newPasswordConfirm.value){
        alert.className = "alert alert-error";
        alertMessage.innerText = "Passwords do not match.";
        setTimeout(()=> {
            alert.className = "hidden";
            alertMessage.innerText = "";
        },2000)
        return;
    }
    try {
        const res = await fetch(window.location.href, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                password : newPassword.value
            }),
            credentials: "include"
        });
        
        if(res.status === 200){
            alert.className = "alert alert-success"
            alertMessage.innerText = "Successfully Changed the password."
        }
    }catch (e) {
        console.log(e);
        alert.className = "alert alert-error";
        alertMessage.innerText = "Unable to communicate with the server.";
        setTimeout(()=> {
            alert.className = "hidden"
            alertMessage.innerText = "";
        },2000)
    }
});