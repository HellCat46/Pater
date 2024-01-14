let logEndpointUrl;
let editUserEndpointUrl;
let userId;

const alertError = document.querySelector("#errorAlert");
const alertMessage = document.querySelector("#alertMessage");

let pageNo = 1;
const pageNoBtn = document.querySelector("#pageNo");
const logsBody = document.querySelector("#logsBody");


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
    await fetch(`${logEndpointUrl}?pageno=${pageNo}&userId=${userId}`, {
    }).then(async (res) => {
        if (res.status === 200){
            logsBody.innerHTML = await res.text();
            pageNoBtn.innerHTML = `Page ${pageNo}`;
        }else {
            alertError.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }).catch(err => {
        console.log(err);
        alertError.className = "alert alert-error";
        alertMessage.innerText = "Unable to fetch Logs.";
    });
    setTimeout(()=> {
        alertError.className= "alert hidden";
        alertMessage.innerText = "";
    }, 5000);
}

function setRequiredVal(userid,  logsActionUrl, editUserActionUrl){
    logEndpointUrl = logsActionUrl;
    editUserEndpointUrl = editUserActionUrl;
    userId = userid;
}