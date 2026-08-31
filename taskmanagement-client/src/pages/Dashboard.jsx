import { useEffect, useState } from 'react';
import axiosInstance from '../api/axiosInstance';
import Navbar from '../components/Navbar';

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [error, setError] = useState('');

useEffect(() => {
  axiosInstance.get('/Dashboard')
    .then((res) => {
      const dashboard = res.data;

      const isValid =
        dashboard &&
        typeof dashboard.pendingCount === 'number' &&
        typeof dashboard.inProgressCount === 'number' &&
        typeof dashboard.completedCount === 'number' &&
        typeof dashboard.totalCount === 'number';

      if (isValid) {
        setData(dashboard);
      } else {
        setError('Invalid dashboard data received.');
      }
    })
    .catch(() => setError('Failed to load dashboard.'));
}, []);

  return (
    <div className="page">
      <Navbar />
      <h1>Dashboard</h1>

      {error && <p className="auth-error">{error}</p>}

      {data && (
        <div className="cards">
          <div className="card">
            Pending
            <br />
            <strong>{data.pendingCount}</strong>
          </div>

          <div className="card">
            In Progress
            <br />
            <strong>{data.inProgressCount}</strong>
          </div>

          <div className="card">
            Completed
            <br />
            <strong>{data.completedCount}</strong>
          </div>

          <div className="card">
            Total
            <br />
            <strong>{data.totalCount}</strong>
          </div>
        </div>
      )}
    </div>
  );
}