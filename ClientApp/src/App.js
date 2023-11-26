import * as React from 'react';
import { useState } from 'react';
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";

import CssBaseline from '@mui/material/CssBaseline';
import Backdrop from '@mui/material/Backdrop';
import CircularProgress from '@mui/material/CircularProgress';

import { createTheme, ThemeProvider } from '@mui/material/styles';
import { deepOrange, orange } from '@mui/material/colors';

import NavBar from './NavBar';
import Books from './Books';
import Reader from './Reader';

const defaultTheme = createTheme({
  palette: {
    primary: deepOrange,
    secondary: orange,
  }
});

export default function App() {
  const [titles, setTitles] = useState([]);
  const [allowDownloadAll, setAllowDownloadAll] = useState(false);
  const [open, setOpen] = React.useState(false);

  const stopSpinner = () => {
    setOpen(false);
  };
  const startSpinner = () => {
    setOpen(true);
  };

  const router = createBrowserRouter([
    {
      path: "/:category?/:serie?/:book?",
      element: <React.Fragment>
        <NavBar titles={titles} allowDownloadAll={allowDownloadAll} stopSpinner={stopSpinner} startSpinner={startSpinner}>
        </NavBar>
        <main>
          <Books setTitles={setTitles} setAllowDownloadAll={setAllowDownloadAll} stopSpinner={stopSpinner} startSpinner={startSpinner}></Books>
        </main>
      </React.Fragment>
    },
    {
      path: "/:category?/:serie?/:book?/:page",
      element: <React.Fragment>
        <Reader stopSpinner={stopSpinner} startSpinner={startSpinner}></Reader>
      </React.Fragment>
    }
  ]);


  return (
    <ThemeProvider theme={defaultTheme}>
      <CssBaseline />
      <RouterProvider router={router} />
      <Backdrop
        sx={{ color: '#fff', zIndex: (theme) => theme.zIndex.drawer + 1000 }}
        open={open}
      >
        <CircularProgress color="inherit" />
      </Backdrop>
    </ThemeProvider>
  );
}