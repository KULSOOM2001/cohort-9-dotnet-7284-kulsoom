import { motion } from 'framer-motion';
import { Mail, ShieldCheck, UserRound, LogOut, ArrowLeft } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/useAuth';
import Navbar from '../components/Navbar';

export default function Profile() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const name = user?.fullName || user?.name || 'User';
  const initials = name.split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join('').toUpperCase() || 'U';

  const handleLogout = () => { logout(); navigate('/login'); };

  if (!user) return <div className="page"><Navbar /><div className="empty-state"><h2>Session not found</h2><p>Please sign in again to continue.</p><button type="button" onClick={() => navigate('/login')}>Go to Login</button></div></div>;

  return (
    <div className="page">
      <Navbar />
      <motion.main className="profile-content" initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}>
        <button className="back-button" type="button" onClick={() => navigate('/dashboard')}><ArrowLeft size={16} /> Dashboard</button>
        <section className="profile-hero">
          <div className="profile-avatar">{initials}</div>
          <div><span className="eyebrow">ACCOUNT</span><h1>{name}</h1><p>Your personal TaskFlow profile</p></div>
          <span className="role-badge"><ShieldCheck size={15} /> {user.role || 'User'}</span>
        </section>
        <section className="profile-card">
          <h2>Account information</h2><p className="section-subtitle">Your authenticated account details.</p>
          <div className="profile-details">
            <div className="info-item"><span className="info-icon"><UserRound size={18} /></span><div><small>Full Name</small><strong>{name}</strong></div></div>
            <div className="info-item"><span className="info-icon"><Mail size={18} /></span><div><small>Email Address</small><strong>{user.email || 'N/A'}</strong></div></div>
            <div className="info-item"><span className="info-icon"><ShieldCheck size={18} /></span><div><small>Role</small><strong>{user.role || 'User'}</strong></div></div>
          </div>
          <div className="profile-actions"><button className="danger-button" type="button" onClick={handleLogout}><LogOut size={17} /> Logout</button></div>
        </section>
      </motion.main>
    </div>
  );
}
