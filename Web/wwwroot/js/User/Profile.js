const alert = document.querySelector("#alert");
const alertMessage = document.querySelector("#alertMessage");


async function UpdateAvatar(endpointUrl) {
    const newAvatar = document.querySelector("#newAvatar");

    const formData = new FormData();
    formData.append("newAvatar", newAvatar.files[0]);

    await  fetch(endpointUrl, {
        method : "PATCH",
        body : formData
    }).then(async res => {
        if(res.status === 200) {
            alert.className = "alert alert-success";
            alertMessage.innerText = "Successfully Updated the Avatar!"


            // Don't Simplify!!! This forces browser to not use cached image
            const userAvatar = document.querySelector("#userAvatar");
            const newSrc = userAvatar.src.split("/UserPics/")[0] + await res.text();
            userAvatar.src = '';
            userAvatar.src = newSrc + "?time=" + new Date().getTime();
        }else {
            alert.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }).catch(err => {
        alert.className = "alert alert-error";
        alertMessage.innerText = "Request Failed. Please try again!";
        console.log(err);
    });
    setTimeout(()=> {
        alert.className= "alert hidden";
        alertMessage.innerHTML = "";
    }, 5000);
}

async function UpdateNameEmail(endpointUrl, e) {
    e.preventDefault();

    const newName = document.querySelector("#newName");
    const newEmail = document.querySelector("#newEmail");

    const formData = new FormData();
    formData.append("newName", newName.value);
    formData.append("newEmail", newEmail.value);
    await fetch(endpointUrl, {
        method : "PATCH",
        body: formData
    }).then(async (res) => {
        if (res.status === 200){
            alert.className = "alert alert-success";
            alertMessage.innerText = "Successfully Updated the Info!"

            if(newName.value !== '' && newName.value != null){
                newName.placeholder = newName.value;
                newName.value = '';
            }
            if(newEmail.value !== '' && newEmail.value != null) {
                newEmail.placeholder = newEmail.value;
                newEmail.value = '';
            }
        }else {
            alert.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }).catch(err => {
        alert.className = "alert alert-error";
        alertMessage.innerText = "Request Failed. Please try again!";
        console.log(err);
    });
    setTimeout(()=> {
        alert.className= "alert hidden";
        alertMessage.innerHTML = "";
    }, 5000);
}

async function ChangePassword(endpointUrl, e){
    document.querySelector("#change_pass").close();
    e.preventDefault();

    const oldPass = document.querySelector("#oldPass");
    const newPass = document.querySelector("#newPass");

    const formData = new FormData();
    formData.append("oldPassword", oldPass.value);
    formData.append("newPassword", newPass.value);

    await fetch(endpointUrl, {
        method : "PATCH",
        body : formData
    }).then(async res => {
        if (res.status === 200){
            alert.className = "alert alert-success";
            alertMessage.innerText = "Successfully Updated the Info!"
        }else {
            alert.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }).catch(err => {
        alert.className = "alert alert-error";
        alertMessage.innerText = "Request Failed. Please try again!";
        console.log(err);
    })

    oldPass.value = "";
    newPass.value = "";

    setTimeout(()=> {
        alert.className= "alert hidden";
        alertMessage.innerHTML = "";
    }, 5000);
}

function CheckPass(){
    const newPass = document.querySelector(`#newPass`);

    if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,50}$/.test(newPass.value)){
        document.querySelector("#passError").style.display = "block";
        document.querySelector("#submitPass").disabled = true;
    }else {
        document.querySelector("#passError").style.display = "none";
        document.querySelector("#submitPass").disabled = false;
    }
}