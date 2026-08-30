import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';

const initialFormData = {
  title: '',
  description: '',
  status: 0,
  priority: 1,
  category: '',
  dueDate: '',
  assignedToUserId: '',
};

export default function TaskForm() {
  const navigate = useNavigate();
  const { id } = useParams();

  const isEditMode = Boolean(id);

  const [formData, setFormData] = useState(initialFormData);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(isEditMode);
  const [loadFailed, setLoadFailed] = useState(false);

  useEffect(() => {
    if (!isEditMode) {
      return;
    }

    let isActive = true;
    const controller = new AbortController();

    const loadTask = async () => {
      try {
        const res = await axiosInstance.get(`/Tasks/${id}`, {
          signal: controller.signal,
        });

        if (!isActive) return;

        const task = res.data;

        if (!task || typeof task !== 'object' || Array.isArray(task)) {
          throw new Error('Invalid task response.');
        }

        setFormData({
          title: task.title || '',
          description: task.description || '',
          status:
            task.status === 'InProgress' || task.status === 1
              ? 1
              : task.status === 'Completed' || task.status === 2
                ? 2
                : 0,
          priority:
            task.priority === 'Low' || task.priority === 0
              ? 0
              : task.priority === 'High' || task.priority === 2
                ? 2
                : 1,
          category: task.category || '',
          dueDate: task.dueDate
            ? task.dueDate.split('T')[0]
            : '',
          assignedToUserId: task.assignedToUserId || '',
        });

        setError('');
        setLoadFailed(false);
      } catch (err) {
        if (!isActive) return;

        if (
          err?.name === 'CanceledError' ||
          err?.code === 'ERR_CANCELED'
        ) {
          return;
        }

        setLoadFailed(true);
        setError(
          err?.response?.data?.message ||
            err?.message ||
            'Failed to load task.'
        );
      } finally {
        if (isActive) {
          setLoading(false);
        }
      }
    };

    loadTask();

    return () => {
      isActive = false;
      controller.abort();
    };
  }, [id, isEditMode]);

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

    const payload = {
      title: formData.title,
      description: formData.description || null,
      status: Number(formData.status),
      priority: Number(formData.priority),
      category: formData.category || null,
      dueDate: formData.dueDate || null,
      assignedToUserId: Number(formData.assignedToUserId),
    };

    try {
      if (isEditMode) {
        await axiosInstance.put(`/Tasks/${id}`, payload);
      } else {
        await axiosInstance.post('/Tasks', payload);
      }

      navigate(isEditMode ? `/tasks/${id}` : '/tasks');
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          (isEditMode
            ? 'Failed to update task.'
            : 'Failed to create task.')
      );
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditMode && !formData.title) {
    return (
      <div className="page">
        <p>Loading task...</p>
      </div>
    );
  }

  if (isEditMode && loadFailed) {
    return (
      <div className="page">
        <p className="auth-error">{error}</p>

        <button
          type="button"
          onClick={() => navigate('/tasks')}
        >
          Back to Tasks
        </button>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="task-form">
        <h1>{isEditMode ? 'Edit Task' : 'New Task'}</h1>

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
              disabled={loading}
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
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="status">Status</label>
            <select
              id="status"
              name="status"
              value={formData.status}
              onChange={handleChange}
              disabled={loading}
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
              disabled={loading}
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
              disabled={loading}
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
              disabled={loading}
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
              disabled={loading}
            />
          </div>

          <div className="form-actions">
            <button type="submit" disabled={loading}>
              {loading
                ? isEditMode
                  ? 'Updating...'
                  : 'Creating...'
                : isEditMode
                  ? 'Update Task'
                  : 'Create Task'}
            </button>

            <button
              type="button"
              onClick={() =>
                navigate(isEditMode ? `/tasks/${id}` : '/tasks')
              }
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