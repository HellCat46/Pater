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

const selectAllLinks = document.querySelector("#selectAllLinks");


selectAllLinks.addEventListener("change", () => {

    const checkboxes = document.querySelectorAll(".links");
    for(let idx =0; idx< checkboxes.length; idx++){
        checkboxes[idx].checked = selectAllLinks.checked;
    }
    if(selectAllLinks.checked) showLinkSelToast(true);
    else showLinkSelToast(false)
});

function clickCheckbox(){
    let anyChecked = false;


    const checkboxes = document.querySelectorAll(".links");
    for(let idx = 0 ; idx < checkboxes.length; idx++){
        if(checkboxes[idx].checked) anyChecked = true;
    }

    if(anyChecked) showLinkSelToast(true);
    else {
        showLinkSelToast(false);
        selectAllLinks.checked = false;
    }
}

function showLinkSelToast(state){
    const toast = document.querySelector("#linkSelToast");
    if(state === true) toast.className = "toast toast-center toast-bottom";
    else toast.className = "hidden";
}

async function deleteSelectedLinks(endpointUrl){
    showLinkSelToast(false);
    const checkboxes = document.querySelectorAll(".links");
    
    let anyChecked = false;
    for(let idx = 0 ; idx < checkboxes.length; idx++){
        if(checkboxes[idx].checked) anyChecked = true;
    }
    
    if(!anyChecked) { // For Some Edge Case I can't find
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error"
        toastAlertType.innerText = "No Selected Links";
        setTimeout(()=> {
            toastAlert.className = "hidden"
            toastAlertType.className = "hidden";
            toastAlertText.innerHTML = "";
        }, 5000);
        return;
    }
    
    
    if (checkboxes[0].checked)
        endpointUrl += "?codes="+checkboxes[0].value+"&";
    else 
        endpointUrl += "?"
    
    for(let idx = 1; idx < checkboxes.length; idx++){
        if(checkboxes[idx].checked)
            endpointUrl += "codes="+checkboxes[idx].value+"&";
    }
    endpointUrl = endpointUrl.slice(0, endpointUrl.length-1);
    
    try {
        const res = await fetch(endpointUrl, {
            method : "DELETE",
            credentials : "include"
        });
        if(res.status === 200){
            toastAlert.className = "toast toast-center";
            toastAlertType.className = "alert alert-success"
            toastAlertType.innerText = "Successfully deleted all the selected link";
            await GetLinks(getLinksURL);
        }else {
            const json = await res.json();
            toastAlert.className = "toast toast-center";
            toastAlertType.className = "alert alert-error"
            toastAlertType.innerText = json.error;
        }
    }catch (ex){
        console.error(ex);
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error"
        toastAlertType.innerText = "Unable to Delete Links. Please try again.";
    }
    setTimeout(()=> {
        toastAlert.className = "hidden"
        toastAlertType.className = "hidden";
        toastAlertText.innerHTML = "";
    }, 5000);
}

async function GetLinks(endpointUrl){
    getLinksURL = endpointUrl;
    if (linkLists == null) return;
        
    await fetch(`${endpointUrl}?pageNo=${pageNo}`).then(async (res) => {
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