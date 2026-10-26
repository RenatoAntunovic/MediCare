// === QUERIES (READ) ===

/**
 * Query parameters for GET /api/Statistics/dashboard
 * Corresponds to: StatisticsDashboardQuery.cs
 */
export interface StatisticsDashboardRequest {
  from?: string | null; // yyyy-MM-dd
  to?: string | null; // yyyy-MM-dd
  lowStockThreshold?: number;
}

/**
 * Response for GET /api/Statistics/dashboard
 * Corresponds to: StatisticsDashboardDto.cs
 */
export interface StatisticsDashboardDto {
  from: string | null;
  to: string | null;

  ordersRevenue: number;
  reservationsRevenue: number;
  ordersCount: number;
  reservationsCount: number;
  newUsersCount: number;
  lowStockCount: number;

  byMonth: StatisticsMonthDto[];
  ordersByStatus: StatisticsCountDto[];
  reservationsByStatus: StatisticsCountDto[];
  reservationsByTreatment: StatisticsCountDto[];
  reservationsByWeekday: StatisticsCountDto[]; // label = "Monday" … "Friday"
  topMedicines: StatisticsTopMedicineDto[];
  lowStock: StatisticsStockDto[];
}

export interface StatisticsMonthDto {
  year: number;
  month: number; // 1–12
  ordersRevenue: number;
  reservationsRevenue: number;
  ordersCount: number;
  reservationsCount: number;
}

export interface StatisticsCountDto {
  label: string;
  count: number;
}

export interface StatisticsTopMedicineDto {
  name: string;
  quantity: number;
  revenue: number;
}

export interface StatisticsStockDto {
  name: string;
  quantityInStock: number;
}
