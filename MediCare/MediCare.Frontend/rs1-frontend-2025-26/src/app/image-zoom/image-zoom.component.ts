import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-image-zoom',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-zoom.component.html',
  styleUrls: ['./image-zoom.component.scss'],
})
export class ImageZoomComponent {
  isZoomed: boolean = false;

  toggleZoom(): void {
    this.isZoomed = !this.isZoomed;
  }
}
