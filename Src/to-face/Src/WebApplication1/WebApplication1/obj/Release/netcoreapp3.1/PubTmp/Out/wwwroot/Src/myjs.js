
    let str = "";
    let timecounter = 0;
    let temps = 60;
    let attendre = 0;
    let videoerror = false;
    let setScrollDown = false;
    let pagecounter = 0;
    let pagelimit = 30;
    let comsetintervaldispose = false;
    let urlon = false;
    function init() {
        str = "";
        timecounter = 0;
        temps = 60;
        attendre = 0;
        videoerror = false;
        setScrollDown = false;
        pagecounter = 0;
        pagelimit = 30;
        comsetintervaldispose = false;
        setPageNumber();
    }
    function GreetingsBtn_Click() {
        init();
        str = ($.trim($("#link").val())).replace("http://", "https://");
        if (str.indexOf("https://www.youtube.com/watch?v=") > -1) {
            videoerror = false;
            var newstr = str.replace("watch?v=", "embed/");
            $("#video").attr("src", newstr);
            videoerror = false;
        }
        else
            videoerror = true;
    }
    setInterval(function () {
        if (!videoerror & !comsetintervaldispose) {
            setComments();
        }
        timecounter += 2;
        if (timecounter <= temps) {
            attendre = temps - timecounter;
        }
        var attendresec = 'Click here for greeting... ' + attendre.toString() + ' s';
        $("#timecounter").html(attendresec);
        setURL();
    }, 2000);
    function setComments() {
        if (!videoerror) {
            let data = { str: str, strname: '', strcom: '', pagecounter: pagecounter, define: 0 };
            let url = '';
            $.post(url, data).done(function (results) {
                let comments = JSON.parse(results);
                var lines = '';
                comments.forEach(function (comment) {
                    lines += '<tr><td><b>Le ' + localDay(comment.DateCreated) + ' à ' + localTime(comment.DateCreated) + ', <u>' + comment.Name + '</u> a dit :</b><br />  - ' + comment.Comment + '</td></tr>';
                });
                $("#comments").html(lines);
                scrolldown();
            });
        }
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
    function GreetingBtn_Click() {
        comsetintervaldispose = true;
        if (!videoerror) {
            var strname = $.trim($("#name").val()).substr(0, 10);
            var strcom = $.trim($("#com").val()).substr(0, 500);
            if (timecounter >= temps & strname != "" & strcom != "") {
                let data = { str: str, strname: strname, strcom: strcom, pagecounter: pagecounter, define: 1 };
                let url = '';
                $.post(url, data).done(function () {
                    setComments();
                    timecounter = 0;
                    setScrollDown = true;
                });
            }
            comsetintervaldispose = false;
        }
    }
    function scrolldown() {
        if (setScrollDown) {
            $("#comments").scrollTop(100000);
            setScrollDown = false;
        }
    }
    function comCountChar(val) {
        var len = val.value.length;
        $('#comCharNum').html(' ' + 500 - len);
    };
    function nameCountChar(val) {
        var len = val.value.length;
        $('#nameCharNum').html(' ' + 10 - len);
    };
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
        $('#comnumber').html(pagenumber);
    }
    function VideoOnError() {
        videoerror = true;
    }
    function setURL() {
        if (!urlon) {
            var url = window.location.href;
            var captured = /link=([^&]+)/.exec(url)[1];
            var result = captured ? captured : '';
            if (result != '') {
                $('#link').val(result);
                $('#linkbutton').click();
            }
            urlon = true;
        }
    }