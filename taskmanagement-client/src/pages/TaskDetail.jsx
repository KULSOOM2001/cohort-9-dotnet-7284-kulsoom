import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { ArrowLeft, CalendarDays, Edit3, Trash2, UserRound, Tag, Clock3 } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';
import Navbar from '../components/Navbar';

const statusLabel = (value) => value === 'InProgress' ? 'In Progress' : value || 'Unknown';
const priorityLabel = (value) => value || 'Unknown';

export default function TaskDetail() {
  const { id } = useParams(); const navigate = useNavigate(); const [task, setTask] = useState(null); const [error, setError] = useState(''); const [loading, setLoading] = useState(true); const [deleting, setDeleting] = useState(false);
  useEffect(() => { let active = true; axiosInstance.get(`/Tasks/${id}`).then((res) => active && setTask(res.data)).catch(() => active && setError('Failed to load task.')).finally(() => active && setLoading(false)); return () => { active = false; }; }, [id]);
  const handleDelete = async () => { if (!window.confirm('Are you sure you want to delete this task?')) return; setDeleting(true); setError(''); try { await axiosInstance.delete(`/Tasks/${id}`); navigate('/tasks'); } catch (err) { setError(err.response?.data?.message || 'Failed to delete task.'); setDeleting(false); } };
  if (loading) return <div className="page"><Navbar /><div className="empty-state"><h2>Loading task…</h2><p>Fetching the latest task details.</p></div></div>;
  if (error && !task) return <div className="page"><Navbar /><div className="empty-state"><h2>Task unavailable</h2><p className="auth-error">{error}</p><Link className="new-task-button" to="/tasks">Back to Tasks</Link></div></div>;
  if (!task) return <div className="page"><Navbar /><div className="empty-state"><h2>Task not found</h2><Link className="new-task-button" to="/tasks">Back to Tasks</Link></div></div>;
  return <div className="page"><Navbar /><motion.main className="task-detail" initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}><div className="task-detail-header"><button className="back-button" type="button" onClick={() => navigate('/tasks')}><ArrowLeft size={16} /> Back to Tasks</button><div className="task-detail-actions"><Link className="edit-link" to={`/tasks/${id}/edit`}><Edit3 size={15} /> Edit</Link><button className="icon-danger" type="button" onClick={handleDelete} disabled={deleting}><Trash2 size={15} /> {deleting ? 'Deleting…' : 'Delete'}</button></div></div><section className="task-detail-card"><div className="task-detail-title"><div><span className="eyebrow">TASK #{id}</span><h1>{task.title}</h1><p>{task.description || 'No description provided.'}</p></div><div className="detail-badges"><span className={`status-badge ${statusLabel(task.status).toLowerCase().replace(/\s+/g,'-')}`}>{statusLabel(task.status)}</span><span className={`priority-badge ${priorityLabel(task.priority).toLowerCase()}`}>{priorityLabel(task.priority)}</span></div></div>{error && <div className="alert error">{error}</div>}<div className="detail-grid"><div className="detail-tile"><span><Tag size={17}/></span><small>Category</small><strong>{task.category || 'Uncategorized'}</strong></div><div className="detail-tile"><span><CalendarDays size={17}/></span><small>Due date</small><strong>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No deadline'}</strong></div><div className="detail-tile"><span><UserRound size={17}/></span><small>Assigned to</small><strong>{task.assignedToUserName || '-'}</strong></div><div className="detail-tile"><span><Clock3 size={17}/></span><small>Created</small><strong>{task.createdAt ? new Date(task.createdAt).toLocaleDateString() : '-'}</strong></div></div></section></motion.main></div>;
}
