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

const statusToNumber = (status) => {
  const value = String(status ?? '').trim().toLowerCase();

  if (value === '0' || value === 'pending') {
    return 0;
  }

  if (
    value === '1' ||
    value === 'in progress' ||
    value === 'inprogress'
  ) {
    return 1;
  }

  if (value === '2' || value === 'completed') {
    return 2;
  }

  return null;
};

const priorityToNumber = (priority) => {
  const value = String(priority ?? '').trim().toLowerCase();

  if (value === '0' || value === 'low') {
    return 0;
  }

  if (value === '1' || value === 'medium') {
    return 1;
  }

  if (value === '2' || value === 'high') {
    return 2;
  }

  return null;
};

const escapeCsvValue = (value) => {
  const text = value == null ? '' : String(value);

  if (
    text.includes(',') ||
    text.includes('"') ||
    text.includes('\n') ||
    text.includes('\r')
  ) {
    return `"${text.replace(/"/g, '""')}"`;
  }

  return text;
};

const parseCsvLine = (line) => {
  const values = [];
  let current = '';
  let insideQuotes = false;

  for (let i = 0; i < line.length; i += 1) {
    const character = line[i];

    if (character === '"') {
      if (insideQuotes && line[i + 1] === '"') {
        current += '"';
        i += 1;
      } else {
        insideQuotes = !insideQuotes;
      }
    } else if (character === ',' && !insideQuotes) {
      values.push(current);
      current = '';
    } else {
      current += character;
    }
  }

  values.push(current);

  return values;
};

const parseCsv = (text) => {
  const normalizedText = text.replace(/^\uFEFF/, '');

  const lines = normalizedText
    .split(/\r?\n/)
    .filter((line) => line.trim() !== '');

  if (lines.length < 2) {
    throw new Error(
      'CSV file must contain a header row and at least one task.'
    );
  }

  const headers = parseCsvLine(lines[0]).map((header) =>
    header.trim().toLowerCase()
  );

  const requiredHeaders = [
    'title',
    'description',
    'status',
    'priority',
    'category',
    'duedate',
    'assignedtouserid',
  ];

  const missingHeaders = requiredHeaders.filter(
    (header) => !headers.includes(header)
  );

  if (missingHeaders.length > 0) {
    throw new Error(
      `Missing CSV columns: ${missingHeaders.join(', ')}`
    );
  }

  return lines.slice(1).map((line) => {
    const values = parseCsvLine(line);
    const row = {};

    headers.forEach((header, index) => {
      row[header] = values[index] ?? '';
    });

    return row;
  });
};

export default function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [realtimeStatus, setRealtimeStatus] =
    useState('Connecting...');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [dueDateFilter, setDueDateFilter] = useState('');
  const [sortBy, setSortBy] = useState('dueDateAsc');
  const [importing, setImporting] = useState(false);

  const requestIdRef = useRef(0);
  const fileInputRef = useRef(null);

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
    let isActive = true;

    const fetchTasks = async () => {
      if (!isActive) return;
      await loadTasks();
    };

    fetchTasks();

    return () => {
      isActive = false;
    };
  }, []);

  useEffect(() => {
    const hubBaseUrl =
      import.meta.env.VITE_API_BASE_URL ||
      'http://localhost:5025/api';

    const apiRoot = hubBaseUrl.replace(/\/api\/?$/, '');
    const hubUrl = `${apiRoot}/hubs/tasks`;

    const parsedHubUrl = new URL(
      hubUrl,
      window.location.origin
    );

    const isLocalDevelopment =
      parsedHubUrl.hostname === 'localhost' ||
      parsedHubUrl.hostname === '127.0.0.1';

    if (
      parsedHubUrl.protocol !== 'https:' &&
      !isLocalDevelopment
    ) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () =>
          sessionStorage.getItem('token') || '',
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
        loadTasks();
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

          const delay = Math.min(
            1000 * 2 ** attempt,
            10000
          );

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
        !priorityFilter ||
        priority === priorityFilter;

      const taskDueDate = task.dueDate
        ? String(task.dueDate).split('T')[0]
        : '';

      const matchesDueDate =
        !dueDateFilter ||
        taskDueDate === dueDateFilter;

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

  const handleExport = () => {
    if (filteredTasks.length === 0) {
      setError('There are no tasks to export.');
      return;
    }

    const headers = [
      'title',
      'description',
      'status',
      'priority',
      'category',
      'dueDate',
      'assignedToUserId',
    ];

    const rows = filteredTasks.map((task) => [
      task.title || '',
      task.description || '',
      normalizeStatus(task.status),
      normalizePriority(task.priority),
      task.category || '',
      task.dueDate
        ? String(task.dueDate).split('T')[0]
        : '',
      task.assignedToUserId ?? '',
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map((row) =>
        row.map(escapeCsvValue).join(',')
      ),
    ].join('\r\n');

    const blob = new Blob([csvContent], {
      type: 'text/csv;charset=utf-8;',
    });

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = `tasks-${new Date()
      .toISOString()
      .slice(0, 10)}.csv`;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(url);
  };

  const handleImportClick = () => {
    setError('');

    if (fileInputRef.current) {
      fileInputRef.current.value = '';
      fileInputRef.current.click();
    }
  };

  const handleImport = async (event) => {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    setError('');
    setImporting(true);

    try {
      if (!file.name.toLowerCase().endsWith('.csv')) {
        throw new Error('Please select a CSV file.');
      }

      const text = await file.text();
      const rows = parseCsv(text);

      const tasksToImport = rows.map((row, index) => {
        const rowNumber = index + 2;

        const title = String(row.title || '').trim();

        if (!title) {
          throw new Error(
            `Row ${rowNumber}: Title is required.`
          );
        }

        const status = statusToNumber(row.status);
        const priority = priorityToNumber(row.priority);

        if (status === null) {
          throw new Error(
            `Row ${rowNumber}: Invalid status. Use Pending, In Progress, or Completed.`
          );
        }

        if (priority === null) {
          throw new Error(
            `Row ${rowNumber}: Invalid priority. Use Low, Medium, or High.`
          );
        }

        const assignedToUserId = Number(
          row.assignedtouserid
        );

        if (
          !row.assignedtouserid ||
          !Number.isInteger(assignedToUserId) ||
          assignedToUserId <= 0
        ) {
          throw new Error(
            `Row ${rowNumber}: Assigned To User ID must be a positive number.`
          );
        }

        return {
          title,
          description:
            String(row.description || '').trim() || null,
          status,
          priority,
          category:
            String(row.category || '').trim() || null,
          dueDate:
            String(row.duedate || '').trim() || null,
          assignedToUserId,
        };
      });

      let importedCount = 0;

      for (const task of tasksToImport) {
        await axiosInstance.post('/Tasks', task);
        importedCount += 1;
      }

      await loadTasks();

      setError(
        `Successfully imported ${importedCount} task${
          importedCount === 1 ? '' : 's'
        }.`
      );
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.message ||
          'Failed to import tasks.'
      );
    } finally {
      setImporting(false);

      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const downloadImportTemplate = () => {
    const headers = [
      'title',
      'description',
      'status',
      'priority',
      'category',
      'dueDate',
      'assignedToUserId',
    ];

    const exampleRow = [
      'Example Task',
      'Example description',
      'Pending',
      'Medium',
      'Work',
      '2026-09-15',
      '1',
    ];

    const csvContent = [
      headers.join(','),
      exampleRow.map(escapeCsvValue).join(','),
    ].join('\r\n');

    const blob = new Blob([csvContent], {
      type: 'text/csv;charset=utf-8;',
    });

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = 'task-import-template.csv';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(url);
  };

  return (
    <div className="page">
      <div className="task-list-header">
        <div>
          <h1>Tasks</h1>

          <p>
            Showing {filteredTasks.length} of {tasks.length}{' '}
            tasks
          </p>

          <small>
            Real-time updates:{' '}
            <strong>{realtimeStatus}</strong>
          </small>
        </div>

        <div className="task-list-actions">
          <button
            type="button"
            onClick={handleExport}
            disabled={loading || filteredTasks.length === 0}
          >
            Export CSV
          </button>

          <button
            type="button"
            onClick={handleImportClick}
            disabled={loading || importing}
          >
            {importing ? 'Importing...' : 'Import CSV'}
          </button>

          <button
            type="button"
            onClick={downloadImportTemplate}
            disabled={loading || importing}
          >
            CSV Template
          </button>

          <input
            ref={fileInputRef}
            type="file"
            accept=".csv,text/csv"
            onChange={handleImport}
            hidden
          />

          <Link
            className="new-task-button"
            to="/tasks/new"
          >
            New Task
          </Link>
        </div>
      </div>

      {error && <p className="auth-error">{error}</p>}

      {loading && <p>Loading tasks...</p>}

      {!loading && (
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
                onChange={(e) =>
                  setStatusFilter(e.target.value)
                }
              >
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="In Progress">
                  In Progress
                </option>
                <option value="Completed">Completed</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="priority-filter">
                Priority
              </label>

              <select
                id="priority-filter"
                value={priorityFilter}
                onChange={(e) =>
                  setPriorityFilter(e.target.value)
                }
              >
                <option value="">All Priorities</option>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="due-date-filter">
                Due Date
              </label>

              <input
                id="due-date-filter"
                type="date"
                value={dueDateFilter}
                onChange={(e) =>
                  setDueDateFilter(e.target.value)
                }
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

                      <td>
                        {normalizeStatus(task.status)}
                      </td>

                      <td>
                        {normalizePriority(task.priority)}
                      </td>

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