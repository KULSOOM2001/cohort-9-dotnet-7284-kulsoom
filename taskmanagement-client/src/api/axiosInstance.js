import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: 'https://localhost:7277/api',
});

axiosInstance.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default axiosInstance;