import * as React from 'react';
import { useNavigate } from 'react-router-dom';

import AppBar from '@mui/material/AppBar';
import BookIcon from '@mui/icons-material/Book';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';

export default function NavBar({titles}) {
  const navigate = useNavigate();

  const nav = (title) => {
    if (!title) {
        navigate("/");
    } else if (title !== titles[titles.length - 1]) {
        navigate(`/${title}`);
    }
  }

  return (
      <AppBar position="relative">
        <Toolbar className="breadcrumb">
          <IconButton color="inherit">
            <BookIcon onClick={() => nav()}/>
          </IconButton>
          <ul>
            {titles.map((title) => (
              <li key={title} variant="h6" color="inherit" noWrap onClick={() => nav(title)}>
                {decodeURIComponent(title)}
              </li>
            ))}
          </ul>
        </Toolbar>
      </AppBar>
  );
}