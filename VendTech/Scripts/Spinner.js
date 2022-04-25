

var spinner = {
    
    show: function () { 
        $("div.spanner").addClass("show");
        $("div.overlay").addClass("show");
    },
    hide: function () {
        $("div.spanner").addClass("hide");
        $("div.overlay").addClass("hide");
    }
}