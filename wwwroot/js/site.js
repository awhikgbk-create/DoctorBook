document.addEventListener("DOMContentLoaded",()=>{document.querySelectorAll('input[type="date"]').forEach(x=>{x.min=new Date().toISOString().slice(0,10);});});
