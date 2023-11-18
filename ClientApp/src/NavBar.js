import * as React from 'react';
import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import AppBar from '@mui/material/AppBar';
import BookIcon from '@mui/icons-material/Book';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import DownloadIcon from '@mui/icons-material/Download';
import { saveAs } from "file-saver";

export default function NavBar({ titles, allowDownloadAll, stopSpinner, startSpinner }) {
  const [link, setLink] = useState(null);
  const location = useLocation();
  const navigate = useNavigate();

  const nav = (title) => {
    if (!title) {
      navigate("/");
    } else if (title !== titles[titles.length - 1]) {
      navigate(`/${title}`);
    }
  };

  const download = () => {
    startSpinner();
    const [_, category, serie, book] = link.split('/');
    saveAs(link, `${decodeURIComponent(serie)}.pdf`);
    stopSpinner();
  };

  useEffect(() => {
    setLink(`/api/download${location.pathname}`)
  }, [location]);

  return (
    <AppBar position="relative">
      <Toolbar className="breadcrumb">
        <IconButton color="inherit">
          <BookIcon onClick={() => nav()} />
        </IconButton>
        <ul>
          {titles.map((title) => (
            <li key={title} variant="h6" color="inherit" noWrap onClick={() => nav(title)}>
              {decodeURIComponent(title)}
            </li>
          ))}
        </ul>
        {allowDownloadAll && (<IconButton color="inherit">
          <DownloadIcon onClick={() => download()} />
        </IconButton>)}
      </Toolbar>
    </AppBar>
  );
}