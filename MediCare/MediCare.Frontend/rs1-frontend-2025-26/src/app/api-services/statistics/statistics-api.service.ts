import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StatisticsDashboardDto, StatisticsDashboardRequest } from './statistics-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({
  providedIn: 'root'
})
export class StatisticsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/Statistics`;
  private http = inject(HttpClient);

  /**
   * GET /api/Statistics/dashboard
   * All data for the admin statistics page (admin only).
   */
  dashboard(request?: StatisticsDashboardRequest): Observable<StatisticsDashboardDto> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<StatisticsDashboardDto>(`${this.baseUrl}/dashboard`, { params });
  }
}
