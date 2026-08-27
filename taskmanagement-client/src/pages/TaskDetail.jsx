import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';

export default function TaskDetail() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [task, setTask] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    axiosInstance
      .get(`/Tasks/${id}`)
      .then((res) => setTask(res.data))
      .catch(() => setError('Failed to load task.'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleDelete = async () => {
    const confirmed = window.confirm(
      'Are you sure you want to delete this task?'
    );

    if (!confirmed) return;

    setDeleting(true);
    setError('');

    try {
      await axiosInstance.delete(`/Tasks/${id}`);
      navigate('/tasks');
    } catch (err) {
      setError(
        err.response?.data?.message ||
          'Failed to delete task.'
      );
      setDeleting(false);
    }
  };

  if (loading) {
    return (
      <div className="page">
        <p>Loading task...</p>
      </div>
    );
  }

  if (error && !task) {
    return (
      <div className="page">
        <p className="auth-error">{error}</p>
        <Link to="/tasks">Back to Tasks</Link>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="page">
        <p>Task not found.</p>
        <Link to="/tasks">Back to Tasks</Link>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="task-detail">
        <div className="task-detail-header">
          <h1>{task.title}</h1>

          <Link to="/tasks">
            Back to Tasks
          </Link>
        </div>

        {error && <p className="auth-error">{error}</p>}

        <div className="task-detail-card">
          <div className="detail-row">
            <strong>Description</strong>
            <p>{task.description || '-'}</p>
          </div>

          <div className="detail-row">
            <strong>Status</strong>
            <p>{task.status}</p>
          </div>

          <div className="detail-row">
            <strong>Priority</strong>
            <p>{task.priority}</p>
          </div>

          <div className="detail-row">
            <strong>Category</strong>
            <p>{task.category || '-'}</p>
          </div>

          <div className="detail-row">
            <strong>Due Date</strong>
            <p>
              {task.dueDate
                ? new Date(task.dueDate).toLocaleDateString()
                : '-'}
            </p>
          </div>

          <div className="detail-row">
            <strong>Assigned To</strong>
            <p>
              {task.assignedToUserName || '-'}
            </p>
          </div>

          <div className="detail-row">
            <strong>Created At</strong>
            <p>
              {task.createdAt
                ? new Date(task.createdAt).toLocaleDateString()
                : '-'}
            </p>
          </div>

          <div className="task-actions">
            <Link to={`/tasks/${id}/edit`}>
              <button type="button">
                Edit Task
              </button>
            </Link>

            <button
              type="button"
              onClick={handleDelete}
              disabled={deleting}
            >
              {deleting ? 'Deleting...' : 'Delete Task'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}