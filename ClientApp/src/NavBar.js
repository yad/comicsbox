import * as React from 'react';
import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import AppBar from '@mui/material/AppBar';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import DownloadIcon from '@mui/icons-material/Download';
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';
import ImageIcon from '@mui/icons-material/Image';
import { saveAs } from "file-saver";

export default function NavBar({ titles, allowDownloadAll, reader, setReader, stopSpinner, startSpinner }) {
  const [link, setLink] = useState(null);
  const location = useLocation();
  const navigate = useNavigate();

  const onChange = (value) => {
    const newValue = value === "img" ? "pdf" : "img";
    localStorage.setItem("reader", newValue);
    setReader(newValue);
    window.location.reload();
  };

  const nav = (title) => {
    if (!title) {
      navigate("/");
    } else if (title !== titles[titles.length - 1]) {
      navigate(`/${encodeURI(title)}`);
    }
  };

  const download = async () => {
    const [/*_*/, /*_*/, /*_*/, /*category*/, serie, /*book*/] = link.split('/');

    startSpinner();

    await fetch(link, { "method": "POST" })

    const check = async () => {
      const response = await fetch(link, { "method": "GET" });
      if (response.status === 204) {
        setTimeout(check, 3000)
      }
      else {
        if (response.status === 200) {
          stopSpinner();
          saveAs(`/temp/${serie}.zip`, `${decodeURI(serie)}.zip`);
        } else {
          stopSpinner();
          throw new Error(response);
        }
      }
    }

    await check();
  };

  useEffect(() => {
    setLink(`/api/download${location.pathname}`)
  }, [location]);

  return (
    <AppBar position="relative">
      <Toolbar className="breadcrumb">
        <IconButton color="inherit" onClick={() => nav()}>
          <MenuBookIcon />
        </IconButton>
        <ul>
          {titles.map((title) => (
            <li key={title} variant="h6" color="inherit" onClick={() => nav(title)}>
              {decodeURI(title)}
            </li>
          ))}
        </ul>
        {allowDownloadAll && (<IconButton style={{ "position": "absolute", "right": "64px" }} color="inherit" onClick={() => download()}>
          <DownloadIcon />
        </IconButton>)}
        <IconButton style={{ "position": "absolute", "right": "24px" }} color="inherit" onClick={() => onChange(reader)}>
          {reader === "img" ? <ImageIcon /> : <PictureAsPdfIcon />}
        </IconButton>
      </Toolbar>
    </AppBar>
  );
}