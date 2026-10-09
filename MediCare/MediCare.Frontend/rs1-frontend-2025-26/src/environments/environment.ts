export const environment = {
  production: false,
  apiUrl: 'https://localhost:7260',

  // Firebase web config – these values are public by design (they end up in every user's browser)
  firebase: {
    apiKey: 'AIzaSyCVQA4rlQXC5apKPM_dxopwVWcZYovx6l8',
    authDomain: 'medicare-6720f.firebaseapp.com',
    projectId: 'medicare-6720f',
    storageBucket: 'medicare-6720f.firebasestorage.app',
    messagingSenderId: '993016260544',
    appId: '1:993016260544:web:844b5ee6d13e35fe5e4a25'
  },
  // Web Push certificate (Firebase console → Project settings → Cloud Messaging)
  firebaseVapidKey: 'BA6kMIYdZMSdisNOTYLWo1aVIXBH4yHnPxb_AlC0z6-1f-NbZXPV7btHs9M_JSaDT9ti-9Xlp_9Ti8J5Xsfv_lM'
};