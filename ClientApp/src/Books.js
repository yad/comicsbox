import * as React from 'react';
import { useState, useEffect } from 'react';
import Card from '@mui/material/Card';
import Button from '@mui/material/Button';
import { useLocation, useNavigate } from 'react-router-dom';

export default function Books({ setTitles }) {
    const [books, setBooks] = useState([]);
    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        const [_, category, serie, book] = location.pathname.split('/');
        if (category) {
            const url = `/api/book${location.pathname}`;

            fetch(url)
                .then((response) => response.json())
                .then((data) => {
                    setBooks(data.collection);
                    setTitles([category, serie, book].filter(Boolean))
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
            ].map((category) => ({...category, "isMain": true})));
            setTitles([]);
        }
    }, [location]);

    const pathname = location.pathname.endsWith('/') ? location.pathname : `${location.pathname}/`;

    return (
        <div style={{ "textAlign": "center" }}>
            {books.map((book) => (
                <Button variant="contained" item key={book.name} onClick={() => navigate(`${pathname}${book.name}`)}
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