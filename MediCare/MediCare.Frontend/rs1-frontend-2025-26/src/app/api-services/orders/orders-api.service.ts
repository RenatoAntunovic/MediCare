import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListOrdersRequest,
  ListOrdersResponse,
  ListOrdersWithItemsRequest,
  ListOrdersWithItemsResponse,
  GetOrderByIdQueryDto,
  CreateOrderCommand,
  UpdateOrderCommand
} from './orders-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

/** Parameters for the PDF report. Dates are "yyyy-MM-dd", everything is optional. */
export interface OrdersReportParams {
  from?: string | null;
  to?: string | null;
  statusId?: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class OrdersApiService {
  private readonly baseUrl = `${environment.apiUrl}/Orders`;
  private http = inject(HttpClient);

  /**
   * GET /Orders
   * List orders with optional query parameters.
   */
  list(request?: ListOrdersRequest): Observable<ListOrdersResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListOrdersResponse>(this.baseUrl, { params });
  }

  /**
   * GET /Orders/with-items
   */
  listWithItems(request?: ListOrdersWithItemsRequest): Observable<ListOrdersWithItemsResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListOrdersWithItemsResponse>(`${this.baseUrl}/with-items`, { params });
  }

  /**
   * GET /Orders/{id}
   */
  getById(id: number): Observable<GetOrderByIdQueryDto> {
    return this.http.get<GetOrderByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * POST /Orders
   */
  create(payload: CreateOrderCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, payload);
  }

  /**
   * PUT /Orders/{id}
   */
  update(id: number, payload: UpdateOrderCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /**
   * PUT /Orders/{id}/change-status
   */
  changeStatus(id: number, newStatusId: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/change-status`, { newStatusId });
  }

  /**
   * GET /Orders/{id}/pdf – single order PDF
   */
  downloadOrderPdf(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  /**
   * GET /Orders/report/pdf?from=...&to=...&statusId=... – parameterized report (Admin)
   */
  downloadReportPdf(p: OrdersReportParams): Observable<Blob> {
    let params = new HttpParams();
    if (p.from) params = params.set('from', p.from);
    if (p.to) params = params.set('to', p.to);
    if (p.statusId) params = params.set('statusId', p.statusId);

    return this.http.get(`${this.baseUrl}/report/pdf`, { params, responseType: 'blob' });
  }
}
