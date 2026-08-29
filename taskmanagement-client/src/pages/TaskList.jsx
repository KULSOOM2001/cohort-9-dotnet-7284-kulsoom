import { useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import axiosInstance from '../api/axiosInstance';
const normalizeStatus = (status) => {
  if (typeof status === 'number') {
    return ['Pending', 'In Progress', 'Completed'][status] || 'Unknown';
  }
  const value = String(status || '').toLowerCase();
  if (value === 'inprogress' || value === 'in progress') {
    return 'In Progress';
  }
  if (value === 'completed') {
    return 'Completed';
  }
  if (value === 'pending') {
    return 'Pending';
  }
  return status || 'Unknown';
};
const normalizePriority = (priority) => {
  if (typeof priority === 'number') {
    return ['Low', 'Medium', 'High'][priority] || 'Unknown';
  }
  const value = String(priority || '').toLowerCase();
  if (value === 'low') return 'Low';
  if (value === 'medium') return 'Medium';
  if (value === 'high') return 'High';
  return priority || 'Unknown';
};
export default function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [realtimeStatus, setRealtimeStatus] = useState('Connecting...');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [dueDateFilter, setDueDateFilter] = useState('');
  const [sortBy, setSortBy] = useState('dueDateAsc');
  const requestIdRef = useRef(0);
  const loadTasks = async () => {
    const requestId = ++requestIdRef.current;
    setLoading(true);
    try {
      const res = await axiosInstance.get('/Tasks');
      if (requestId !== requestIdRef.current) {
        return;
      }
      if (Array.isArray(res.data)) {
        const validTasks = res.data.filter(
          (task) => task !== null && typeof task === 'object'
        );
        setTasks(validTasks);
        setError('');
      } else {
        setError('Invalid task data received.');
      }
    } catch {
      if (requestId !== requestIdRef.current) {
        return;
      }
      setError('Failed to load tasks.');
    } finally {
      if (requestId === requestIdRef.current) {
        setLoading(false);
      }
    }
  };
  useEffect(() => {
    loadTasks();
  }, []);
  useEffect(() => {
    const hubBaseUrl =
      import.meta.env.VITE_API_BASE_URL ||
      'http://localhost:5025/api';
    const apiRoot = hubBaseUrl.replace(/\/api\/?$/, '');
    const connection = new HubConnectionBuilder()
      .withUrl(`${apiRoot}/hubs/tasks`, {
        accessTokenFactory: () => sessionStorage.getItem('token') || '',
      })
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();
    let disposed = false;
    let retryTimeoutId = null;
    let connectionStarting = false;
    const refreshTasks = () => {
      loadTasks();
    };
    connection.on('TaskCreated', refreshTasks);
    connection.on('TaskUpdated', refreshTasks);
    connection.on('TaskDeleted', refreshTasks);
    connection.onreconnecting(() => {
      if (!disposed) {
        setRealtimeStatus('Reconnecting...');
      }
    });
    connection.onreconnected(() => {
      if (!disposed) {
        setRealtimeStatus('Live');
      }
    });
    connection.onclose(() => {
      if (disposed) {
        return;
      }
      setRealtimeStatus('Disconnected');
      retryTimeoutId = setTimeout(() => {
        startConnection();
      }, 5000);
    });
    const startConnection = async (attempt = 0) => {
      if (
        disposed ||
        connectionStarting ||
        connection.state === 'Connected' ||
        connection.state === 'Connecting' ||
        connection.state === 'Reconnecting'
      ) {
        return;
      }
      connectionStarting = true;
      try {
        await connection.start();
        if (!disposed) {
          setRealtimeStatus('Live');
        }
      } catch {
        if (!disposed) {
          setRealtimeStatus('Unavailable');
          const delay = Math.min(1000 * 2 ** attempt, 10000);
          retryTimeoutId = setTimeout(() => {
            startConnection(Math.min(attempt + 1, 4));
          }, delay);
        }
      } finally {
        connectionStarting = false;
      }
    };
    startConnection();
    return () => {
      disposed = true;
      if (retryTimeoutId) {
        clearTimeout(retryTimeoutId);
      }
      connection.off('TaskCreated', refreshTasks);
      connection.off('TaskUpdated', refreshTasks);
      connection.off('TaskDeleted', refreshTasks);
      connection.stop();
    };
  }, []);
  const filteredTasks = useMemo(() => {
    const searchValue = search.trim().toLowerCase();
    const result = tasks.filter((task) => {
      const title = String(task.title || '').toLowerCase();
      const category = String(task.category || '').toLowerCase();
      const status = normalizeStatus(task.status);
      const priority = normalizePriority(task.priority);
      const matchesSearch =
        !searchValue ||
        title.includes(searchValue) ||
        category.includes(searchValue);
      const matchesStatus =
        !statusFilter || status === statusFilter;
      const matchesPriority =
        !priorityFilter || priority === priorityFilter;
      const taskDueDate = task.dueDate
        ? String(task.dueDate).split('T')[0]
        : '';
      const matchesDueDate =
        !dueDateFilter || taskDueDate === dueDateFilter;
      return (
        matchesSearch &&
        matchesStatus &&
        matchesPriority &&
        matchesDueDate
      );
    });
    return [...result].sort((a, b) => {
      if (sortBy === 'titleAsc') {
        return String(a.title || '').localeCompare(
          String(b.title || '')
        );
      }
      if (sortBy === 'titleDesc') {
        return String(b.title || '').localeCompare(
          String(a.title || '')
        );
      }
      if (sortBy === 'priorityHigh') {
        const priorityOrder = {
          High: 3,
          Medium: 2,
          Low: 1,
        };
        return (
          (priorityOrder[normalizePriority(b.priority)] || 0) -
          (priorityOrder[normalizePriority(a.priority)] || 0)
        );
      }
      if (sortBy === 'priorityLow') {
        const priorityOrder = {
          High: 3,
          Medium: 2,
          Low: 1,
        };
        return (
          (priorityOrder[normalizePriority(a.priority)] || 0) -
          (priorityOrder[normalizePriority(b.priority)] || 0)
        );
      }
      if (sortBy === 'dueDateDesc') {
        return (
          new Date(b.dueDate || '9999-12-31') -
          new Date(a.dueDate || '9999-12-31')
        );
      }
      return (
        new Date(a.dueDate || '9999-12-31') -
        new Date(b.dueDate || '9999-12-31')
      );
    });
  }, [
    tasks,
    search,
    statusFilter,
    priorityFilter,
    dueDateFilter,
    sortBy,
  ]);
  const clearFilters = () => {
    setSearch('');
    setStatusFilter('');
    setPriorityFilter('');
    setDueDateFilter('');
    setSortBy('dueDateAsc');
  };
  const hasFilters =
    search ||
    statusFilter ||
    priorityFilter ||
    dueDateFilter ||
    sortBy !== 'dueDateAsc';
  return (
    <div className="page">
      <div className="task-list-header">
        <div>
          <h1>Tasks</h1>
          <p>
            Showing {filteredTasks.length} of {tasks.length} tasks
          </p>
          <small>
            Real-time updates:{' '}
            <strong>{realtimeStatus}</strong>
          </small>
        </div>
        <Link className="new-task-button" to="/tasks/new">
          New Task
        </Link>
      </div>
      {error && <p className="auth-error">{error}</p>}
      {loading && <p>Loading tasks...</p>}
      {!loading && !error && (
        <>
          <div className="task-filters">
            <div className="form-group">
              <label htmlFor="task-search">Search</label>
              <input
                id="task-search"
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search title or category..."
              />
            </div>
            <div className="form-group">
              <label htmlFor="status-filter">Status</label>
              <select
                id="status-filter"
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="In Progress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
            <div className="form-group">
              <label htmlFor="priority-filter">Priority</label>
              <select
                id="priority-filter"
                value={priorityFilter}
                onChange={(e) => setPriorityFilter(e.target.value)}
              >
                <option value="">All Priorities</option>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>
            <div className="form-group">
              <label htmlFor="due-date-filter">Due Date</label>
              <input
                id="due-date-filter"
                type="date"
                value={dueDateFilter}
                onChange={(e) => setDueDateFilter(e.target.value)}
              />
            </div>
            <div className="form-group">
              <label htmlFor="sort-by">Sort By</label>
              <select
                id="sort-by"
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
              >
                <option value="dueDateAsc">
                  Due Date: Earliest
                </option>
                <option value="dueDateDesc">
                  Due Date: Latest
                </option>
                <option value="titleAsc">
                  Title: A-Z
                </option>
                <option value="titleDesc">
                  Title: Z-A
                </option>
                <option value="priorityHigh">
                  Priority: High-Low
                </option>
                <option value="priorityLow">
                  Priority: Low-High
                </option>
              </select>
            </div>
            <button
              type="button"
              onClick={clearFilters}
              disabled={!hasFilters}
            >
              Clear Filters
            </button>
          </div>
          {filteredTasks.length === 0 ? (
            <div className="empty-state">
              <h2>No tasks found</h2>
              <p>
                Try changing your search or filter criteria.
              </p>
              {hasFilters && (
                <button
                  type="button"
                  onClick={clearFilters}
                >
                  Clear Filters
                </button>
              )}
            </div>
          ) : (
            <div className="task-table-wrapper">
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
                  {filteredTasks.map((task) => (
                    <tr key={task.id}>
                      <td>
                        <Link to={`/tasks/${task.id}`}>
                          {task.title || 'Untitled Task'}
                        </Link>
                      </td>
                      <td>{normalizeStatus(task.status)}</td>
                      <td>{normalizePriority(task.priority)}</td>
                      <td>{task.category || '-'}</td>
                      <td>
                        {task.dueDate
                          ? new Date(
                              task.dueDate
                            ).toLocaleDateString()
                          : '-'}
                      </td>
                      <td>
                        {task.assignedToUserName || '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </div>
  );
}
