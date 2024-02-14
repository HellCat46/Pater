function ChangePlanDetails() {
    const input = document.querySelector("#planSelector");
    
    const planInfos = document.querySelectorAll(".planInfo");
    planInfos.forEach(planInfo => planInfo.style.display = "none");
    
    // I don't want to use Static Price 
    if(input.value === "Premium"){
        
        const planInfo = document.querySelector("#premium");
        planInfo.style.display = "block";
    }else if(input.value === "Business"){
        const planInfo = document.querySelector("#business");
        planInfo.style.display = "block";
    }else if(input.value === "Custom"){
        const planInfo = document.querySelector("#custom");
        planInfo.style.display = "block";
        document.querySelector("#customLinkLimitSelector").disabled = false;
    }
}