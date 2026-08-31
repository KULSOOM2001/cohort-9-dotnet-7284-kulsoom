import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/useAuth';
import Navbar from '../components/Navbar';

export default function Profile() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!user) {
    return (
      <div className="page">
        <Navbar />

        <h1>Profile</h1>

        <p className="auth-error">
          User information is not available.
        </p>

        <button
          type="button"
          onClick={() => navigate('/login')}
        >
          Go to Login
        </button>
      </div>
    );
  }

  return (
    <div className="page">
      <Navbar />

      <div className="profile-card">
        <h1>My Profile</h1>

        <div className="profile-details">
          <div className="detail-row">
            <span className="detail-label">Full Name</span>
            <span>
              {user.fullName || user.name || 'N/A'}
            </span>
          </div>

          <div className="detail-row">
            <span className="detail-label">Email</span>
            <span>{user.email || 'N/A'}</span>
          </div>

          <div className="detail-row">
            <span className="detail-label">Role</span>
            <span>{user.role || 'User'}</span>
          </div>
        </div>

        <div className="profile-actions">
          <button
            type="button"
            onClick={handleLogout}
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  );
}

