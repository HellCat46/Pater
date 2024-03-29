setTimeout(() => {
    document.querySelector("#errorAlert").remove();
}, 10000)

function OpenResetDialog(){
    document.querySelector("#resetPassEmail").value = document.querySelector("#userEmail").value;
    document.querySelector("#resetPassDialog").showModal();
}

async function resetPassword(EndpointUrl, element) {
    element.preventDefault();

    const submitButton = document.querySelector("#resetPassSubmit");
    const loading = document.querySelector("#resetPassLoading");
    
    submitButton.style.display = "none";
    loading.style.display = "flex";
    
    const email = document.querySelector("#resetPassEmail");
    
    const alert = document.querySelector("#resetPasswordAlert");
    const alertMessage = document.querySelector("#resetPasswordAlertMessage");
    
    const formData = new FormData();
    formData.append("email", email.value);
    
    try {
        const res = await fetch(EndpointUrl, {
            method : "POST",
            body : formData,
            credentials : "include"
        })
        
        if(res.status === 200){
            alert.className = "alert alert-success";
            alertMessage.innerText = "Check your inbox for the mail."
        }else {
            const json = await res.json();
            alert.className = "alert alert-error";
            alertMessage.innerText = json.error;
        }
    }catch (e){
        console.log(e);
        alert.className = "alert alert-error";
        alertMessage.innerText = "Unable to communicate with server."
    }
    submitButton.style.display = "inline-flex";
    loading.style.display = "none";
}