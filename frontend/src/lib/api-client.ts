import axios, { AxiosInstance } from 'axios';
import type {
  Item,
  BomLine,
  AssemblyOrder,
  AssemblyOrderInput,
  PostAssemblyInput,
  PostAssemblyResult,
  SalesOrderInput,
  SalesOrderResult,
  LabelPrintInput,
  LabelPrintResult,
  BarcodeConfig,
} from '@/types';

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add request interceptor for auth token
    this.client.interceptors.request.use((config) => {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Add response interceptor for error handling
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Redirect to login
          if (typeof window !== 'undefined') {
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }

  // Health check
  async healthCheck() {
    const response = await this.client.get('/health');
    return response.data;
  }

  // Items
  async searchItems(search?: string): Promise<Item[]> {
    const response = await this.client.get('/items', {
      params: { search },
    });
    return response.data;
  }

  async getItem(itemCode: string): Promise<Item> {
    const response = await this.client.get(`/items/${itemCode}`);
    return response.data;
  }

  async createItem(item: Item): Promise<Item> {
    const response = await this.client.post('/items', item);
    return response.data;
  }

  async updateItem(itemCode: string, item: Item): Promise<Item> {
    const response = await this.client.put(`/items/${itemCode}`, item);
    return response.data;
  }

  // BOMs
  async getBom(itemCode: string): Promise<BomLine[]> {
    const response = await this.client.get(`/boms/${itemCode}`);
    return response.data;
  }

  async saveBom(itemCode: string, bomLines: BomLine[]): Promise<void> {
    await this.client.post(`/boms/${itemCode}`, bomLines);
  }

  async importBomFromCsv(itemCode: string, csvContent: string): Promise<void> {
    await this.client.post(`/boms/${itemCode}/import-csv`, { csvContent });
  }

  // Assembly Orders
  async createAssemblyOrder(input: AssemblyOrderInput): Promise<AssemblyOrder> {
    const response = await this.client.post('/assembly-orders', input);
    return response.data;
  }

  async getAssemblyOrder(docNo: string): Promise<AssemblyOrder> {
    const response = await this.client.get(`/assembly/orders/${docNo}`);
    return response.data;
  }

  async getOpenAssemblyOrders(): Promise<AssemblyOrder[]> {
    const response = await this.client.get('/assembly/orders/open');
    return response.data;
  }

  async postAssembly(input: PostAssemblyInput): Promise<PostAssemblyResult> {
    const response = await this.client.post('/assemblies/post', input);
    return response.data;
  }

  async cancelAssemblyOrder(docNo: string): Promise<void> {
    await this.client.delete(`/assembly/orders/${docNo}`);
  }

  // Sales Orders
  async createSalesOrder(input: SalesOrderInput): Promise<SalesOrderResult> {
    const response = await this.client.post('/sales-orders', input);
    return response.data;
  }

  async validateCustomer(customerCode: string): Promise<boolean> {
    const response = await this.client.get(`/sales-orders/validate-customer/${customerCode}`);
    return response.data.isValid;
  }

  async validateItems(itemCodes: string[]): Promise<string[]> {
    const response = await this.client.post('/sales-orders/validate-items', itemCodes);
    return response.data.invalidItems;
  }

  // Labels
  async printLabel(input: LabelPrintInput): Promise<LabelPrintResult> {
    const response = await this.client.post('/labels/print', input);
    return response.data;
  }

  async getBarcodeConfig(): Promise<BarcodeConfig> {
    const response = await this.client.get('/labels/barcode-config');
    return response.data;
  }

  async updateBarcodeConfig(config: BarcodeConfig): Promise<void> {
    await this.client.put('/labels/barcode-config', config);
  }

  async parseBarcode(input: string): Promise<{ itemCode: string; quantity: number }> {
    const response = await this.client.post('/labels/parse-barcode', { input });
    return response.data;
  }
}

export const apiClient = new ApiClient();

