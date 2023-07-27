var videoid = "";
var videotitle = "";
var author = "";
var msg = "";
var resindicex = window.screen.availWidth;
var resindicey = window.screen.availHeight;
var polling;
var index = 0;
var search = "";
var indexsearch = 0;

document.addEventListener('DOMContentLoaded', function() {
	try {
		let app = firebase.app();
		let features = [
			'auth',
			'database',
			'firestore',
			'functions',
			'messaging',
			'storage',
			'analytics',
			'remoteConfig',
			'performance',
		].filter(feature => typeof app[feature] === 'function');
	} catch (e) {
		console.error(e);
	}
});

var firebaseConfig = {
	apiKey: "AIzaSyD1tJc6PA5iUZyQpztBoumRZ5BzhsENL6w",
	authDomain: "go-face-so.firebaseapp.com",
	databaseURL: "https://go-face-so-default-rtdb.firebaseio.com",
	projectId: "go-face-so",
	storageBucket: "go-face-so.appspot.com",
	messagingSenderId: "864044517590",
	appId: "1:864044517590:web:4651fb286b1a394a1ae177",
	measurementId: "G-5QC469PC35"
};

firebase.initializeApp(firebaseConfig);

firebase.analytics();

const db = firebase.firestore();

$(".linking").click(function() {
	index = 0;
	var link = $(".link").val();
	videoid = link.replace("https://www.youtube.com/watch?v=", "");
	videoid = videoid.replace("http://www.youtube.com/watch?v=", "");
	videoid = videoid.replace("https://www.youtu.be/watch?v=", "");
	videoid = videoid.replace("http://www.youtu.be/watch?v=", "");
    $.ajax({
        type: "GET",
        async: false,
        cache: false,
        url: "https://noembed.com/embed?url=https://www.youtube.com/watch?v=" + videoid,
        dataType: "json",
        success: function(data) {
            if (data.error != "404 Not Found") {
				videotitle = data.author_name + " : " + data.title;
				var file = "https://www.youtube.com/embed/" + videoid;
				var htmlString = "";
				htmlString = `<div class="mySlides" data-link="` + file + `">
                                <iframe src="` + file + `" frameborder="0" allowfullscreen class="content" style="width:` + resindicex * 50 / 100 + `px;height:` + 9 / 16 * resindicex * 50 / 100 + `px;"></iframe>
                            </div>`;
				$(".slideshow-container").empty();
				$(".slideshow-container").append(htmlString);
				var t = Date.now() / 1000 | 0;
				db.collection('list').doc(videoid).set({ videotitle: stringToArray(videotitle), date : t }, { merge: true })
				updateMsg();
				try { 
					clearInterval(polling);
					polling = setInterval(updateMsg, 20000);
				}
				catch {
					polling = setInterval(updateMsg, 20000);
				}
            }
        }
    });
});

$(".messaging").click(function() {
	msg = $(".msg").val();
	author = $(".author").val();
	if (videotitle != "" & msg != "" & author != "" & msg != "Message !" & author != "Author") {
		var t = Date.now() / 1000 | 0;
		db.collection(videoid).add({
			videotitle: videotitle,
			author: author,
			msg: msg,
			date : t
		}).then((docRef) => {
			console.log("Document written with ID: ", docRef.id);
			updateMsg();
		}).catch((error) => {
			console.error("Error adding document: ", error);
		});
	}
});

function updateMsg() {
	var i = 0;
	var htmlString = "";
	db.collection(videoid).orderBy("date").get().then((querySnapshot) => {
		querySnapshot.forEach((doc) => {
			if (i == 0) {
				htmlString = `<div class="comments"><p>` + doc.data().videotitle + `</p>`;
			}
			var msg = doc.data().msg;
			var author = doc.data().author;
			if (msg != "" & author != "") {
				htmlString += `<p>` + author + ` : ` + msg + `</p>`;
			}
			i++;
		});
		htmlString += `</div>`;
		$(".comment-container").empty();
		$(".comment-container").append(htmlString);
	});
}

showList(0);

function openLink(img) {
	var src = img.src;
	var id = src.replace("https://img.youtube.com/vi/", "https://www.youtube.com/watch?v=");
	id = id.replace("/mqdefault.jpg", "");
	$(".link:text").val(id);
	$( ".linking" ).click();
}

function minusIndex() {
	index = index - 15;
	if (index < 0) {
		index = 0;
	}
	showList(index);
}

function plusIndex() {
	index = index + 15;
	showList(index);
}

function showList(index) {
	var HTMLStr = "";
	db.collection('list').orderBy("date").get().then((querySnapshot) => {
		var visible = querySnapshot.docs[index];
		db.collection('list').orderBy("date").startAt(visible).limit(15).get().then((querySnapshot) => {
			querySnapshot.forEach((doc) => {
				var id = doc.id;
				var title = arrayToString(doc.data().videotitle);
				var src = "https://img.youtube.com/vi/" + id + "/mqdefault.jpg";
				HTMLStr += `<span><img onclick="openLink(this)" class="image" src="` + src + `">
								<div class="title">` + title + `</div></span>`;
				HTMLStr += `<div></div>`;
				HTMLStr += `<div class="showlist"><span onclick="minusIndex()" class="minus"><</span>
								<span onclick="plusIndex()" class="plus">></span></div>`;
			});
			$(".list-container").empty();
			$(".list-container").append(HTMLStr);
		});
	});
}

$(".searching").click(function() {
	indexsearch = 0;
	index = 0;
	if ($(".search").val() != "Youtube video search" & $(".search").val() != "") {
		showListSearch(0);
	}
	else {
		showList(0);
	}
});

function minusIndexSearch() {
	indexsearch = indexsearch - 15;
	if (indexsearch < 0) {
		indexsearch = 0;
	}
	showListSearch(indexsearch);
}

function plusIndexSearch() {
	indexsearch = indexsearch + 15;
	showListSearch(indexsearch);
}

function showListSearch(indexsearch) {
	var HTMLStr = "";
	search = $(".search").val();
	db.collection('list').where('videotitle', 'array-contains-any', stringToArray(search)).get().then((querySnapshot) => {
		var visible = querySnapshot.docs[indexsearch];
		db.collection('list').where('videotitle', 'array-contains-any', stringToArray(search)).startAt(visible).limit(15).get().then((querySnapshot) => {
			querySnapshot.forEach((doc) => {
				var id = doc.id;
				var title = arrayToString(doc.data().videotitle);
				var src = "https://img.youtube.com/vi/" + id + "/mqdefault.jpg";
				HTMLStr += `<span><img onclick="openLink(this)" class="image" src="` + src + `">
								<div class="title">` + title + `</div></span>`;
				HTMLStr += `<div></div>`;
				HTMLStr += `<div class="showlist"><span onclick="minusIndexSearch()" class="minus"><</span>
								<span onclick="plusIndexSearch()" class="plus">></span></div>`;
			});
			$(".list-container").empty();
			$(".list-container").append(HTMLStr);
		});
	});
}

function arrayToString(arr) {
	return arr.join(" ");
}

function stringToArray(str) {
	return str.split(' ');
}
