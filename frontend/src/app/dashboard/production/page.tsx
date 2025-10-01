'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Plus, CheckCircle, Clock, AlertCircle } from 'lucide-react';
import { apiClient } from '@/lib/api-client';
import { formatDate, formatNumber } from '@/lib/utils';
import type { AssemblyOrder } from '@/types';

export default function ProductionPage() {
  const router = useRouter();
  const [orders, setOrders] = useState<AssemblyOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [posting, setPosting] = useState<string | null>(null);

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      const data = await apiClient.getOpenAssemblyOrders();
      setOrders(data);
    } catch (error) {
      console.error('Failed to load orders:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePostAssembly = async (docNo: string) => {
    if (!confirm(`Are you sure you want to post assembly order ${docNo}? This will consume raw materials and produce finished goods.`)) {
      return;
    }

    setPosting(docNo);
    try {
      const result = await apiClient.postAssembly({ orderDocNo: docNo });
      
      if (result.success) {
        alert(`Assembly posted successfully!\n\nDoc No: ${result.docNo}\nTotal Cost: $${result.totalCost.toFixed(2)}\n\nCost Breakdown:\n${result.costBreakdowns.map(cb => `${cb.componentCode}: ${cb.quantity} x $${cb.unitCost} = $${cb.totalCost}`).join('\n')}`);
        loadOrders(); // Refresh list
      } else {
        alert(`Failed to post assembly: ${result.errorMessage}`);
      }
    } catch (error: any) {
      alert(`Error posting assembly: ${error.message}`);
    } finally {
      setPosting(null);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'open':
        return <Clock className="h-4 w-4 text-blue-500" />;
      case 'posted':
        return <CheckCircle className="h-4 w-4 text-green-500" />;
      case 'cancelled':
        return <AlertCircle className="h-4 w-4 text-red-500" />;
      default:
        return null;
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-muted-foreground">Loading production orders...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Production</h1>
          <p className="text-muted-foreground">Manage assembly orders and production workflow</p>
        </div>
        <Button onClick={() => router.push('/dashboard/production/create')}>
          <Plus className="mr-2 h-4 w-4" />
          Create Assembly Order
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Open Orders
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{orders.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Units
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {formatNumber(orders.reduce((sum, o) => sum + o.quantity, 0), 0)}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Today's Production
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {orders.filter(o => 
                new Date(o.productionDate).toDateString() === new Date().toDateString()
              ).length}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Orders Table */}
      <Card>
        <CardHeader>
          <CardTitle>Open Assembly Orders</CardTitle>
          <CardDescription>
            Assembly orders ready for production
          </CardDescription>
        </CardHeader>
        <CardContent>
          {orders.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-muted-foreground mb-4">No open assembly orders</p>
              <Button onClick={() => router.push('/dashboard/production/create')}>
                <Plus className="mr-2 h-4 w-4" />
                Create First Order
              </Button>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Doc No</TableHead>
                  <TableHead>Item</TableHead>
                  <TableHead>Quantity</TableHead>
                  <TableHead>Production Date</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Remarks</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {orders.map((order) => (
                  <TableRow key={order.docNo}>
                    <TableCell className="font-medium">{order.docNo}</TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{order.itemCode}</p>
                        <p className="text-sm text-muted-foreground">
                          {order.itemDescription}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>{formatNumber(order.quantity, 0)}</TableCell>
                    <TableCell>{formatDate(order.productionDate)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        {getStatusIcon(order.status)}
                        <span className="text-sm">{order.status}</span>
                      </div>
                    </TableCell>
                    <TableCell className="max-w-xs truncate">
                      {order.remarks || '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        size="sm"
                        onClick={() => handlePostAssembly(order.docNo)}
                        disabled={posting === order.docNo}
                      >
                        {posting === order.docNo ? 'Posting...' : 'Post Assembly'}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Production Tips */}
      <Card>
        <CardHeader>
          <CardTitle>Production Tips</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2 text-sm text-muted-foreground">
            <li>• Verify raw material availability before posting assembly orders</li>
            <li>• Post assemblies will consume raw materials and produce finished goods in AutoCount</li>
            <li>• Check cost breakdown after posting to verify material costs</li>
            <li>• Print labels immediately after posting for batch tracking</li>
            <li>• Multi-level BOMs are supported for complex assemblies</li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}

