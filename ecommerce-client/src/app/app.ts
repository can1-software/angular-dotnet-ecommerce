import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './components/header/header';

// Uygulamanın kök bileşeni.
// Üstte her zaman Header, altında ise o anki sayfa (RouterOutlet) gösterilir.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header],
  templateUrl: './app.html',
})
export class App {}
