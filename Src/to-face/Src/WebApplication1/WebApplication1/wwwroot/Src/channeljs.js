
    let yourapikey = "MY_API_KEY";
    let loadon = false;
    let param = '';
    setInterval(function () {
        if (!loadon) {
            try {
                setIdURL();
            }
            catch{
                setForUsernameURL();
            }
            finally {
                loadPage();
                loadon = true;
            }
        }
    }, 2000);
    function loadPage() {
        $.get("https://www.googleapis.com/youtube/v3/channels?part=contentDetails&" + param + "&key=" + yourapikey,
            function (data) {
                $.each(data.items, function (i, item) {
                    pid = item.contentDetails.relatedPlaylists.uploads;
                    getVids(pid);
                });
            }
        );
    }
    function getVids(pid) {
        $.get(
            "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=20&playlistId=" + pid + "&key=" + yourapikey,
            function (data) {
                var results;
                $.each(data.items, function (i, item) {
                    if (i == 0)
                        results = '<tr><td>' + item.snippet.channelTitle + '</tr></td>';
                    results += '<tr><td><a href=../?link=https://www.youtube.com/watch?v=' + item.snippet.resourceId.videoId + '>' + item.snippet.title + '</a></tr></td>';
                });
                $('#channelcontent').append(results);
            }
        );
    }
    function setIdURL() {
        var url = window.location.href;
        var captured = /id=([^&]+)/.exec(url)[1];
        if (captured) {
            param = 'id=' + (captured ? captured : '');
        }
    }
    function setForUsernameURL() {
        var url = window.location.href;
        var captured = /forUsername=([^&]+)/.exec(url)[1];
        if (captured) {
            param = 'forUsername=' + (captured ? captured : '');
        }
    }