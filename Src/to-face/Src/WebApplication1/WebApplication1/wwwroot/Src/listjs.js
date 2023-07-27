
    let pagecounter = 0;
    let pagelimit = 10;
    let searchmode = false;
    let searchon = false;
    let search = "";
    setInterval(function () {
        if (!searchon & searchmode) {
            setSearchList();
            searchon = true;
        }
    }, 2000);
    function PageBeforeBtn_Click() {
        if (pagecounter > 0) {
            pagecounter = pagecounter - 1;
            setPageNumber();
        }
    }
    function PageAfterBtn_Click() {
        pagecounter = pagecounter + 1;
        setPageNumber();
    }
    function setPageNumber() {
        var pagenumber = (pagecounter * pagelimit) + ' - ' + ((pagecounter + 1) * pagelimit);
        $('#listpagenumber').html(pagenumber);
        searchon = false;
    }
    function localDay(time) {
        var local = new Date(time);
        var date = local.toISOString().substr(0, 10);
        return date.slice(8, 10) + '/' + date.slice(5, 7) + '/' + date.slice(0, 4);
    }
    function localTime(time) {
        var local = new Date(time);
        return local.toISOString().substr(11, 5);
    }
    function SearchBtn_Click() {
        if (search != $.trim($("#search").val()))
            searchon = false;
        search = $.trim($("#search").val());
        searchmode = true;
    }
    function setSearchList() {
        let data = { pagecounter: pagecounter , search: search };
        let url = '';
        $.post(url, data).done(function (results) {
            let links = JSON.parse(results);
            let lines = '<tr><td><hr /></td></tr><br />';
            $("#links").html(lines);
            links.forEach(function (link) {
                if (link.Type == 'user') {
                    lines += '<tr><td><a href=../Channel?forUsername=' + link.User + '>' + link.Channel + '</a></td></tr><br /><tr><td><a href=../?link=' + link.Link + '>' + link.Title + '</a></td></tr><br /><tr><td>Dernier commentaire le ' + localDay(link.DateCreated) + ' à ' + localTime(link.DateCreated) + '</td></tr><br /><tr><td><hr /></td></tr><br />';
                }
                if (link.Type == 'channel') {
                    lines += '<tr><td><a href=../Channel?id=' + link.User + '>' + link.Channel + '</a></td></tr><br /><tr><td><a href=../?link=' + link.Link + '>' + link.Title + '</a></td></tr><br /><tr><td>Dernier commentaire le ' + localDay(link.DateCreated) + ' à ' + localTime(link.DateCreated) + '</td></tr><br /><tr><td><hr /></td></tr><br />';
                }
                $("#links").html(lines);
            });
        });
    }