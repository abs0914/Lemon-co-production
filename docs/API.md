# Lemon Co Production API Documentation

Base URL: `https://localhost:5001`

## Authentication

Currently using simple token-based authentication. Include token in header:
```
Authorization: Bearer YOUR_TOKEN
```

## Endpoints

### Health Check

#### GET /health
Check API and AutoCount connection status.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-01-30T10:00:00Z",
  "autoCountConnected": true
}
```

---

## Items & BOM

### GET /items
Search for items.

**Query Parameters:**
- `search` (optional): Search term for item code or description

**Response:**
```json
[
  {
    "itemCode": "STR-500ML",
    "description": "Strawberry Flavor 500ml",
    "baseUom": "BTL",
    "itemType": "Finished",
    "barcode": "8888000000001",
    "hasBom": true,
    "stockBalance": 150,
    "standardCost": 12.50
  }
]
```

### GET /items/{itemCode}
Get specific item details.

**Response:** Same as single item above

### POST /items
Create a new item.

**Request Body:**
```json
{
  "itemCode": "NEW-ITEM",
  "description": "New Item Description",
  "baseUom": "PCS",
  "itemType": "Finished",
  "barcode": "1234567890",
  "hasBom": false,
  "standardCost": 10.00
}
```

### GET /boms/{itemCode}
Get BOM for an item.

**Response:**
```json
[
  {
    "componentCode": "RAW-STR",
    "qtyPer": 0.3,
    "uom": "KG",
    "description": "Raw Strawberry Extract",
    "sequence": 1
  }
]
```

### POST /boms/{itemCode}
Create or update BOM for an item.

**Request Body:**
```json
[
  {
    "componentCode": "RAW-STR",
    "qtyPer": 0.3,
    "uom": "KG",
    "description": "Raw Strawberry Extract",
    "sequence": 1
  },
  {
    "componentCode": "PKG-BTL-500",
    "qtyPer": 1,
    "uom": "PCS",
    "description": "500ml Bottle",
    "sequence": 2
  }
]
```

### POST /boms/{itemCode}/import-csv
Import BOM from CSV.

**Request Body:**
```json
{
  "csvContent": "ComponentCode,QtyPer,UOM,Description\nRAW-STR,0.3,KG,Raw Strawberry\nPKG-BTL-500,1,PCS,Bottle"
}
```

---

## Assembly Orders

### POST /assembly-orders
Create a new assembly order.

**Request Body:**
```json
{
  "itemCode": "STR-500ML",
  "quantity": 100,
  "productionDate": "2025-01-30",
  "remarks": "Batch 001"
}
```

**Response:**
```json
{
  "docNo": "ASM-20250130-1001",
  "itemCode": "STR-500ML",
  "itemDescription": "Strawberry Flavor 500ml",
  "quantity": 100,
  "productionDate": "2025-01-30T00:00:00Z",
  "remarks": "Batch 001",
  "status": "Open",
  "createdDate": "2025-01-30T10:00:00Z"
}
```

### GET /assembly/orders/{docNo}
Get assembly order details.

### GET /assembly/orders/open
Get all open assembly orders.

**Response:** Array of assembly orders

### POST /assemblies/post
Post an assembly order (consume materials, produce finished goods).

**Request Body:**
```json
{
  "orderDocNo": "ASM-20250130-1001"
}
```

**Response:**
```json
{
  "docNo": "ASM-20250130-1001",
  "success": true,
  "totalCost": 1250.00,
  "costBreakdowns": [
    {
      "componentCode": "RAW-STR",
      "description": "Raw Strawberry Extract",
      "quantity": 30,
      "unitCost": 25.00,
      "totalCost": 750.00
    },
    {
      "componentCode": "PKG-BTL-500",
      "description": "500ml Bottle",
      "quantity": 100,
      "unitCost": 1.50,
      "totalCost": 150.00
    }
  ]
}
```

### DELETE /assembly/orders/{docNo}
Cancel an assembly order.

---

## Sales Orders (Integration API)

### POST /sales-orders
Create sales order from external platform.

**Request Body:**
```json
{
  "customerCode": "CUST001",
  "lines": [
    {
      "itemCode": "STR-500ML",
      "qty": 50,
      "unitPrice": 15.00,
      "discountPercent": 5,
      "remarks": "Urgent order"
    }
  ],
  "remarks": "From franchise portal",
  "externalRef": "FP-2025-001",
  "deliveryDate": "2025-02-05"
}
```

**Response:**
```json
{
  "soNo": "SO-20250130-1001",
  "status": "Created",
  "success": true,
  "errors": [],
  "totalAmount": 712.50
}
```

**Error Response:**
```json
{
  "soNo": "",
  "status": "Validation Failed",
  "success": false,
  "errors": [
    "Customer CUST999 not found in AutoCount",
    "Item INVALID-ITEM not found in AutoCount"
  ]
}
```

### GET /sales-orders/validate-customer/{customerCode}
Validate customer exists.

**Response:**
```json
{
  "customerCode": "CUST001",
  "isValid": true
}
```

### POST /sales-orders/validate-items
Validate multiple items exist.

**Request Body:**
```json
["STR-500ML", "MNG-500ML", "INVALID-ITEM"]
```

**Response:**
```json
{
  "allValid": false,
  "invalidItems": ["INVALID-ITEM"]
}
```

---

## Labels & Barcodes

### POST /labels/print
Generate barcode label.

**Request Body:**
```json
{
  "itemCode": "STR-500ML",
  "batchNo": "B001",
  "mfgDate": "2025-01-30",
  "expDate": "2025-07-30",
  "copies": 2,
  "format": "ZPL"
}
```

**Response (ZPL):**
```json
{
  "success": true,
  "content": "^XA\n^FO50,50^A0N,50,50^FDSTR-500ML^FS\n...",
  "contentType": "text/plain"
}
```

**Response (PDF):**
```json
{
  "success": true,
  "content": "JVBERi0xLjQKJeLjz9MKMSAwIG9iago8PC...",
  "contentType": "application/pdf"
}
```

### GET /labels/barcode-config
Get barcode configuration.

**Response:**
```json
{
  "type": "Code128",
  "qtySeparator": "*",
  "includeQty": false
}
```

### PUT /labels/barcode-config
Update barcode configuration.

**Request Body:**
```json
{
  "type": "QR",
  "qtySeparator": "*",
  "includeQty": true
}
```

### POST /labels/parse-barcode
Parse barcode scanner input with quantity separator.

**Request Body:**
```json
{
  "input": "STR-500ML*5"
}
```

**Response:**
```json
{
  "itemCode": "STR-500ML",
  "quantity": 5
}
```

---

## Error Responses

All endpoints return standard error responses:

**400 Bad Request:**
```json
{
  "error": "Item STR-500ML does not have a BOM"
}
```

**404 Not Found:**
```json
{
  "error": "Assembly order ASM-20250130-9999 not found"
}
```

**500 Internal Server Error:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "AutoCount connection failed"
}
```

---

## Rate Limiting

No rate limiting currently implemented. Recommended for production:
- 100 requests per minute per IP
- 1000 requests per hour per API key

## Webhooks (Future)

Planned webhook support for:
- Assembly order completed
- Stock level alerts
- Production milestones

## SDK Examples

### JavaScript/TypeScript
```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:5001',
  headers: { 'Authorization': 'Bearer YOUR_TOKEN' }
});

// Create assembly order
const order = await api.post('/assembly-orders', {
  itemCode: 'STR-500ML',
  quantity: 100,
  productionDate: '2025-01-30'
});

console.log('Order created:', order.data.docNo);
```

### C#
```csharp
using System.Net.Http;
using System.Text.Json;

var client = new HttpClient { 
  BaseAddress = new Uri("https://localhost:5001") 
};

var order = new {
  itemCode = "STR-500ML",
  quantity = 100,
  productionDate = "2025-01-30"
};

var response = await client.PostAsJsonAsync("/assembly-orders", order);
var result = await response.Content.ReadFromJsonAsync<AssemblyOrder>();
```

### Python
```python
import requests

api_url = "https://localhost:5001"
headers = {"Authorization": "Bearer YOUR_TOKEN"}

order = {
    "itemCode": "STR-500ML",
    "quantity": 100,
    "productionDate": "2025-01-30"
}

response = requests.post(
    f"{api_url}/assembly-orders",
    json=order,
    headers=headers
)

print(f"Order created: {response.json()['docNo']}")
```

