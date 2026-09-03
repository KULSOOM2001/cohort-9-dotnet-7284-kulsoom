import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { ArrowUpRight, CheckCircle2, CircleDot, Clock3, ListChecks, Plus, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';
import Navbar from '../components/Navbar';
import { useAuth } from '../context/useAuth';

const stats = [
  { key: 'pendingCount', label: 'Pending', icon: Clock3, tone: 'amber', helper: 'Need your attention' },
  { key: 'inProgressCount', label: 'In progress', icon: CircleDot, tone: 'blue', helper: 'Currently underway' },
  { key: 'completedCount', label: 'Completed', icon: CheckCircle2, tone: 'green', helper: 'Successfully finished' },
  { key: 'totalCount', label: 'Total tasks', icon: ListChecks, tone: 'purple', helper: 'Across your workspace' },
];

export default function Dashboard() {
  const { user } = useAuth();
  const [data, setData] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    axiosInstance.get('/Dashboard')
      .then((res) => {
        const dashboard = res.data;
        const valid = dashboard && ['pendingCount', 'inProgressCount', 'completedCount', 'totalCount']
          .every((key) => typeof dashboard[key] === 'number');
        if (!active) return;
        if (valid) setData(dashboard); else setError('Invalid dashboard data received.');
      })
      .catch(() => active && setError('Failed to load dashboard.'));
    return () => { active = false; };
  }, []);

  const completed = data?.completedCount || 0;
  const pending = data?.pendingCount || 0;
  const inProgress = data?.inProgressCount || 0;
  const total = data?.totalCount || 0;
  const completion = total ? Math.round((completed / total) * 100) : 0;

  return (
    <div className="page dashboard-page">
      <Navbar />
      <motion.main
        className="dashboard-content"
        initial={{ opacity: 0, y: 14 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4, ease: 'easeOut' }}
      >
        <section className="dashboard-hero">
          <div className="hero-copy">
            <span className="eyebrow"><Sparkles size={13} /> WORKSPACE OVERVIEW</span>
            <h1>Good to see you, {user?.fullName?.split(' ')[0] || 'there'}.</h1>
            <p>Stay focused, track progress, and keep your team moving forward.</p>
            <div className="hero-actions">
              <Link className="hero-primary" to="/tasks"><Plus size={17} /> Create a task</Link>
              <Link className="hero-secondary" to="/tasks">View all tasks <ArrowUpRight size={16} /></Link>
            </div>
          </div>
          <div className="hero-orbit" aria-hidden="true">
            <div className="orbit orbit-one" /><div className="orbit orbit-two" /><div className="orbit-core"><CheckCircle2 size={30} /></div>
          </div>
        </section>

        {error && <div className="alert error">{error}</div>}

        {!data ? (
          <div className="stats-grid dashboard-stats">{stats.map((item) => <div className="stat-card skeleton" key={item.key} />)}</div>
        ) : (
          <>
            <section className="dashboard-section-heading">
              <div><span className="section-kicker">AT A GLANCE</span><h2>Your productivity snapshot</h2></div>
              <span className="updated-note">Live workspace data</span>
            </section>

            <div className="stats-grid dashboard-stats">
              {stats.map(({ key, label, icon: Icon, tone, helper }, index) => (
                <motion.div
                  className={`stat-card ${tone}`}
                  key={key}
                  initial={{ opacity: 0, y: 18 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.07, duration: 0.35 }}
                  whileHover={{ y: -5 }}
                >
                  <div className="stat-top"><span className="stat-icon"><Icon size={19} /></span><span className="stat-label">{label}</span></div>
                  <strong>{data[key]}</strong>
                  <span className="stat-caption">{helper}</span>
                </motion.div>
              ))}
            </div>

            <div className="dashboard-grid">
              <section className="completion-card">
                <div className="card-heading"><div><span className="section-kicker">COMPLETION</span><h2>Overall progress</h2><p>Keep momentum going across your tasks.</p></div></div>
                <div className="completion-body">
                  <div className="progress-ring" style={{ '--progress': `${completion * 3.6}deg` }}>
                    <div><strong>{completion}%</strong><span>complete</span></div>
                  </div>
                  <div className="completion-copy">
                    <div className="completion-number"><strong>{completed}</strong><span>of {total} tasks completed</span></div>
                    <div className="progress-track"><motion.div className="progress-bar" initial={{ width: 0 }} animate={{ width: `${completion}%` }} transition={{ duration: 0.9, ease: 'easeOut' }} /></div>
                    <Link to="/tasks" className="text-link">Review your tasks <ArrowUpRight size={15} /></Link>
                  </div>
                </div>
              </section>

              <section className="workload-card">
                <div className="card-heading"><div><span className="section-kicker">WORKLOAD</span><h2>Task distribution</h2></div></div>
                <div className="workload-list">
                  <WorkloadRow label="Pending" value={pending} total={total} tone="amber" />
                  <WorkloadRow label="In progress" value={inProgress} total={total} tone="blue" />
                  <WorkloadRow label="Completed" value={completed} total={total} tone="green" />
                </div>
              </section>
            </div>
          </>
        )}
      </motion.main>
    </div>
  );
}

function WorkloadRow({ label, value, total, tone }) {
  const percent = total ? Math.round((value / total) * 100) : 0;
  return <div className="workload-row"><div className="workload-meta"><span>{label}</span><strong>{value}<small> / {total}</small></strong></div><div className={`workload-track ${tone}`}><motion.div initial={{ width: 0 }} animate={{ width: `${percent}%` }} transition={{ duration: 0.7 }} /></div><span className="workload-percent">{percent}%</span></div>;
}
