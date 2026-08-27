import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';

export default function TaskForm() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    status: 0,
    priority: 1,
    category: '',
    dueDate: '',
    assignedToUserId: '',
  });

  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!formData.title.trim()) {
      setError('Title is required.');
      return;
    }

    if (!formData.assignedToUserId) {
      setError('Assigned To User ID is required.');
      return;
    }

    setLoading(true);

    try {
      await axiosInstance.post('/Tasks', {
        title: formData.title,
        description: formData.description || null,
        status: Number(formData.status),
        priority: Number(formData.priority),
        category: formData.category || null,
        dueDate: formData.dueDate || null,
        assignedToUserId: Number(formData.assignedToUserId),
      });

      navigate('/tasks');
    } catch (err) {
      setError(
        err.response?.data?.message || 'Failed to create task.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <div className="task-form">
        <h1>New Task</h1>

        {error && <p className="auth-error">{error}</p>}

        <form onSubmit={handleSubmit}>

          <div className="form-group">
            <label htmlFor="title">Title</label>
            <input
              id="title"
              name="title"
              type="text"
              value={formData.title}
              onChange={handleChange}
              placeholder="Enter task title"
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              placeholder="Enter task description"
            />
          </div>

          <div className="form-group">
            <label htmlFor="status">Status</label>
            <select
              id="status"
              name="status"
              value={formData.status}
              onChange={handleChange}
            >
              <option value="0">Pending</option>
              <option value="1">In Progress</option>
              <option value="2">Completed</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="priority">Priority</label>
            <select
              id="priority"
              name="priority"
              value={formData.priority}
              onChange={handleChange}
            >
              <option value="0">Low</option>
              <option value="1">Medium</option>
              <option value="2">High</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="category">Category</label>
            <input
              id="category"
              name="category"
              type="text"
              value={formData.category}
              onChange={handleChange}
              placeholder="Enter category"
            />
          </div>

          <div className="form-group">
            <label htmlFor="dueDate">Due Date</label>
            <input
              id="dueDate"
              name="dueDate"
              type="date"
              value={formData.dueDate}
              onChange={handleChange}
            />
          </div>

          <div className="form-group">
            <label htmlFor="assignedToUserId">
              Assigned To User ID
            </label>
            <input
              id="assignedToUserId"
              name="assignedToUserId"
              type="number"
              value={formData.assignedToUserId}
              onChange={handleChange}
              placeholder="Enter user ID"
            />
          </div>

          <div className="form-actions">
            <button type="submit" disabled={loading}>
              {loading ? 'Creating...' : 'Create Task'}
            </button>

            <button
              type="button"
              onClick={() => navigate('/tasks')}
              disabled={loading}
            >
              Cancel
            </button>
          </div>

        </form>
      </div>
    </div>
  );
}