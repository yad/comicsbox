import * as React from 'react';
import { useState } from 'react';
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";

import CssBaseline from '@mui/material/CssBaseline';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { deepOrange, orange } from '@mui/material/colors';

import NavBar from './NavBar';
import Books from './Books';

const defaultTheme = createTheme({
  palette: {
    primary: deepOrange,
    secondary: orange,
  }
});

export default function App() {
  const [titles, setTitles] = useState([]);

  const router = createBrowserRouter([
    {
      path: "/:category?/:serie?/:book?",
      element: <div>
        <NavBar titles={titles} >
        </NavBar>
        <main>
          <Books setTitles={setTitles}></Books>
        </main>
      </div>,
    }
  ]);

  return (
    <ThemeProvider theme={defaultTheme}>
      <CssBaseline />
      <RouterProvider router={router} />
    </ThemeProvider>
  );
}