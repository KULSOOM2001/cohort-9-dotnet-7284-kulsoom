import { Link } from 'react-router-dom';
import { useAuth } from '../context/useAuth';

export default function Navbar() {
  const { user, logout } = useAuth();

  return (
    <nav className="navbar">
      <span>Welcome, {user?.fullName}</span>
      <Link to="/dashboard">Dashboard</Link>
      <Link to="/tasks">Tasks</Link>
      <Link to="/profile">Profile</Link>
      <button type="button" onClick={logout}>
        Logout
      </button>
    </nav>
  );
}