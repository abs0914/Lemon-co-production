// Item types
export interface Item {
  itemCode: string;
  description: string;
  baseUom: string;
  itemType: string;
  barcode?: string;
  hasBom: boolean;
  stockBalance: number;
  standardCost: number;
}

// BOM types
export interface BomLine {
  componentCode: string;
  qtyPer: number;
  uom: string;
  description?: string;
  sequence: number;
}

// Assembly Order types
export interface AssemblyOrder {
  docNo: string;
  itemCode: string;
  itemDescription?: string;
  quantity: number;
  productionDate: string;
  remarks?: string;
  status: string;
  createdDate: string;
  postedDate?: string;
  totalCost?: number;
}

export interface AssemblyOrderInput {
  itemCode: string;
  quantity: number;
  productionDate: string;
  remarks?: string;
}

export interface PostAssemblyInput {
  orderDocNo: string;
}

export interface CostBreakdown {
  componentCode: string;
  description: string;
  quantity: number;
  unitCost: number;
  totalCost: number;
}

export interface PostAssemblyResult {
  docNo: string;
  success: boolean;
  errorMessage?: string;
  totalCost: number;
  costBreakdowns: CostBreakdown[];
}

// Sales Order types
export interface SalesOrderLine {
  itemCode: string;
  qty: number;
  unitPrice?: number;
  discountPercent?: number;
  remarks?: string;
}

export interface SalesOrderInput {
  customerCode: string;
  lines: SalesOrderLine[];
  remarks?: string;
  externalRef?: string;
  deliveryDate?: string;
}

export interface SalesOrderResult {
  soNo: string;
  status: string;
  success: boolean;
  errors: string[];
  totalAmount?: number;
}

// Label types
export interface LabelPrintInput {
  itemCode: string;
  batchNo?: string;
  mfgDate?: string;
  expDate?: string;
  copies: number;
  format: 'ZPL' | 'PDF';
}

export interface LabelPrintResult {
  success: boolean;
  content?: string;
  contentType: string;
  errorMessage?: string;
}

export interface BarcodeConfig {
  type: string;
  qtySeparator: string;
  includeQty: boolean;
}

// User types
export interface User {
  id: number;
  username: string;
  fullName: string;
  role: 'Admin' | 'Production' | 'Warehouse';
  email?: string;
}

// Auth types
export interface LoginCredentials {
  username: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  token?: string;
}

