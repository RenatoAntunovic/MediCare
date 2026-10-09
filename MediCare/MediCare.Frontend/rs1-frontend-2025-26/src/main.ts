import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';

import 'zone.js'; // Razvoj softvera 1 setup, install "npm install zone.js" first

platformBrowser().bootstrapModule(AppModule, {

})
  .catch(err => console.error(err));
