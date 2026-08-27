import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axiosInstance from '../api/axiosInstance';

export default function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    axiosInstance
      .get('/Tasks')
      .then((res) => {
        if (Array.isArray(res.data)) {
          setTasks(res.data);
        } else {
           setError('Invalid task data received.');
        }
      })
      .catch(() => setError('Failed to load tasks.'));
  }, []);

  return (
    <div className="page">
      <h1>Tasks</h1>

      <Link to="/tasks/new">
        <button>New Task</button>
      </Link>

      {error && <p className="auth-error">{error}</p>}

      {tasks.length === 0 && !error && <p>No tasks found.</p>}

      {tasks.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Category</th>
              <th>Due Date</th>
              <th>Assigned To</th>
            </tr>
          </thead>

          <tbody>
            {tasks.map((task) => (
              <tr
                key={task.id}
              >
                <td>
                  <Link to={`/tasks/${task.id}`}>
                    {task.title}
                  </Link>
                </td>
                <td>{task.status}</td>
                <td>{task.priority}</td>
                <td>{task.category || '-'}</td>
                <td>
                  {task.dueDate
                    ? new Date(task.dueDate).toLocaleDateString()
                    : '-'}
                </td>
                <td>{task.assignedToUserName || '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}