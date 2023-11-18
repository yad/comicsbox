import * as React from 'react';
import { useState, useEffect } from 'react';
import Card from '@mui/material/Card';
import Button from '@mui/material/Button';
import { useLocation, useNavigate } from 'react-router-dom';

import { saveAs } from "file-saver";

export default function Books({ setTitles, setAllowDownloadAll, stopSpinner, startSpinner }) {
    const [books, setBooks] = useState([]);
    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        const [_, category, serie, book] = location.pathname.split('/');
        if (category) {
            const url = `/api/book${location.pathname}`;

            startSpinner();
            fetch(url)
                .then((response) => response.json())
                .then((data) => {
                    setBooks(data.collection);
                    setTitles([category, serie, book].filter(Boolean));
                    setAllowDownloadAll(Boolean(serie || book));
                    stopSpinner();
                })
                .catch(() => {
                    stopSpinner();
                })
        }
        else {
            setBooks([
                {
                    "name": "BD",
                    "thumbnail": "BD.jpg"
                },
                {
                    "name": "Comics",
                    "thumbnail": "Comics.jpg"
                },
                {
                    "name": "Mangas",
                    "thumbnail": "Mangas.jpg"
                }
            ].map((category) => ({ ...category, "isMain": true })));
            setTitles([]);
            setAllowDownloadAll(false);
        }
    }, [location]);

    const nav = (current) => {
        const pathname = location.pathname.endsWith('/') ? location.pathname : `${location.pathname}/`;
        const [_, category, serie, book] = pathname.split('/');

        if (!category || !serie) {
            navigate(`${pathname}${current}`);
        } else {
            // temp download pdf
            const link = `/api/download/${category}/${serie}/${current}`;
            startSpinner();
            saveAs(link, `${decodeURIComponent(serie)}.pdf`);
            stopSpinner();
        }
    }


    return (
        <div style={{ "textAlign": "center" }}>
            {books.map((book) => (
                <Button variant="contained" item key={book.name} onClick={() => nav(book.name)}
                    style={{ margin: 3, padding: 3 }}
                >
                    <Card
                        sx={{ width: book.isMain ? '333px' : '111px', height: book.isMain ? '441px' : '147px', display: 'flex', flexDirection: 'column' }}
                    >
                        <img src={`/cache/thumbnails/${book.thumbnail}`}></img>
                    </Card>
                </Button>
            ))}
        </div>
    );
}