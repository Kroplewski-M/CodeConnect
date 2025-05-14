window.onload = function(){
    console.log("Infinite Scroll");
    window.observers = {};

    window.observeSentinel = function(sentinelId, dotnetHelper){
        const sentinel = document.getElementById(sentinelId);
        if(!sentinel) return;


        const observer = new IntersectionObserver((entries) =>{
            entries.forEach(entry =>{
                if(entry.isIntersecting){
                    dotnetHelper.invokeMethodAsync("OnSentinelVisible");
                }
            });
        });
        observer.observe(sentinel);
        window.observers[sentinelId] = observer;
    }
    window.unobserveSentinel = function(sentinelId){
        const observer = window.observers[sentinelId];
        if(observer){
            observer.unobserve(document.getElementById(sentinelId));
            observer.disconnect();
            delete window.observers[sentinelId];
        }
    };
}

