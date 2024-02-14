function ChangePlanDetails() {
    const input = document.querySelector("#planSelector");
    
    const planInfos = document.querySelectorAll(".planInfo");
    planInfos.forEach(planInfo => planInfo.style.display = "none");
    
    const BillPlan = document.querySelector("#billPlanName");
    const BillPrice = document.querySelector("#billPlanPrice");
    const BillTotal = document.querySelector("#billTotalAmt");

    const extra = document.querySelector("#billExtraDetail");
    const extraAmt = document.querySelector("#billExtraAmt");
    extra.innerText = "No Extra";
    extraAmt.innerHTML = "";
    
    // I don't want to use Static Price 
    if(input.value === "Premium"){
        const planInfo = document.querySelector("#premium");
        planInfo.style.display = "block";
        
        BillPlan.innerText = "Premium Plan";
        BillPrice.innerText = "2";
        BillTotal.innerText = "2";
        
        UpdateCheckout("Premium", "None", "2");
    }else if(input.value === "Business"){
        const planInfo = document.querySelector("#business");
        planInfo.style.display = "block";

        BillPlan.innerText = "Business Plan";
        BillPrice.innerText = "20";
        BillTotal.innerText = "20";

        UpdateCheckout("Business", "None", "20");
    }else if(input.value === "Custom"){
        const planInfo = document.querySelector("#custom");
        planInfo.style.display = "block";
        document.querySelector("#customLinkLimitSelector").disabled = false;


        BillPlan.innerText = "Custom Plan (No Custom Links Included)";
        BillPrice.innerText = "20";
        
        UpdateExtra();
    }
}

function UpdateExtra(){
    const extra = document.querySelector("#billExtraDetail");
    const extraAmt = document.querySelector("#billExtraAmt");
    const extraSelector = document.querySelector("#customLinkLimitSelector");
    const BillTotal = document.querySelector("#billTotalAmt");

    extra.innerText = extraSelector.value + " Custom Links";
    extraAmt.innerText = parseInt(extraSelector.value)/20;
    BillTotal.innerText = 20 + (parseInt(extraSelector.value)/20);


    UpdateCheckout("Custom",  extraSelector.value, BillTotal.innerText);
}

function UpdateCheckout(planName, extraLinks, total){
    const btn = document.querySelector("#checkoutbtn");
    
    btn.href = `mailto:hellcat2388+mailto@gmail.com?subject=Plan%20Purchase&body=Plan%3D${planName}%0D%0AExtra%3D${extraLinks}%0D%0ATotal%3D${total}`;
}