import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';
import { useAuth } from '../context/useAuth';

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [error, setError] = useState('');
  const { user, logout } = useAuth();

  useEffect(() => {
    axiosInstance.get('/Dashboard')
      .then((res) => setData(res.data))
      .catch(() => setError('Failed to load dashboard.'));
  }, []);

  return (
    <div className="page">
      <nav className="navbar">
        <span>Welcome, {user?.fullName}</span>
        <Link to="/tasks">Tasks</Link>
        <Link to="/profile">Profile</Link>
        <button onClick={logout}>Logout</button>
      </nav>

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