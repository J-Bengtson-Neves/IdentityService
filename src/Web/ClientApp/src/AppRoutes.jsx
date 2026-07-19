import Login from "./components/Login";
import Dashboard from "./components/Dashboard";

const AppRoutes = [
  {
    index: true,
    element: <Login />
  },
  {
    path: '/login',
    element: <Login />
  },
  {
    path: '/dashboard',
    element: <Dashboard />
  }
];

export default AppRoutes;
