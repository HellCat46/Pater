let visitorChart;
let browserChart;
let osChart;
let countryChart;
let cityChart;
let deviceChart;

async function VisitsChart(actionUrl, code, timeFrame){
    const data= await GetVisitsData(actionUrl, code,timeFrame);
    
    if(visitorChart !== undefined){
        visitorChart.data.labels = data.map(d => d.label)
        visitorChart.data.datasets[0].data = data.map(d => d.data)
        visitorChart.update();
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
            tbl.innerHTML += `<tr><th>${i}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
        }
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
        tbl.innerHTML += `<tr><th>${i}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
    }
}
async function CityChart(actionUrl, code, timeFrame){
    const data= await GetData(actionUrl,"city", code, timeFrame);
    //const tbl = document.querySelector("#CityTable")

    if(cityChart !== undefined){
        cityChart.data.labels = data.map(d => d.label)
        cityChart.data.datasets[0].data = data.map(d => d.data)
        cityChart.update();

        //tbl.innerHTML = "";
        //for(let i =0; i< data.length; i++){
        //    tbl.innerHTML += `<tr><th>${i}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
        //}
        return;
    }
    
    cityChart = new Chart(document.querySelector('#CityChart'), {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: 'Visitor\'s Country',
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
    
    //tbl.innerHTML = "";
    //for(let i =0; i< data.length; i++){
    //    tbl.innerHTML += `<tr><th>${i}</th><th>${data[i].label}</th><th>${data[i].data}</th></tr>`;
    //}
}


// async function UpdateOtherChart(, actionUrl, type, code, timeFrame){
//     const data = await  GetData(actionUrl, type, code, timeFrame)
// }


async function GetData(actionUrl, detailType, code, timeFrame) {
    const res = await fetch(`${actionUrl}?detailType=${detailType}&code=${code}&timeFrame=${timeFrame}`, {
        credentials : "include"
    })

    return (await res.json());
}
async function GetVisitsData(actionUrl, code, timeFrame) {
    const res = await fetch(`${actionUrl}?code=${code}&timeFrame=${timeFrame}`, {
        credentials : "include"
    })

    return (await res.json());
}