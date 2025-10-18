// React Web - SignalR Integration Example
// This is a complete example showing how to use SignalR in React

import React, { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

// ==========================================
// SignalR Service
// ==========================================

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Map<string, Function[]> = new Map();

  async connect(accessToken: string): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5006/hubs/notifications', {
        accessTokenFactory: () => accessToken,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();

    try {
      await this.connection.start();
      console.log('✅ SignalR Connected');
    } catch (error) {
      console.error('❌ SignalR Connection Error:', error);
      throw error;
    }
  }

  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Booking notifications
    this.connection.on('ReceiveBookingCreated', (notification) => {
      this.emit('bookingCreated', notification);
    });

    this.connection.on('ReceiveBookingConfirmed', (notification) => {
      this.emit('bookingConfirmed', notification);
    });

    this.connection.on('ReceiveBookingCancelled', (notification) => {
      this.emit('bookingCancelled', notification);
    });

    // Payment notifications
    this.connection.on('ReceivePaymentCompleted', (notification) => {
      this.emit('paymentCompleted', notification);
    });

    this.connection.on('ReceivePaymentFailed', (notification) => {
      this.emit('paymentFailed', notification);
    });

    // General notifications
    this.connection.on('ReceiveNotification', (notification) => {
      this.emit('notification', notification);
    });
  }

  // Event emitter pattern for React components
  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(callback);
  }

  off(event: string, callback: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(callback);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data: any): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      listeners.forEach(callback => callback(data));
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.listeners.clear();
    }
  }
}

export const signalRService = new SignalRService();

// ==========================================
// React Hook for SignalR
// ==========================================

export function useSignalR() {
  const [isConnected, setIsConnected] = useState(false);
  const [notifications, setNotifications] = useState<any[]>([]);

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    
    if (token) {
      signalRService.connect(token)
        .then(() => setIsConnected(true))
        .catch(() => setIsConnected(false));

      // Listen for notifications
      const handleNotification = (notification: any) => {
        setNotifications(prev => [notification, ...prev]);
      };

      signalRService.on('notification', handleNotification);
      signalRService.on('bookingCreated', handleNotification);
      signalRService.on('bookingConfirmed', handleNotification);
      signalRService.on('paymentCompleted', handleNotification);

      return () => {
        signalRService.off('notification', handleNotification);
        signalRService.disconnect();
      };
    }
  }, []);

  return { isConnected, notifications };
}

// ==========================================
// React Component Example
// ==========================================

export function NotificationBell() {
  const { isConnected, notifications } = useSignalR();
  const [showDropdown, setShowDropdown] = useState(false);

  return (
    <div className="notification-bell">
      <button onClick={() => setShowDropdown(!showDropdown)}>
        🔔 {notifications.length > 0 && <span className="badge">{notifications.length}</span>}
        {isConnected && <span className="status-indicator">🟢</span>}
      </button>

      {showDropdown && (
        <div className="notification-dropdown">
          <h3>Notifications</h3>
          {notifications.length === 0 ? (
            <p>No new notifications</p>
          ) : (
            notifications.map((notif, index) => (
              <div key={index} className="notification-item">
                <strong>{notif.title || 'Notification'}</strong>
                <p>{notif.message}</p>
                <small>{new Date(notif.timestamp).toLocaleString()}</small>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}

// ==========================================
// Booking Page with Real-Time Updates
// ==========================================

export function BookingPage() {
  const [bookingStatus, setBookingStatus] = useState('Ready to book');
  const [bookingId, setBookingId] = useState<string | null>(null);

  useEffect(() => {
    // Listen for booking updates
    signalRService.on('bookingCreated', (notification: any) => {
      setBookingStatus('Booking created! Processing payment...');
      setBookingId(notification.bookingId);
    });

    signalRService.on('paymentCompleted', (notification: any) => {
      setBookingStatus('Payment successful! Confirming booking...');
    });

    signalRService.on('bookingConfirmed', (notification: any) => {
      setBookingStatus('Booking confirmed! ✅');
      // Redirect or show success page
      setTimeout(() => {
        window.location.href = `/bookings/${notification.bookingId}`;
      }, 2000);
    });

    return () => {
      // Cleanup listeners
    };
  }, []);

  const handleCreateBooking = async () => {
    setBookingStatus('Creating booking...');
    
    const response = await fetch('/api/v1/bookings', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({
        packageId: 'pkg123',
        travelDate: '2025-12-01',
        numberOfTravelers: 2,
        idempotencyKey: crypto.randomUUID()
      })
    });
    
    // SignalR will handle the rest!
    // No need to poll for status
  };

  return (
    <div>
      <h1>Create Booking</h1>
      <p>Status: {bookingStatus}</p>
      <button onClick={handleCreateBooking}>Book Now</button>
      {bookingId && <p>Booking ID: {bookingId}</p>}
    </div>
  );
}

// ==========================================
// Admin Dashboard with Live Updates
// ==========================================

export function AdminDashboard() {
  const [stats, setStats] = useState({
    bookings: 0,
    revenue: 0
  });

  useEffect(() => {
    // Listen for updates
    signalRService.on('bookingCreated', (notification: any) => {
      setStats(prev => ({
        ...prev,
        bookings: prev.bookings + 1
      }));
      
      // Show toast notification
      showToast('New booking created!', 'info');
    });

    signalRService.on('paymentCompleted', (notification: any) => {
      setStats(prev => ({
        ...prev,
        revenue: prev.revenue + notification.amount
      }));
      
      // Show toast notification
      showToast(`Payment received: $${notification.amount}`, 'success');
    });
  }, []);

  return (
    <div>
      <h1>Live Dashboard</h1>
      <div className="stat-card">
        <h3>Total Bookings</h3>
        <p>{stats.bookings}</p>
      </div>
      <div className="stat-card">
        <h3>Total Revenue</h3>
        <p>${stats.revenue}</p>
      </div>
    </div>
  );
}

function showToast(message: string, type: string) {
  // Implement using your toast library
  console.log(`[${type}] ${message}`);
}

// ==========================================
// Export
// ==========================================

export default signalRService;

