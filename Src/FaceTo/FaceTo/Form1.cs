using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using NetFwTypeLib;
using EO.WebBrowser;
namespace FaceTo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static string username;
        private static bool isadmin = false;
        private static FirestoreDb db;
        public static EO.WebBrowser.DOM.Document document;
        public static string path, readText;
        private async void Form1_Shown(object sender, EventArgs e)
        {
            username = Program.username;
            await adminConnection();
            if (isadmin) 
                EO.WebBrowser.WebView.ShowDebugUI();
            this.pictureBox1.Dock = DockStyle.Fill;
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            EO.WebBrowser.Runtime.DefaultEngineOptions.SetDefaultBrowserOptions(options);
            EO.WebEngine.Engine.Default.Options.AllowProprietaryMediaFormats();
            EO.WebEngine.Engine.Default.Options.SetDefaultBrowserOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Create(pictureBox1.Handle);
            this.webView1.Engine.Options.AllowProprietaryMediaFormats();
            this.webView1.SetOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Engine.Options.DisableGPU = false;
            this.webView1.Engine.Options.DisableSpellChecker = true;
            this.webView1.Engine.Options.CustomUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            this.webView1.JSInitCode = @"setInterval(function(){ 
                try {
                    if (window.location.href.indexOf('youtube') > -1) {
                        document.cookie='VISITOR_INFO1_LIVE = oKckVSqvaGw; path =/; domain =.youtube.com';
                        var cookies = document.cookie.split('; ');
                        for (var i = 0; i < cookies.length; i++)
                        {
                            var cookie = cookies[i];
                            var eqPos = cookie.indexOf('=');
                            var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
                            document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT';
                        }
                        var el = document.getElementsByClassName('ytp-ad-skip-button');
                        for (var i=0;i<el.length; i++) {
                            el[i].click();
                        }
                        var element = document.getElementsByClassName('ytp-ad-overlay-close-button');
                        for (var i=0;i<element.length; i++) {
                            element[i].click();
                        }
                    }
                }
                catch {}
            }, 5000);";
            path = @"ppia.html";
            readText = DecryptFiles(path + ".encrypted", "tybtrybrtyertu50727885");
            webView1.LoadHtml(readText);
            webView1.RegisterJSExtensionFunction("PostAMessage", new JSExtInvokeHandler(WebView_JSPostAMessage));
            webView1.RegisterJSExtensionFunction("UpdateMessages", new JSExtInvokeHandler(WebView_JSUpdateMessages));
            webView1.RegisterJSExtensionFunction("RetrieveList", new JSExtInvokeHandler(WebView_JSRetrieveList));
            webView1.RegisterJSExtensionFunction("RetrieveListSearch", new JSExtInvokeHandler(WebView_JSRetrieveListSearch));
            webView1.RegisterJSExtensionFunction("UpdateModalMessages", new JSExtInvokeHandler(WebView_JSUpdateModalMessages));
            webView1.RegisterJSExtensionFunction("DeleteMessage", new JSExtInvokeHandler(WebView_JSDeleteMessage));
        }
        public static string DecryptFiles(string inputFile, string password)
        {
            using (var input = File.OpenRead(inputFile))
            {
                byte[] salt = new byte[8];
                input.Read(salt, 0, salt.Length);
                using (var decryptedStream = new MemoryStream())
                using (var pbkdf = new Rfc2898DeriveBytes(password, salt))
                using (var aes = new RijndaelManaged())
                using (var decryptor = aes.CreateDecryptor(pbkdf.GetBytes(aes.KeySize / 8), pbkdf.GetBytes(aes.BlockSize / 8)))
                using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    string contents;
                    int data;
                    while ((data = cs.ReadByte()) != -1)
                        decryptedStream.WriteByte((byte)data);
                    decryptedStream.Position = 0;
                    using (StreamReader sr = new StreamReader(decryptedStream))
                        contents = sr.ReadToEnd();
                    decryptedStream.Flush();
                    return contents;
                }
            }
        }
        private async Task adminConnection()
        {
            try
            {
                var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
                var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
                db = FirestoreDb.Create("faceto", builder.Build());
                DocumentReference docRef = db.Collection("administration").Document(username);
                DocumentSnapshot document = await docRef.GetSnapshotAsync();
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                isadmin = Convert.ToBoolean(documentDictionary["isadmin"].ToString());
            }
            catch
            {
                isadmin = false;
            }
        }
        private void LoadPage()
        {
            string imgyoutube = "file:///" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"file:\", "").Replace(Process.GetCurrentProcess().ProcessName + ".exe", "").Replace(@"\", "/").Replace(@"//", "") + "youtube.jpg";
            string stringinject;
            stringinject = @"

    <link type='text/css' rel='stylesheet' href='https://www.gstatic.com/firebasejs/ui/4.8.0/firebase-ui-auth.css' />
    <link type='text/css' rel='stylesheet' href='https://www.w3schools.com/w3css/4/w3.css' />
    <style>

body {
    font-family: sans-serif;
    background-color: #141e30;
    font-size: calc(0.74vw + 0.74vh + 0.37vmin);
    text-align: center;
    align-content: center;
    width: 100%;
    overflow-x: hidden;
}

.link, .msg, .author, .search {
    color: #03e9f4;
    font-size: calc(0.74vw + 0.74vh + 0.37vmin);
    height: 40px;
    width: 900px;
    background: rgba(0,0,0,.5);
    box-sizing: border-box;
    box-shadow: 0 15px 25px rgba(0,0,0,.6);
    border-radius: 10px;
    display: inline-block;
    box-shadow: 0 2px 2px 0 rgb(0, 0, 0), 0 3px 1px -2px rgb(0, 0, 0) 0 1px 5px 0 rgb(0, 0, 0);
    text-align: center;
    vertical-align: middle;
    line-height: 1%;
    padding-left: 1%;
}

.link, .search {
    width: 600px;
}

.author {
    width: 200px;
}

.link-container, .msg-container, .search-container {
    margin-top: 20px;
    vertical-align: middle;
    width: 100%;
    text-align: center;
}

.slideshow-container {
    justify-content: center;
    display: flex;
    margin-top: 20px;
    border: none;
    text-align: center;
    width: 100%;
}

    .slideshow-container img {
        width: 50%;
        height: 40%;
    }

.comment-container {
    display: inline-block;
    justify-content: center;
    margin-top: 20px;
    width: 80%;
    height: 500px;
    background: rgba(0,0,0,.5);
    color: white;
    text-align: center;
    border-radius: 10px;
    box-shadow: 0 15px 25px rgba(0,0,0,.6);
    overflow-y: auto;
    overflow-x: hidden;
    word-wrap: break-word;
}

.comments {
    margin: 10px;
}

.username {
    cursor: pointer;
    display: inline;
}

.deletion {
    cursor: pointer;
    display: inline;
}

.w3-modal-content {
    height: 90%;
    overflow-y: auto;
    overflow-x: hidden;
    color: white;
    background: rgba(0,0,0,.5);
    box-sizing: border-box;
    box-shadow: 0 15px 25px rgba(0,0,0,.6);
    background-color: #141e30;
    border-radius: 10px;
}

#modal {
    margin: 50px;
    display: inline-block;
}

.list-container {
    display: inline-grid;
    grid-template-rows: auto;
    grid-template-columns: 33% 33% 33%;
    column-gap: 10px;
    row-gap: 1em;
    justify-content: center;
    margin-top: 20px;
    margin-bottom: 20px;
    width: 80%;
    height: 800px;
    background: rgba(0,0,0,.5);
    color: white;
    text-align: center;
    border-radius: 10px;
    box-shadow: 0 15px 25px rgba(0,0,0,.6);
    overflow-y: auto;
    overflow-x: hidden;
    word-wrap: break-word;
}

.griditems {
    margin: 10px;
}

#list img {
    width: 80%;
    height: auto;
    cursor: pointer;
}

.title {
    margin-top: 10px;
    color: #FFFFFF;
    font-size: calc(0.64vw + 0.64vh + 0.30vmin);
}

.showlist, .showmessages {
    text-align: center;
    width: 100%;
    padding-top: 5%;
    padding-bottom: 5%;
}

.minus, .plus {
    padding: 10px;
    cursor: pointer;
}

::-webkit-scrollbar {
    width: 10px;
}

::-webkit-scrollbar-track {
    background: #444;
}

::-webkit-scrollbar-thumb {
    background: #888;
}

    ::-webkit-scrollbar-thumb:hover {
        background: #eee;
    }

    </style>
".Replace("\r\n", " ");
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(" + stringinject + @" ).appendTo('head');";
            this.webView1.EvalScript(stringinject);
            stringinject = @"

    <div class='link-container'>
        <input class='mdl-textfield__input firebaseui-input firebaseui-id-email link' value='Youtube video link'>
        <button class='linking firebaseui-id-submit firebaseui-button mdl-button mdl-js-button mdl-button--raised mdl-button--colored' data-upgraded=',MaterialButton'>next</button>
    </div>
    <div class='slideshow-container'>
        <img src='img/youtube.jpg' />
    </div>
    <div class='msg-container'>
        <input class='mdl-textfield__input firebaseui-input firebaseui-id-email author' value='Author'>
        <input class='mdl-textfield__input firebaseui-input firebaseui-id-email msg' value='Message !'>
        <button class='messaging firebaseui-id-submit firebaseui-button mdl-button mdl-js-button mdl-button--raised mdl-button--colored' data-upgraded=',MaterialButton'>next</button>
    </div>
    <div class='comment-container' id='messages'></div>
    <div class='search-container'>
        <input class='mdl-textfield__input firebaseui-input firebaseui-id-email search' value='Youtube video search'>
        <button class='searching firebaseui-id-submit firebaseui-button mdl-button mdl-js-button mdl-button--raised mdl-button--colored' data-upgraded=',MaterialButton'>next</button>
    </div>
    <div class='list-container' id='list'></div>

    <div id='id01' class='w3-modal'>
        <div class='w3-modal-content'>
            <span onclick='hideModal()' class='w3-button w3-display-topright'>&times;</span>
            <div id='modal'></div>
        </div>
    </div>

<script>

var username = '';

function showModal(data) {
    document.getElementById('id01').style.display = 'block';
    username = data.dataset.username;
    UpdateModalMessages(videoid, username);
}

function hideModal() {
    document.getElementById('id01').style.display = 'none';
}

function deleteMessage(data) {
    var id = '';
    id = data.dataset.id;
    DeleteMessage(videoid, username, id);
}

var videoid = '';
var videotitle = '';
var author = '';
var msg = '';
var resindicex = window.screen.availWidth;
var resindicey = window.screen.availHeight;
var polling;
var index = 0;
var search = '';
var indexsearch = 0;

var startindexmessages = 0;
var endindexmessages = 15;

$('.linking').click(function() {
	index = 0;
	var link = $('.link').val();
	videoid = link.replace('https://www.youtube.com/watch?v=', '');
	videoid = videoid.replace('http://www.youtube.com/watch?v=', '');
	videoid = videoid.replace('https://www.youtu.be/watch?v=', '');
	videoid = videoid.replace('http://www.youtu.be/watch?v=', '');
    $.ajax({
        type: 'GET',
        async: false,
        cache: false,
        url: 'https://noembed.com/embed?url=https://www.youtube.com/watch?v=' + videoid,
        dataType: 'json',
        success: function(data) {
            if (data.error != '404 Not Found') {
				videotitle = data.author_name + ' : ' + data.title;
				var file = 'https://www.youtube.com/embed/' + videoid;
				var htmlString = '';
				htmlString = `<div class=\'mySlides\' data-link=\'` + file + `\'>
                                <iframe src=\'` + file + `\' frameborder=\'0\' allowfullscreen class=\'content\' style=\'width:` + resindicex * 50 / 100 + `px;height:` + 9 / 16 * resindicex * 50 / 100 + `px;\'></iframe>
                            </div>`;
				$('.slideshow-container').empty();
				$('.slideshow-container').append(htmlString);
                startindexmessages = 0;
                endindexmessages = 15;
				UpdateMessages(videoid, videotitle, startindexmessages.toString(), endindexmessages.toString());
            }
        }
    });
});

function minusIndexMessages() {
	startindexmessages = startindexmessages - 15;
	endindexmessages = endindexmessages - 15;
	if (startindexmessages < 0) {
		startindexmessages = 0;
		endindexmessages = 15;
	}
	UpdateMessages(videoid, videotitle, startindexmessages.toString(), endindexmessages.toString());
}

function plusIndexMessages() {
	startindexmessages = startindexmessages + 15;
	endindexmessages = endindexmessages + 15;
	UpdateMessages(videoid, videotitle, startindexmessages.toString(), endindexmessages.toString());
}

$('.messaging').click(function() {
	msg = $('.msg').val();
	author = $('.author').val();
	if (videoid != '' & videotitle != '' & msg != '' & author != '' & msg != 'Message !' & author != 'Author') {
        startindexmessages = 0;
        endindexmessages = 15;
        PostAMessage(videoid, videotitle, author, msg);
	    $('.msg').val('Message !');
	    $('.author').val('Author');
	}
});

var startindex = 0;
var endindex = 15;

RetrieveList(startindex.toString(), endindex.toString());

function minusIndex() {
	startindex = startindex - 15;
	endindex = endindex - 15;
	if (startindex < 0) {
		startindex = 0;
		endindex = 15;
	}
	RetrieveList(startindex.toString(), endindex.toString());
}

function plusIndex() {
	startindex = startindex + 15;
	endindex = endindex + 15;
	RetrieveList(startindex.toString(), endindex.toString());
}

function openLink(img) {
	var src = img.src;
	var id = src.replace('https://img.youtube.com/vi/', 'https://www.youtube.com/watch?v=');
	id = id.replace('/mqdefault.jpg', '');
	$('.link:text').val(id);
	$( '.linking' ).click();
}

var startindexsearch = 0;
var endindexsearch = 15;
var searchwords = '';

$('.searching').click(function() {
    searchwords = $('.search').val();
	if (searchwords != 'Youtube video search' & searchwords != '') {
        startindexsearch = 0;
        endindexsearch = 15;
		RetrieveListSearch(startindexsearch.toString(), endindexsearch.toString(), searchwords);
	}
	else {   
        startindex = 0;
        endindex = 15;
		RetrieveList(startindex.toString(), endindex.toString());
	}
});

function minusIndexSearch() {
	startindexsearch = startindexsearch - 15;
	endindexsearch = endindexsearch - 15;
	if (startindexsearch < 0) {
		startindexsearch = 0;
		endindexsearch = 15;
	}
	RetrieveListSearch(startindexsearch.toString(), endindexsearch.toString(), searchwords);
}

function plusIndexSearch() {
	startindexsearch = startindexsearch + 15;
	endindexsearch = endindexsearch + 15;
	RetrieveListSearch(startindexsearch.toString(), endindexsearch.toString(), searchwords);
}

</script>
".Replace("\r\n", " ").Replace("img/youtube.jpg", imgyoutube);
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('body').append(" + stringinject + @");});";
            this.webView1.EvalScript(stringinject);
        }
        async void WebView_JSPostAMessage(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string videotitle = e.Arguments[1] as string;
            string author = e.Arguments[2] as string;
            string msg = e.Arguments[3] as string;
            string t = Convert.ToString(DateTime.Now);
            await PostAMessage(videoid, videotitle, author, msg, t);
        }
        private async Task PostAMessage(string videoid, string videotitle, string author, string msg, string t)
        {
            var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
            db = FirestoreDb.Create("faceto", builder.Build());
            Dictionary<string, object> message = new Dictionary<string, object>
            {
                { "username", username },
                { "author", author },
                { "msg", msg },
                { "t", t }
            };
            await db.Collection(videoid).Document().SetAsync(message, SetOptions.MergeAll);
            DocumentReference docRef = db.Collection("list").Document(videoid);
            Dictionary<string, object> update = new Dictionary<string, object>
            {
                { "videotitle", videotitle },
                { "t", t }
            };
            await docRef.SetAsync(update, SetOptions.MergeAll);
            await UpdateMessages(videoid, videotitle, 0, 15);
        }
        async void WebView_JSUpdateMessages(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string videotitle = e.Arguments[1] as string;
            double startindex = Convert.ToDouble(e.Arguments[2]);
            double endindex = Convert.ToDouble(e.Arguments[3]);
            await UpdateMessages(videoid, videotitle, startindex, endindex);
        }
        private async Task UpdateMessages(string videoid, string videotitle, double startindex, double endindex)
        {
            string messages = "";
            var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
            db = FirestoreDb.Create("faceto", builder.Build());
            Query query = db.Collection(videoid).OrderByDescending("t");
            QuerySnapshot querysnapshot = await query.GetSnapshotAsync();
            int inc = 0;
            foreach (DocumentSnapshot documentSnapshot in querysnapshot.Documents)
            {
                Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();
                string author = documentDictionary["author"].ToString();
                string msg = documentDictionary["msg"].ToString();
                string t = documentDictionary["t"].ToString();
                if (inc < 1)
                {
                    messages = @"<div class='comments'><p>" + videotitle + @"</p>";
                }
                if (inc >= startindex & inc < endindex)
                {
                    messages += @"<p><div class='username' onclick='showModal(this)' data-username='" + username + @"'>" + author + @"</div> : " + msg + @"</p>";
                }
                else
                {
                    if (inc >= endindex)
                    {
                        break;
                    }
                }
                inc++;
            }
            messages += @"<div class='showmessages'><span onclick='minusIndexMessages()' class='minus'><</span><span onclick='plusIndexMessages()' class='plus'>></span></div></div>";
            Task.Run(() => ReceiveInMessages(messages));
        }
        public void ReceiveInMessages(string messages)
        {
            try
            {
                document = webView1.GetDOMWindow().document;
                TraverseElementTree(document, (currentElement) =>
                {
                    string id = currentElement.GetID();
                    if (id.StartsWith("messages") & id.EndsWith("messages"))
                    {
                        currentElement.SetHtml(messages);
                    }
                });
            }
            catch { }
        }
        async void WebView_JSRetrieveList(object sender, JSExtInvokeArgs e)
        {
            double startindex = Convert.ToDouble(e.Arguments[0]);
            double endindex = Convert.ToDouble(e.Arguments[1]);
            string list = "";
            string videoid = "";
            string videotitle = "";
            var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
            db = FirestoreDb.Create("faceto", builder.Build());
            Query query = db.Collection("list").OrderByDescending("t");
            QuerySnapshot querysnapshot = await query.GetSnapshotAsync();
            double inc = 0;
            foreach (DocumentSnapshot documentSnapshot in querysnapshot.Documents)
            {
                if (inc >= startindex & inc < endindex)
                {
                    Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();
                    videoid = documentSnapshot.Id;
                    videotitle = documentDictionary["videotitle"].ToString();
                    list += @"<div class='griditems'><img onclick='openLink(this)' class='image' src='https://img.youtube.com/vi/" + videoid + @"/mqdefault.jpg'><div class='title'>" + videotitle + @"</div></div>";
                }
                else
                {
                    if (inc >= endindex)
                    {
                        break;
                    }
                }
                inc++;
            }
            list += @"<div class='showlist'><span onclick='minusIndex()' class='minus'><</span><span onclick='plusIndex()' class='plus'>></span></div>";
            Task.Run(() => ReceiveInList(list));
        }
        async void WebView_JSRetrieveListSearch(object sender, JSExtInvokeArgs e)
        {
            double startindex = Convert.ToDouble(e.Arguments[0]);
            double endindex = Convert.ToDouble(e.Arguments[1]);
            string searchwords = e.Arguments[2] as string;
            string list = "";
            string videoid = "";
            string videotitle = "";
            var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
            db = FirestoreDb.Create("faceto", builder.Build());
            Query query = db.Collection("list").OrderByDescending("t");
            QuerySnapshot querysnapshot = await query.GetSnapshotAsync();
            double inc = 0;
            foreach (DocumentSnapshot documentSnapshot in querysnapshot.Documents)
            {
                Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();
                videoid = documentSnapshot.Id;
                videotitle = documentDictionary["videotitle"].ToString();
                if (videotitle.ToLower().Contains(searchwords.ToLower()))
                {
                    if (inc >= startindex & inc < endindex)
                    {
                        list += @"<div class='griditems'><img onclick='openLink(this)' class='image' src='https://img.youtube.com/vi/" + videoid + @"/mqdefault.jpg'><div class='title'>" + videotitle + @"</div></div>";
                    }
                    else
                    {
                        if (inc >= endindex)
                        {
                            break;
                        }
                    }
                    inc++;
                }
            }
            list += @"<div class='showlist'><span onclick='minusIndexSearch()' class='minus'><</span><span onclick='plusIndexSearch()' class='plus'>></span></div>";
            Task.Run(() => ReceiveInList(list));
        }
        public void ReceiveInList(string list)
        {
            try
            {
                document = webView1.GetDOMWindow().document;
                TraverseElementTree(document, (currentElement) =>
                {
                    string id = currentElement.GetID();
                    if (id.StartsWith("list") & id.EndsWith("list"))
                    {
                        currentElement.SetHtml(list);
                    }
                });
            }
            catch { }
        }
        async void WebView_JSUpdateModalMessages(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string usernamemessages = e.Arguments[1] as string;
            await UpdateModalMessages(videoid, usernamemessages);
        }
        private async Task UpdateModalMessages(string videoid, string usernamemessages)
        {
            string messages = "";
            var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
            db = FirestoreDb.Create("faceto", builder.Build());
            Query query = db.Collection(videoid).OrderByDescending("t");
            QuerySnapshot querysnapshot = await query.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in querysnapshot.Documents)
            {
                Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();
                string author = documentDictionary["author"].ToString();
                string msg = documentDictionary["msg"].ToString();
                string t = documentDictionary["t"].ToString();
                string id = documentSnapshot.Id;
                if (usernamemessages == documentDictionary["username"].ToString())
                {
                    if (username != documentDictionary["username"].ToString() & !isadmin)
                    {
                        messages += @"<p>" + author + @" : " + msg + @"</p>";
                    }
                    else
                    {
                        messages += @"<p>" + author + @" : " + msg + @"<div data-id='" + id + @"' class='deletion' onclick='deleteMessage(this)'>&times;</div></p>";
                    }
                }
            }
            Task.Run(() => ReceiveInModal(messages));
        }
        public void ReceiveInModal(string messages)
        {
            try
            {
                document = webView1.GetDOMWindow().document;
                TraverseElementTree(document, (currentElement) =>
                {
                    string id = currentElement.GetID();
                    if (id.StartsWith("modal") & id.EndsWith("modal"))
                    {
                        currentElement.SetHtml(messages);
                    }
                });
            }
            catch { }
        }
        async void WebView_JSDeleteMessage(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string usernamemessages = e.Arguments[1] as string;
            string id = e.Arguments[2] as string;
            await DeleteMessage(videoid, id);
            await UpdateModalMessages(videoid, usernamemessages);
        }
        private async Task DeleteMessage(string videoid, string id)
        {
            var jsonString = @"{
                      ""type"": ""service_account"",
                      ""project_id"": ""faceto"",
                      ""private_key_id"": ""09ad60ee345110cde962744ff51579f3acdbb7cc"",
                      ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQC55nLgYpqvzHPa\n/n2k6nL1Q7/NN/GJGwv69Yf1hTPvFmw7+V/cxcvY9qwKd6YurLCoh3S1b99SOob0\nwJh8BLjow/I9ldy4iKCpyQ0/1d1X09fLsbXgWYUwv0fkftj64EgyEcLaFkRYkAmU\nPrZ1KHC8CBi7LmxI1UAiJ5DjHt7X3CznfYaSh52jqyoh7eMgYeiPow7VLzJshMKt\nzS5VsP79oBXhAyOoiH9oz8jcb7A0BCHNeEIZ0nX8i1AqKnXXfT0n/BNKj2886dJO\nIzygkjxAseg5dsIV0bBhxCryKUytr24yBzBxKSfgthy8LOSInpP/lTwKCg1xvV1x\nY30ithAlAgMBAAECggEAM22r+SR+O8U5heuiscWEcRLBlJH1+aKoYVCcwNENaYbQ\nAZV/LjHwL4EqXij0qfPvWWhD4s/kvbhgToSbiq+5wfc3ZE85xTlTDTWIO1E8j0gV\nao4qzTqmzLIWPwHSoDD8+BEO0UuYs9GBPOhOjMHX0kUBJoN0xH9uYySEAjkvmBKh\nJLlbCN++BFuHGHDE/dqrO+4/ygAlvcH7rz2FUayP/Y0YuUnMXHzvCsMxUf68ndZL\n6s9J8bPjVd3sajvlYODSpKLyZ3CgWHz1lVajVmZPXPFFwVqSLqm4V8BMGaZrHe/B\nzhjN2YE3BhX1pX9x+qypvLhFuX6n3t+NwR7+yInOjwKBgQD2flR7vNqJ6C5bSMSk\nc/HsxrEuOluFv5Dtvq6PL1jGGTF44tVX7DZ3puW5AQL9r9y7xanrbW5uahUz5DVk\nbxepkZ8062b/rBp4dpP+W8h1ZFY2aAE9mS9E3PvMidRndw9l7lCOo3mnNbEg+Vor\nfcEQn8UHcRSxtAWAytzA6tN24wKBgQDBEd8UMWFwRcFskUdidWctthok4odC8o2Z\nCNQ3XblvYuNlCZvNL5kJ4wWk1xFcihXhGP54NUrKMIPuB02ZD3eUOvpRjIgbnxvs\nAa0Ka4m9U4jui8B1SJU+/8r52o+b57fVvuhXfH7YXohPFTfe5zQeWS8T2YtZMSnH\ncnLiUfwDVwKBgQDZzbTvBWgBlYRoqrr/KWhqtQLYez5lx2jTersZ0Fdb6+T4EU88\nen+CaJnySD+RVDTyQm2rlq2OqPQFPzAih7tb3U3VX/BKGJPnP7fzeLx/ZmJ7fpki\nCdpnufBQwrVJmz2i7tqFv1N+eYYDQfH4Hg1bmCFsOvJzN0PpktdPK/AgywKBgBmo\nzGMcpPyM6MYLENevDsVufE8GpD9riRIbsEijdi+tjtcwzboZZ1d/CpL72lzYJUxD\nTB6hxozUodQSuGdtPNFAfWA1Mymoncdh+aN241l8Lqi1fiCYQu2ahVlriMaJp08L\nDkoCS8Fp3ufTxBcl1zFpXO5gbTqvZAQT29zkVIAFAoGAFnkHW4JUla4OlQFA6mEj\nvbgvl2PPTIknFIy+xNwv0X5TL8ILT/qG4EPKUKiiLLr81FpBycQ10tFIQgXBC0XC\nxyIOo+KULMjAJP7As/LdFdbFGyTmbj74Va/+KmmYOufUGOtKBju0cPscOpUfj8vT\nDQcuHWik27qG1c95Uw8TWrs=\n-----END PRIVATE KEY-----\n"",
                      ""client_email"": ""faceto@faceto.iam.gserviceaccount.com"",
                      ""client_id"": ""102405338303384679418"",
                      ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                      ""token_uri"": ""https://oauth2.googleapis.com/token"",
                      ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                      ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/faceto%40faceto.iam.gserviceaccount.com""
                    }";
            var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
            db = FirestoreDb.Create("faceto", builder.Build()); 
            DocumentReference docref = db.Collection(videoid).Document(id);
            await docref.DeleteAsync();
        }
        private void TraverseElementTree(JSObject root, Action<JSObject> action)
        {
            action(root);
            foreach (var child in root.GetChildren())
                TraverseElementTree(child, action);
        }
        private void webView1_LoadCompleted(object sender, EO.WebBrowser.LoadCompletedEventArgs e)
        {
            Task.Run(() => LoadPage());
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.webView1.Dispose();
        }
    }
    public static class JSObjectExtensions
    {
        public static void SetValue(this JSObject jsObj, string value)
        {
            jsObj["value"] = value;
        }
        public static void SetHtml(this JSObject jsObj, string value)
        {
            jsObj["innerHTML"] = value;
        }
        public static string GetValue(this JSObject jsObj)
        {
            return jsObj["value"] as string ?? string.Empty;
        }
        public static string GetHtml(this JSObject jsObj)
        {
            return jsObj["innerHTML"] as string ?? string.Empty;
        }
        public static string GetTagName(this JSObject jsObj)
        {
            return (jsObj["tagName"] as string ?? string.Empty).ToUpper();
        }
        public static string GetID(this JSObject jsObj)
        {
            return jsObj["id"] as string ?? string.Empty;
        }
        public static string GetAttribute(this JSObject jsObj, string attribute)
        {
            return jsObj.InvokeFunction("getAttribute", attribute) as string ?? string.Empty;
        }
        public static JSObject GetParent(this JSObject jsObj)
        {
            return jsObj["parentElement"] as JSObject;
        }
        public static IEnumerable<JSObject> GetChildren(this JSObject jsObj)
        {
            var childrenCollection = (JSObject)jsObj["children"];
            int childObjectCount = (int)childrenCollection["length"];
            for (int i = 0; i < childObjectCount; i++)
            {
                yield return (JSObject)childrenCollection[i];
            }
        }
    }
}