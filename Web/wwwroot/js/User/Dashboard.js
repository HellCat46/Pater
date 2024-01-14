let pageNo = 1;
const linkLists = document.querySelector("#links");
let getLinksURL;

const alert = document.querySelector("#alert");
const alertMessage = document.querySelector("#alertMessage");

const toastAlert = document.querySelector("#toastAlert");
const toastAlertType= document.querySelector("#toastAlertType");
const toastAlertText = document.querySelector("#toastAlertText");

const createLinkModal = document.querySelector("#create");
const editLinkModal = document.querySelector("#editLink");


async function GetLinks(endpointUrl){
    getLinksURL = endpointUrl;
    await fetch(`${endpointUrl}?pageno=${pageNo}`).then(async (res) => {
        if (res.status === 200){
            linkLists.innerHTML = await res.text();
        }else {
            alert.className = "alert alert-error";
            const json = await res.json();
            alertMessage.innerText = json.error;
        }
    }).catch(err => {
        alert.className = "alert alert-error";
        alertMessage.innerText = "Unable to Fetch Links";
        console.error(err);
    });
    setTimeout(()=> {
        alert.className= "alert hidden";
        alertMessage.innerText = "";
    }, 5000);
}

async function prevPage(){
    if (pageNo === 1) return;
    pageNo--;
    await GetLinks(getLinksURL);
}
async function nextPage() {
    pageNo++;
    await GetLinks(getLinksURL);
}

async function createLinkSubmit(endpointUrl){
        createLinkModal.close();
        const link = document.querySelector("#newLink");
        const code = document.querySelector("#newLinkCode");
        const name = document.querySelector("#newLinkName");

        await fetch(endpointUrl, {
            method : "POST",
            headers : new Headers({'Content-Type': 'application/json'}),
            body : JSON.stringify({
                LinkURL : link.value,
                LinkName: name.value,
                LinkCode: code.value
            }),
            credentials : "include"
        }).then(async (res) => {
            if (res.status === 200){
                alert.className = "alert alert-success";
                alertMessage.innerText = "Successfully Shorten the Link!"

                link.value = "";
                name.value = "";
                code.value = "";

                await GetLinks(getLinksURL);
            }else {
                alert.className = "alert alert-error";
                const json = await res.json();
                alertMessage.innerText = json.error;
            }
        }).catch(err =>{
            alert.className = "alert alert-error";
            alertMessage.innerHTML = "Unable to Create Link. Please try again later";
            console.log(err)
        })
        setTimeout(()=> {
            alert.className= "hidden";
            alertMessage.innerHTML = ""
        }, 5000);
}

function copyCode(code){
    // This wouldn't work on unsecure connection
    navigator.clipboard.writeText(window.location.origin+"/"+code).then(() => {
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Successfully Copied Code to Clipboard";
    }).catch((err) => {
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error"
        toastAlertType.innerText = "Failed to Copy Code to Clipboard";
        console.log(err);
    })

    setTimeout(() => {
        toastAlert.className = "hidden"
        toastAlertType.className = "hidden";
        toastAlertText.innerHTML = "";
    }, 2000);
}

function openEditLink(code){
    const editLinkCode  = document.querySelector("#editLinkCode");
    const editLinkName = document.querySelector("#EditLinkName");
    const editLinkURL = document.querySelector("#editLinkURL");

    editLinkName.value = "";
    editLinkURL.value = "";
    editLinkCode.value = code;
    editLinkModal.showModal();
}

async function editLinkSubmit(endpointUrl){
        editLinkModal.close();

        const editLinkCode = document.querySelector("#editLinkCode");
        const editLinkURL = document.querySelector("#editLinkURL");
        const editLinkName = document.querySelector("#EditLinkName");

        await fetch(endpointUrl, {
            method : "PATCH",
            headers : new Headers({'Content-Type': 'application/json'}),
            body : JSON.stringify({
                LinkURL : editLinkURL.value,
                LinkName: editLinkName.value,
                LinkCode: editLinkCode.value
            })
        }).then(async (res) => {
            if (res.status === 200){
                toastAlert.className = "toast toast-center";
                toastAlertType.className = "alert alert-success"
                toastAlertText.innerText = "Successfully Updated the Link";
                await GetLinks(getLinksURL);
            }else {
                toastAlert.className = "toast toast-center";
                toastAlertType.className = "alert alert-error";
                const json = await res.json();
                toastAlertText.innerText = json.error;
            }
        }).catch(err => {
            toastAlert.className = "toast toast-center";
            toastAlertType.className = "alert alert-error";
            toastAlertText.innerText = "Unable to Create Link. Please try again later";
            console.log(err);
        });
        setTimeout(()=> {
            toastAlert.className = "hidden";
            toastAlertType.className = "hidden";
            toastAlertText.innerText = "";
        }, 5000);
}

function deleteLinkRequest(endpointUrl, code) {

    const formData = new FormData();
    formData.append("code", code);

    fetch(endpointUrl, {
        method : "DELETE",
        body : formData
    }).then(async (res) => {
        if (res.status === 200){
            await GetLinks(getLinksURL);
            toastAlert.className = "toast toast-center";
            toastAlertType.className = "alert alert-success"
            toastAlertText.innerText = "Successfully Deleted Link";
        }else {
            toastAlert.className = "toast toast-center";
            toastAlertType.className = "alert alert-error"
            const json = await res.json();
            toastAlertText.innerText = json.error;
        }
    }).catch(err => {
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error";
        toastAlertText.innerText = "Unable to Delete Link";
        console.error(err);
    });
    setTimeout(() => {
        toastAlert.className = "hidden"
        toastAlertType.className = "";
        toastAlertText.innerText = "";
    }, 2000);
}