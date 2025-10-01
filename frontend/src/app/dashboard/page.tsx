'use client';

import { useEffect, useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Package, Factory, CheckCircle, Clock } from 'lucide-react';
import { apiClient } from '@/lib/api-client';
import type { AssemblyOrder } from '@/types';

export default function DashboardPage() {
  const [openOrders, setOpenOrders] = useState<AssemblyOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [healthStatus, setHealthStatus] = useState<any>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [orders, health] = await Promise.all([
        apiClient.getOpenAssemblyOrders(),
        apiClient.healthCheck(),
      ]);
      setOpenOrders(orders);
      setHealthStatus(health);
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  const stats = [
    {
      title: 'Open Orders',
      value: openOrders.length,
      icon: Clock,
      color: 'text-blue-600',
      bgColor: 'bg-blue-100',
    },
    {
      title: 'Total Quantity',
      value: openOrders.reduce((sum, order) => sum + order.quantity, 0),
      icon: Package,
      color: 'text-green-600',
      bgColor: 'bg-green-100',
    },
    {
      title: 'Production Lines',
      value: 3,
      icon: Factory,
      color: 'text-purple-600',
      bgColor: 'bg-purple-100',
    },
    {
      title: 'System Status',
      value: healthStatus?.autoCountConnected ? 'Connected' : 'Disconnected',
      icon: CheckCircle,
      color: healthStatus?.autoCountConnected ? 'text-green-600' : 'text-red-600',
      bgColor: healthStatus?.autoCountConnected ? 'bg-green-100' : 'bg-red-100',
    },
  ];

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-muted-foreground">Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Welcome to Lemon Co Production System</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <Card key={stat.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  {stat.title}
                </CardTitle>
                <div className={`rounded-full p-2 ${stat.bgColor}`}>
                  <Icon className={`h-4 w-4 ${stat.color}`} />
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stat.value}</div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Recent Orders */}
      <Card>
        <CardHeader>
          <CardTitle>Open Assembly Orders</CardTitle>
          <CardDescription>
            Recent production orders awaiting completion
          </CardDescription>
        </CardHeader>
        <CardContent>
          {openOrders.length === 0 ? (
            <p className="text-center text-muted-foreground py-8">
              No open orders
            </p>
          ) : (
            <div className="space-y-4">
              {openOrders.slice(0, 5).map((order) => (
                <div
                  key={order.docNo}
                  className="flex items-center justify-between border-b pb-4 last:border-0"
                >
                  <div>
                    <p className="font-medium">{order.docNo}</p>
                    <p className="text-sm text-muted-foreground">
                      {order.itemDescription}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="font-medium">{order.quantity} units</p>
                    <p className="text-sm text-muted-foreground">
                      {new Date(order.productionDate).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
          <CardDescription>Common tasks and shortcuts</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <a
              href="/dashboard/production/create"
              className="flex flex-col items-center justify-center rounded-lg border p-6 hover:bg-accent transition-colors"
            >
              <Factory className="h-8 w-8 mb-2 text-primary" />
              <span className="font-medium">Create Assembly Order</span>
            </a>
            <a
              href="/dashboard/labels"
              className="flex flex-col items-center justify-center rounded-lg border p-6 hover:bg-accent transition-colors"
            >
              <Package className="h-8 w-8 mb-2 text-primary" />
              <span className="font-medium">Print Labels</span>
            </a>
            <a
              href="/dashboard/bom"
              className="flex flex-col items-center justify-center rounded-lg border p-6 hover:bg-accent transition-colors"
            >
              <CheckCircle className="h-8 w-8 mb-2 text-primary" />
              <span className="font-medium">Manage BOMs</span>
            </a>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

