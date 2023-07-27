
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

firebase.auth().setPersistence(firebase.auth.Auth.Persistence.SESSION).then(() => {
	return firebase.auth().signInWithEmailAndPassword(email, password);
}).catch((error) => {
	var errorCode = error.code;
	var errorMessage = error.message;
});

var ui = new firebaseui.auth.AuthUI(firebase.auth());

ui.start('#firebaseui-auth-container', {
	signInSuccessUrl: '/play.html',
	signInOptions: [
		firebase.auth.EmailAuthProvider.PROVIDER_ID
	],
});