var paging = {
    startIndex: 1,
    currentPage: 0,
    pageSize: 10,
    pagingWrapper: 'pagingWrapper1',
    first: 'first',
    last: 'last',
    previous: 'prev',
    next: 'next',
    numeric: 'numeric',
    pageInfo: 'pageInfo',
    PagingFunction: ''
}

function PageNumbering(TotalRecords) {
    var totalPages = 0;
    /**** Setting Total Records & Page Size *************/
    totalPages = parseInt((parseInt(TotalRecords) + parseInt(paging.pageSize) - 1) / parseInt(paging.pageSize));
    /**** Setting Total Records & Page Size *************/
    if (TotalRecords == 0 || totalPages == 1) { // in case there are no records or only one page
        $("." + paging.pagingWrapper).css("display", 'none'); // hide the paging 
    }
    else {
        $("." + paging.pagingWrapper).css("display", ''); // show the paging 
    }
    /*  Creating Pagination */
    /*  Code Commented because client don't want numbered paging */

    var LastIndex = parseInt(paging.startIndex - 1 + paging.pageSize); // this is the last displaying record
    if (LastIndex > TotalRecords) { // in case that last page includes records less than the size of the page 
        LastIndex = TotalRecords;
    }
    //$("." + paging.pageInfo).html("Showing <b>" + parseInt(paging.startIndex) + "-" + LastIndex + "</b> of <b>" + TotalRecords + "</b> Records."); // displaying current records interval  and currnet page infromation 
    if (paging.currentPage > 0) {
        $('.' + paging.pagingWrapper + ' .' + paging.first).unbind('click'); // rmove previous click events
        $('.' + paging.pagingWrapper + ' .' + paging.first).removeClass('disabled'); // remove the inactive page style

        $('.' + paging.pagingWrapper + ' .' + paging.first).click(function () { // set goto page to first page 
            GotoPage(0, this);
            return false;
        });
        $('.' + paging.pagingWrapper + ' .' + paging.previous).unbind('click');
        $('.' + paging.pagingWrapper + ' .' + paging.previous).removeClass('disabled');

        $('.' + paging.pagingWrapper + ' .' + paging.previous).click(function () {
            GotoPage(paging.currentPage - 1, this); // set the previous page next value  to current page - 1
            return false;
        });
    }
    else {

        $('.' + paging.pagingWrapper + ' .' + paging.first).addClass('disabled');
        $('.' + paging.pagingWrapper + ' .' + paging.first).unbind('click');
        $('.' + paging.pagingWrapper + ' .' + paging.previous).addClass('disabled');
        $('.' + paging.pagingWrapper + ' .' + paging.previous).unbind('click');
    }
    if (paging.currentPage < totalPages - 1) { // if you are not displaying the last index 
        $('.' + paging.pagingWrapper + ' .' + paging.next).unbind('click');
        $('.' + paging.pagingWrapper + ' .' + paging.next).removeClass('disabled');
        $('.' + paging.pagingWrapper + ' .' + paging.next).click(function () {
            GotoPage(paging.currentPage + 1, this);
            return false;
        });

        $('.' + paging.pagingWrapper + ' .' + paging.last).unbind('click');
        $('.' + paging.pagingWrapper + ' .' + paging.last).removeClass('disabled');
        $('.' + paging.pagingWrapper + ' .' + paging.last).click(function () {
            GotoPage(totalPages - 1, this);
            return false;
        });
    } else {

        $('.' + paging.pagingWrapper + ' .' + paging.next).addClass('disabled');
        $('.' + paging.pagingWrapper + ' .' + paging.next).unbind('click');
        $('.' + paging.pagingWrapper + ' .' + paging.last).addClass('disabled');
        $('.' + paging.pagingWrapper + ' .' + paging.last).unbind('click');
    }


    // displaying the numeric pages by default there are 10 numeric pages 
    var firstPage = 0;
    var lastPage = 10;
    if (paging.currentPage >= 5) {
        lastPage = paging.currentPage + 5;
        firstPage = paging.currentPage - 5
    }
    if (lastPage > totalPages) {
        lastPage = totalPages;
        firstPage = lastPage - 10;
    }
    if (firstPage < 0) {
        firstPage = 0;
    }
    var pagesString = '';
    for (var i = firstPage; i < lastPage; i++) {
        if (i == paging.currentPage)
        { pagesString += "<li class='active " + paging.numeric + "' > <a href='javascript:void(0)'>" + parseInt(i + 1) + "</a></li>" }
        else {
            pagesString += "<li class='" + paging.numeric + "'><a href='javascript:void(0)' onclick='return GotoPage(" + i + ", this)' > " + parseInt(i + 1) + "</a></li>" // add goto page event
        }
    }
    $('.' + paging.pagingWrapper + " li").each(function () {
        if ($(this).hasClass(paging.numeric)) {
            $(this).remove();
        }
    });
    $(pagesString).insertAfter($('.' + paging.pagingWrapper + " ." + paging.previous));
    /**** Loading data and binding grid *******************/
}


/*  This function will call if user click on numbered paging links. */
function GotoPage(page, sender) {
    paging.currentPage = page;
    paging.startIndex = page * paging.pageSize + 1;
    if (typeof (onGotoPage) != "undefined")
        onGotoPage(page, sender);
    if (paging.PagingFunction.trim().length > 0) {
        window[paging.PagingFunction]();
    }
    else
        Paging(paging.startIndex, paging.pageSize, sender);

    return false;
}