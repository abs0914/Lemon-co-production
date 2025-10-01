'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export default function LoginPage() {
  const router = useRouter();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      // Mock authentication - replace with actual API call
      if (username === 'admin' && password === 'admin123') {
        const user = {
          id: 1,
          username: 'admin',
          fullName: 'System Administrator',
          role: 'Admin',
        };
        localStorage.setItem('user', JSON.stringify(user));
        router.push('/dashboard');
      } else if (username === 'production' && password === 'prod123') {
        const user = {
          id: 2,
          username: 'production',
          fullName: 'Production User',
          role: 'Production',
        };
        localStorage.setItem('user', JSON.stringify(user));
        router.push('/dashboard');
      } else if (username === 'warehouse' && password === 'wh123') {
        const user = {
          id: 3,
          username: 'warehouse',
          fullName: 'Warehouse User',
          role: 'Warehouse',
        };
        localStorage.setItem('user', JSON.stringify(user));
        router.push('/dashboard');
      } else {
        setError('Invalid username or password');
      }
    } catch (err) {
      setError('Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-3xl font-bold text-primary">Lemon Co</CardTitle>
          <CardDescription>Production Workflow System</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleLogin} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="username">Username</Label>
              <Input
                id="username"
                type="text"
                placeholder="Enter username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                placeholder="Enter password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            {error && (
              <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
                {error}
              </div>
            )}
            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? 'Logging in...' : 'Login'}
            </Button>
          </form>
          <div className="mt-6 text-sm text-muted-foreground">
            <p className="font-semibold">Demo Credentials:</p>
            <p>Admin: admin / admin123</p>
            <p>Production: production / prod123</p>
            <p>Warehouse: warehouse / wh123</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

