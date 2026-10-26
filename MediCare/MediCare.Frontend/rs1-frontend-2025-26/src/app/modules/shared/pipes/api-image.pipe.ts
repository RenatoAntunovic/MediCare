import { Pipe, PipeTransform } from '@angular/core';
import { apiImageUrl } from '../../../core/utils/api-image-url';

/**
 * Turns an image path from the API into a full URL.
 * Usage: <img [src]="item.imagePath | apiImage">
 */
@Pipe({ name: 'apiImage', standalone: true })
export class ApiImagePipe implements PipeTransform {
  transform(path: string | null | undefined): string {
    return apiImageUrl(path);
  }
}