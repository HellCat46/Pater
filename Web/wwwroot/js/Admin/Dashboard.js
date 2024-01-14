let logEndpointUrl;

const alertError = document.querySelector("#errorAlert");
const alertMessage = document.querySelector("#alertMessage");

let pageNo = 1;
const pageNoBtn = document.querySelector("#pageNo");
const logsBody = document.querySelector("#logsBody");


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
    await fetch(`${logEndpointUrl}?pageno=${pageNo}`, {
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

async function FetchFirstPage(endpointUrl){
    logEndpointUrl = endpointUrl;
    await GetLogs();
}