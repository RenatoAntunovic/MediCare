// Firebase Cloud Messaging service worker.
// It runs in the background, so notifications arrive even when the MediCare tab is closed.
// Messages that contain a "notification" part are displayed automatically by Firebase.
importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: 'AIzaSyCVQA4rlQXC5apKPM_dxopwVWcZYovx6l8',
  authDomain: 'medicare-6720f.firebaseapp.com',
  projectId: 'medicare-6720f',
  storageBucket: 'medicare-6720f.firebasestorage.app',
  messagingSenderId: '993016260544',
  appId: '1:993016260544:web:844b5ee6d13e35fe5e4a25'
});

firebase.messaging();