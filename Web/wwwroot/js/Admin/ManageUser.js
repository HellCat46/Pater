let logEndpointUrl;
let editUserEndpointUrl;
let userId;

const Alert = document.querySelector("#Alert");
const alertMessage = document.querySelector("#alertMessage");

let pageNo = 1;
const pageNoBtn = document.querySelector("#pageNo");
const logsBody = document.querySelector("#logsBody");

async function UpdateUser(endpointUrl){
    const UserId = document.querySelector("#userId");
    
    const userNewName = document.querySelector("#userNewName");
    const userNewEmail = document.querySelector("#userNewEmail");
    const userNewPlan = document.querySelector("#userNewPlan");
    const userNewPassword = document.querySelector("#userNewPassword");
    const userNewLinkLimit = document.querySelector("#userNewLinkLimit");
    const userIsVerified = document.querySelector("#userIsVerified");
    
    
    try {
        const formData = new FormData();
        formData.append("UserId", UserId.value);
        if(userNewName.value !== "") formData.append("Name", userNewName.value);
        if(userNewEmail.value !== "") formData.append("Email", userNewEmail.value);
        if(userNewPlan.value !== "") formData.append("Plan", userNewPlan.value);
        if(userNewPassword.value !== "") formData.append("Password", userNewPassword.value);
        if(userNewLinkLimit.value !== "") formData.append("LinkLimit", userNewLinkLimit.value);
        if(userIsVerified.value !== "") formData.append("Verified", userIsVerified.checked);
        
        const res = await fetch(endpointUrl, {
            method : "PATCH",
            body : formData,
            credentials : "include"
        })
        if(res.status === 200){
            Alert.className = "alert alert-success";
            alertMessage.innerText = "Successfully Updated User Info";
        }else {
            Alert.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }catch (ex) {
        console.error(ex);
        Alert.className = "alert alert-error";
        alertMessage.innerText = "Unable to fetch Logs.";
    }
    setTimeout(()=> {
        Alert.className= "alert hidden";
        alertMessage.innerText = "";
    }, 5000);
}
async function OpenLogs(){
    const userLogs = document.querySelector("#userLogs");
    userLogs.showModal();
    await GetLogs();
}

async function prevPage() {
    if (pageNo === 1) return;
    pageNo--;
    await GetLogs();
}

async function nextPage() {
    pageNo++;
    await GetLogs();
}

async function GetLogs() {
    await fetch(`${logEndpointUrl}?pageNo=${pageNo}&userId=${userId}`, {
    }).then(async (res) => {
        if (res.status === 200){
            logsBody.innerHTML = await res.text();
            pageNoBtn.innerHTML = `Page ${pageNo}`;
        }else {
            Alert.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }).catch(err => {
        console.log(err);
        Alert.className = "alert alert-error";
        alertMessage.innerText = "Unable to fetch Logs.";
    });
    setTimeout(()=> {
        Alert.className= "alert hidden";
        alertMessage.innerText = "";
    }, 5000);
}

function setRequiredVal(userid,  logsActionUrl, editUserActionUrl){
    logEndpointUrl = logsActionUrl;
    editUserEndpointUrl = editUserActionUrl;
    userId = userid;
}