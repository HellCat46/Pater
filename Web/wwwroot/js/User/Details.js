let visitorChart;
let browserChart;
let osChart;
let countryChart;
let cityChart;
let deviceChart;

const toastAlert = document.querySelector("#toastAlert");
const toastAlertType= document.querySelector("#toastAlertType");
const toastAlertText = document.querySelector("#toastAlertText");


async function VisitsChart(actionUrl, code, timeFrame){
    const data= await GetVisitsData(actionUrl, code,timeFrame);
    
    if(visitorChart !== undefined){
        visitorChart.data.labels = data.map(d => d.label)
        visitorChart.data.datasets[0].data = data.map(d => d.data)
        visitorChart.update();

        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Set Time Frame for Visitor Chart to "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        return;
    }
    
    visitorChart = new Chart(document.querySelector('#VisitsChart'), {
        type: 'line',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'Visitors',
                data: data.map(d => d.data),
                borderColor: "rgb(180, 190, 254)"
            }]
        },
        options: {
            scales: {
                x :{
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                },
                y: {
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                }
            }
        }
    });
}
async function BrowserChart(actionUrl, code, timeFrame) {
    const data= await GetData(actionUrl,"browser", code, timeFrame);

    if(browserChart !== undefined){
        browserChart.data.labels = data.map(d => d.label)
        browserChart.data.datasets[0].data = data.map(d => d.data)
        browserChart.update();

        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Set Time Frame for Browser Chart to "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        return;
    }

    browserChart = new Chart(document.querySelector('#BrowserChart'), {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'Visitor\'s Browser',
                data: data.map(d => d.data),
                borderWidth: 1,
                backgroundColor : ["rgba(166, 227, 161, 0.7)", "rgba(243, 139, 168, 0.7)", "rgba(245, 194, 231, 0.7)", "rgba(116, 199, 236, 0.7)"],
                borderColor: ["rgb(166, 227, 161)", "rgb(243, 139, 168)", "rgb(245, 194, 231)", "rgb(116, 199, 236)"]
            }]
        },
        options: {
            scales: {
                x :{
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                },
                y: {
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                }
            }
        }
    });
}
async function OSChart(actionUrl, code, timeFrame){
    const data= await GetData(actionUrl,"os", code, timeFrame);

    if(osChart !== undefined){
        osChart.data.labels = data.map(d => d.label)
        osChart.data.datasets[0].data = data.map(d => d.data)
        osChart.update();

        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Set Time Frame for OS Chart to "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        return;
    }
    
    osChart = new Chart(document.querySelector('#OSChart'), {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'OS Pie Chart',
                data: data.map(d => d.data),
            }]
        },
        options: {
            scales: {
                x :{
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                },
                y: {
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                }
            }
        }
    });
}
async function DeviceChart(actionUrl, code, timeFrame){
    const data= await GetData(actionUrl,"device", code, timeFrame);

    if(deviceChart !== undefined){
        deviceChart.data.labels = data.map(d => d.label)
        deviceChart.data.datasets[0].data = data.map(d => d.data)
        deviceChart.update();

        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Set Time Frame for Device Chart to "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        return;
    }
    
    deviceChart = new Chart(document.querySelector('#DeviceChart'), {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'OS Pie Chart',
                data: data.map(d => d.data),
            }]
        },
        options: {
            scales: {
                x :{
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                },
                y: {
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                }
            }
        }
    });
}
async function CountryChart(actionUrl, code, timeFrame){
    const data= await GetData(actionUrl,"country", code, timeFrame);
    const tbl = document.querySelector("#CountryTable")

    if(countryChart !== undefined){
        countryChart.data.labels = data.map(d => d.label)
        countryChart.data.datasets[0].data = data.map(d => d.data)
        countryChart.update();
        
        tbl.innerHTML = "";
        for(let i =0; i< data.length; i++){
            tbl.innerHTML += `<tr><th>${i+1}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
        }        
        
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Set Time Frame for Country Chart to "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        return;
    }
    
    countryChart = new Chart(document.querySelector('#CountryChart'), {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'Visitor\'s Countries',
                data: data.map(d => d.data),
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                x :{
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                },
                y: {
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                }
            }
        }
    });

    tbl.innerHTML = "";
    for(let i =0; i< data.length; i++){
        tbl.innerHTML += `<tr><th>${i+1}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
    }
}
async function CityChart(actionUrl, code, timeFrame){
    const data= await GetData(actionUrl,"city", code, timeFrame);
    const tbl = document.querySelector("#CityTable")

    if(cityChart !== undefined){
        cityChart.data.labels = data.map(d => d.label)
        cityChart.data.datasets[0].data = data.map(d => d.data)
        cityChart.update();

        tbl.innerHTML = "";
        for(let i =0; i< data.length; i++){
            tbl.innerHTML += `<tr><th>${i+1}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
        }


        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-success"
        toastAlertText.innerText = "Set Time Frame for City Chart to "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        
        return;
    }
    
    cityChart = new Chart(document.querySelector('#CityChart'), {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'Visitor\'s Cities',
                data: data.map(d => d.data),
                borderWidth: 1,
                backgroundColor: "rgb(255, 99, 132)"
            }]
        },
        options: {
            scales: {
                x :{
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                },
                y: {
                    grid : {
                        color : "rgb(88, 91, 112)"
                    }
                }
            }
        }
    });
    
    tbl.innerHTML = "";
    for(let i =0; i< data.length; i++){
        tbl.innerHTML += `<tr><th>${i+1}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
    }
}

async function GetData(actionUrl, detailType, code, timeFrame) {
    try {
        const res = await fetch(`${actionUrl}?detailType=${detailType}&code=${code}&timeFrame=${timeFrame}`, {
            credentials: "include"
        })

        return (await res.json());
    }catch (e){
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error"
        toastAlertText.innerText = "Failed to Fetch Chart for "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        throw e;
    }
}
async function GetVisitsData(actionUrl, code, timeFrame) {
    try {
        const res = await fetch(`${actionUrl}?code=${code}&timeFrame=${timeFrame}`, {
            credentials: "include"
        })

        return (await res.json());
    }catch (e) {
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error"
        toastAlertText.innerText = "Failed to Fetch Chart for "+timeFrame;
        setTimeout(() => {
            toastAlert.className = "hidden"
            toastAlertType.className = "";
            toastAlertText.innerText = "";
        }, 2000);
        throw e;
    }
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



const editLinkCode  = document.querySelector("#editLinkCode");
const editLinkName = document.querySelector("#editLinkName");
const editLinkURL = document.querySelector("#editLinkURL");
const editLinkModal = document.querySelector("#editLink");
function editLink(code){
    editLinkName.value = "";
    editLinkURL.value = "";
    editLinkCode.value = code;
    editLinkModal.showModal();
}
async function editLinkSubmit(endpointUrl) {
    editLinkModal.close();

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
            if(editLinkName.value !== "")
                document.querySelector("#LinkName").innerText = editLinkName.value;
            if(editLinkURL.value !== ""){
                document.querySelector("#LinkFavicon").src = `https://t2.gstatic.com/faviconV2?client=SOCIAL&type=FAVICON&fallback_opts=TYPE,SIZE,URL&url=${editLinkURL.value}&size=48`;
                document.querySelector("#LinkURL").href = editLinkURL.value;
            }
            toastAlert.className = "toast toast-center";
            toastAlertType.className = "alert alert-success"
            toastAlertText.innerText = "Successfully Updated the Link";
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
async function deleteLink(endpointUrl, code){
    const formData = new FormData();
    formData.append("code", code);

    const res = await fetch(endpointUrl, {
        method : "DELETE",
        body : formData
    }).catch(err => {
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error";
        toastAlertText.innerText = "Unable to Delete Link";
        console.error(err);
    });
    if (res.status === 200){
        window.location.href = "https://"+window.location.host;
    }else {
        toastAlert.className = "toast toast-center";
        toastAlertType.className = "alert alert-error"
        const json = await res.json();
        toastAlertText.innerText = json.error;
    }
    setTimeout(() => {
        toastAlert.className = "hidden"
        toastAlertType.className = "";
        toastAlertText.innerText = "";
    }, 2000);
}