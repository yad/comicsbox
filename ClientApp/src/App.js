import * as React from 'react';
import { useState } from 'react';
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";

import CssBaseline from '@mui/material/CssBaseline';
import Backdrop from '@mui/material/Backdrop';
import CircularProgress from '@mui/material/CircularProgress';
import GlobalStyles from '@mui/material/GlobalStyles';

import { createTheme, ThemeProvider } from '@mui/material/styles';
import { deepOrange, red } from '@mui/material/colors';

import NavBar from './NavBar';
import Books from './Books';
import Reader from './Reader';
import Search from './Search';

const imgTheme = createTheme({
  palette: {
    primary: deepOrange
  }
});

const pdfTheme = createTheme({
  palette: {
    primary: red
  }
});

export default function App() {
  const [reader, setReader] = useState(localStorage.getItem("reader") || "img");
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
      path: "/search/",
      element: <React.Fragment>
        <NavBar titles={titles} allowDownloadAll={allowDownloadAll} reader={reader} setReader={setReader} stopSpinner={stopSpinner} startSpinner={startSpinner}></NavBar>
        <main>
          <Search stopSpinner={stopSpinner} startSpinner={startSpinner}></Search>
        </main>
      </React.Fragment>
    },
    {
      path: "/:category?/:serie?/:book?",
      element: <React.Fragment>
        <NavBar titles={titles} allowDownloadAll={allowDownloadAll} reader={reader} setReader={setReader} stopSpinner={stopSpinner} startSpinner={startSpinner}></NavBar>
        <main>
          <Books setTitles={setTitles} setAllowDownloadAll={setAllowDownloadAll} reader={reader} stopSpinner={stopSpinner} startSpinner={startSpinner}></Books>
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
    <ThemeProvider theme={reader === "img" ? imgTheme : pdfTheme}>
      <CssBaseline />
      <GlobalStyles
          styles={{
            body: { backgroundColor: "#424242" },
          }}
      />
      <RouterProvider router={router} />
      <Backdrop
        sx={{ color: '#424242', zIndex: (theme) => theme.zIndex.drawer + 1000 }}
        open={open}
      >
        <CircularProgress color="inherit" />
      </Backdrop>
    </ThemeProvider>
  );
}