import { Component, ElementRef, HostListener, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { MedicineApiService } from '../../../../api-services/medicine/medicine-api.service';
import { GetMedicineByIdQueryDto } from '../../../../api-services/medicine/medicine-api.models';
import { CartsApiService } from '../../../../api-services/carts/carts-api.service';
import { FavouritesService } from '../../../../api-services/favourites/favourites-api.service';
import { ForLaterApiService } from '../../../../api-services/for-later/for-later-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { environment } from '../../../../../environments/environment';

/** How much the image is magnified on click. */
const ZOOM_LEVEL = 2.5;
/** Movement (px) after which a gesture counts as a "drag" rather than a click. */
const DRAG_THRESHOLD = 4;

@Component({
  selector: 'app-medicine-detail',
  standalone: false,
  templateUrl: './medicine-detail.component.html',
  styleUrls: ['./medicine-detail.component.scss']
})
export class MedicineDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private medicineService = inject(MedicineApiService);
  private cartApi = inject(CartsApiService);
  private favApi = inject(FavouritesService);
  private forLaterApi = inject(ForLaterApiService);
  private toaster = inject(ToasterService);
  private location = inject(Location);

  @ViewChild('zoomImage') zoomImage?: ElementRef<HTMLImageElement>;

  selectedDose = '';
  selectedPackage = '';
  errorMessage = '';
  isLoading = true;
  quantity = 1;

  medicine: GetMedicineByIdQueryDto = {
    id: 0,
    name: '',
    description: '',
    price: 0,
    medicineCategoryId: 0,
    medicineCategoryName: '',
    imagePath: '',
    weight: 0,
    isEnabled: false,
  };

  // ==================== IMAGE ZOOM STATE ====================
  zoomLevel = 1;
  /** Image offset in "unscaled" pixels (applied inside the scale transform). */
  offsetX = 0;
  offsetY = 0;
  isDragging = false;

  private pointerDown = false;
  private hasMoved = false;
  private dragStartX = 0;
  private dragStartY = 0;
  private lastOffsetX = 0;
  private lastOffsetY = 0;
  /** Image dimensions at normal size – used to clamp panning. */
  private baseWidth = 0;
  private baseHeight = 0;

  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id');
    if (!idStr) {
      this.errorMessage = 'ID nije pronađen u URL-u';
      this.isLoading = false;
      return;
    }

    this.medicineService.getById(+idStr).subscribe({
      next: (data) => {
        this.medicine = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Greška pri dohvaćanju podataka o lijeku';
        this.isLoading = false;
      }
    });
  }

  get imageUrl(): string {
    const path = this.medicine?.imagePath;
    if (!path) return '';
    return `${environment.apiUrl}/${path.replace(/^\/+/, '')}`;
  }

  get imageTransform(): string {
    return `scale(${this.zoomLevel}) translate(${this.offsetX}px, ${this.offsetY}px)`;
  }

  get imageCursor(): string {
    if (this.isDragging) return 'grabbing';
    return this.zoomLevel > 1 ? 'grab' : 'zoom-in';
  }

  goBack(): void {
    this.location.back();
  }

  // ==================== CART / FAVOURITES / FOR LATER ====================

  addToCart(): void {
    if (!this.medicine || !this.quantity || this.quantity < 1) {
      this.toaster.error('Količina mora biti najmanje 1');
      return;
    }

    this.cartApi.addToCart({ medicineId: this.medicine.id, quantity: this.quantity }).subscribe({
      next: () => this.toaster.success('Dodano u korpu'),
      error: (err) => {
        console.error(err);
        this.toaster.error('Greška pri dodavanju u korpu');
      }
    });
  }

  addToFavourites(): void {
    if (!this.medicine) return;

    this.favApi.addToFavourites({ medicineId: this.medicine.id }).subscribe({
      next: () => this.toaster.success('Dodano u favorite'),
      error: (err) => {
        console.error(err);
        this.toaster.error('Greška pri dodavanju u favorite');
      }
    });
  }

  addToForLater(): void {
    if (!this.medicine) return;

    this.forLaterApi.addToForLater({ medicineId: this.medicine.id }).subscribe({
      next: () => this.toaster.success('Sačuvano za kasnije'),
      error: (err) => {
        console.error(err);
        this.toaster.error('Greška pri spremanju za kasnije');
      }
    });
  }

  // ==================== IMAGE ZOOM ====================
  // Pointer events work the same for mouse, touch (mobile) and pen.

  onPointerDown(event: PointerEvent): void {
    this.pointerDown = true;
    this.hasMoved = false;

    if (this.zoomLevel === 1) return;

    event.preventDefault();
    (event.currentTarget as HTMLElement).setPointerCapture?.(event.pointerId);
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.lastOffsetX = this.offsetX;
    this.lastOffsetY = this.offsetY;
  }

  onPointerMove(event: PointerEvent): void {
    if (!this.pointerDown || this.zoomLevel === 1) return;

    const deltaX = event.clientX - this.dragStartX;
    const deltaY = event.clientY - this.dragStartY;

    if (!this.hasMoved && Math.hypot(deltaX, deltaY) < DRAG_THRESHOLD) return;

    event.preventDefault();
    this.hasMoved = true;
    this.isDragging = true;

    // Translate is applied inside the scale → divide by zoomLevel
    // so the image follows the pointer 1:1 instead of twice as fast.
    this.setOffset(
      this.lastOffsetX + deltaX / this.zoomLevel,
      this.lastOffsetY + deltaY / this.zoomLevel
    );
  }

  onPointerUp(event: PointerEvent): void {
    (event.currentTarget as HTMLElement).releasePointerCapture?.(event.pointerId);
    this.pointerDown = false;
    this.isDragging = false;
    // hasMoved is NOT reset here – the click event fires after pointerup
    // and must know this was a drag.
  }

  onImageClick(event: MouseEvent): void {
    if (this.hasMoved) {
      // A drag just ended – do not change the zoom
      this.hasMoved = false;
      return;
    }

    if (this.zoomLevel === 1) {
      this.zoomIn(event);
    } else {
      this.resetZoom();
    }
  }

  /** Esc returns the image to normal size. */
  @HostListener('document:keydown.escape')
  resetZoom(): void {
    this.zoomLevel = 1;
    this.offsetX = 0;
    this.offsetY = 0;
    this.isDragging = false;
    this.pointerDown = false;
  }

  /** Keyboard: Enter/Space on the focused image zooms in/out (image center). */
  onImageKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter' && event.key !== ' ') return;
    event.preventDefault();

    if (this.zoomLevel === 1) {
      this.captureBaseSize();
      this.zoomLevel = ZOOM_LEVEL;
      this.offsetX = 0;
      this.offsetY = 0;
    } else {
      this.resetZoom();
    }
  }

  private zoomIn(event: MouseEvent): void {
    const img = this.zoomImage?.nativeElement;
    if (!img) return;

    // Read the dimensions while the image is still at 1× (no transform)
    const rect = img.getBoundingClientRect();
    this.captureBaseSize();

    const clickX = event.clientX - rect.left;
    const clickY = event.clientY - rect.top;

    this.zoomLevel = ZOOM_LEVEL;
    // The point the user clicked moves to the center
    this.setOffset(rect.width / 2 - clickX, rect.height / 2 - clickY);
  }

  private captureBaseSize(): void {
    const img = this.zoomImage?.nativeElement;
    if (!img) return;
    const rect = img.getBoundingClientRect();
    this.baseWidth = rect.width / this.zoomLevel;
    this.baseHeight = rect.height / this.zoomLevel;
  }

  /**
   * Clamps the offset so the magnified image always covers the frame
   * (it cannot be pushed out of view leaving empty space).
   */
  private setOffset(x: number, y: number): void {
    const maxX = (this.baseWidth * (this.zoomLevel - 1)) / (2 * this.zoomLevel);
    const maxY = (this.baseHeight * (this.zoomLevel - 1)) / (2 * this.zoomLevel);

    this.offsetX = Math.max(-maxX, Math.min(maxX, x));
    this.offsetY = Math.max(-maxY, Math.min(maxY, y));
  }
}
