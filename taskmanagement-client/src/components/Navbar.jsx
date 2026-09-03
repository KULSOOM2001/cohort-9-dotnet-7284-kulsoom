import { NavLink, useNavigate } from 'react-router-dom';
import { LayoutDashboard, ListTodo, LogOut, UserCircle, CheckSquare } from 'lucide-react';
import { useAuth } from '../context/useAuth';

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const name = user?.fullName || user?.name || 'User';
  const initials = name.split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join('').toUpperCase() || 'U';

  const handleLogout = () => { logout(); navigate('/login'); };

  return (
    <nav className="navbar">
      <button className="brand" onClick={() => navigate('/dashboard')} type="button" aria-label="Go to dashboard">
        <span className="brand-mark"><CheckSquare size={18} /></span>
        <span className="brand-copy"><strong>TaskFlow</strong><small>WORKSPACE</small></span>
      </button>
      <div className="nav-links">
        <NavLink to="/dashboard" className={({ isActive }) => isActive ? 'active' : ''}><LayoutDashboard size={16} /><span>Dashboard</span></NavLink>
        <NavLink to="/tasks" className={({ isActive }) => isActive ? 'active' : ''}><ListTodo size={16} /><span>Tasks</span></NavLink>
        <NavLink to="/profile" className={({ isActive }) => isActive ? 'active' : ''}><UserCircle size={16} /><span>Profile</span></NavLink>
      </div>
      <div className="nav-user">
        <div className="avatar">{initials}</div>
        <div className="nav-user-info"><strong>{name}</strong><span>{user?.role || 'User'}</span></div>
        <button className="logout-button" type="button" onClick={handleLogout} aria-label="Logout" title="Logout"><LogOut size={16} /></button>
      </div>
    </nav>
  );
}
