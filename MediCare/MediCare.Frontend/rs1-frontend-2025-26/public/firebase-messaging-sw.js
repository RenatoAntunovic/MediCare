importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: "AIzaSyAuPUq4Rm8084w4e74Kzvib8peBhW7BlCQ",
  authDomain: "medicare-a919a.firebaseapp.com",
  projectId: "medicare-a919a",
  storageBucket: "medicare-a919a.firebasestorage.app",
  messagingSenderId: "207583477678",
  appId: "1:207583477678:web:59519150f75230e653523f"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
  console.log('[firebase-messaging-sw.js] Received background message', payload);

  const notificationTitle = payload.notification.title;
  const notificationOptions = {
    body: payload.notification.body
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});
