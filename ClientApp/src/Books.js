import * as React from 'react';
import { useState, useEffect } from 'react';
import Card from '@mui/material/Card';
import Button from '@mui/material/Button';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTheme } from "@mui/material/styles";
import { getCurrentPage, getBookStatus } from "./progress";
import DoneIcon from '@mui/icons-material/Done';
import PendingIcon from '@mui/icons-material/Pending';

import { saveAs } from "file-saver";

const getBookId = (name) => {
    if (name > 3) {
        return Number.parseInt(name.substring(0, 3));
    }

    return Number.parseInt(name);
};

const completeSerie = (reader, category, serie, books) => {
    const newBooks = [...books];

    let firstBookId = -1;
    let lastBookId = -1;

    newBooks.forEach((book) => {
        const bookId = getBookId(book.name);
        if (Number.isInteger(bookId)) {
            if (firstBookId === -1) {
                firstBookId = bookId;
            }

            lastBookId = bookId;
        }
    });

    if (firstBookId > -1) {
        for (var i = firstBookId; i <= lastBookId; i++) {
            const previousBook = (i - 1).toString().padStart(3, '0');
            const currentBook = i.toString().padStart(3, '0');

            if (!newBooks.some(b => b.name === currentBook)) {
                const previous = newBooks.filter(b => b.name.startsWith(previousBook)).pop();
                const index = newBooks.indexOf(previous);
                newBooks.splice(index + 1, 0, {
                    "name": currentBook,
                    "thumbnail": "Missing.jpg",
                    "isMissing": true
                });
            }
        }
    }

    return (reader === "img") ? newBooks.map((book) => ({
        ...book,
        ...getBookStatus(category, serie, book.name)
    })) : newBooks;
};

export default function Books({ setTitles, setAllowDownloadAll, stopSpinner, startSpinner, reader }) {
    const theme = useTheme();
    const [books, setBooks] = useState([]);
    const location = useLocation();
    const navigate = useNavigate();

    const check = async () => {
        const [/*_*/, category, serie, book] = location.pathname.split('/');
        if (category) {
            const url = `/api/book${location.pathname}`;

            startSpinner();
            fetch(url)
                .then((response) => response.json())
                .then((data) => {
                    const collection = serie ? completeSerie(reader, category, serie, data.collection) : data.collection;

                    if (collection.length === 0) {
                        setTimeout(check, 3000);
                    } else {
                        setBooks(collection);
                        setTitles([category, serie, book].filter(Boolean));
                        setAllowDownloadAll(Boolean(serie || book));
                        stopSpinner();
                    }
                })
                .catch(() => {
                    stopSpinner();
                })
        }
        else {
            setBooks([
                {
                    "name": "Recherche",
                    "thumbnail": "Search.jpg",
                    "onClick": () => {
                        navigate("/search/");
                        window.location.reload();
                    }
                },
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
    }

    useEffect(() => {
        check();
    }, [location]);

    const nav = async (current) => {
        const pathname = location.pathname.endsWith('/') ? location.pathname : `${location.pathname}/`;
        const [/*_*/, category, serie, /*book*/] = pathname.split('/');

        if (!category || !serie) {
            navigate(`${pathname}${encodeURI(current)}`);
            window.scrollTo(0, 0);
        } else if (reader === "pdf") {
            const link = `/api/download/${category}/${serie}/${current}`;
            startSpinner();
            saveAs(link, `${decodeURI(serie)}.pdf`);
            stopSpinner();
        }
        else {
            navigate(`${pathname}${encodeURI(current)}/${getCurrentPage(category, serie, current)}`);
        }
    }

    const getCardStyle = (book) => {
        const cardSize = {
            width: 111,
            height: 147,
            display: 'flex',
            flexDirection: 'column'
        };

        if (book.isMain) {
            cardSize.width *= 3;
            cardSize.height *= 3;
        }

        cardSize.height += 50;

        return cardSize;
    };

    const getThumbnailStyle = (book) => {
        const thumbnailSize = {
            width: "auto",
            height: 147,
        };

        if (book.isMain) {
            thumbnailSize.height *= 3;
        }

        return thumbnailSize;
    };

    const getLegendStyle = (book) => {
        const legendStyle = {
            ...theme.typography.subtitle2,
            height: 50,
            fontSize: 10,
            backgroundColor: theme.palette.primary.A200,
            color: theme.palette.primary.contrastText,
            padding: 2
        };

        if (book.isMain) {
            legendStyle.fontSize *= 3;
        }

        return legendStyle;
    };

    return (
        <div style={{ "textAlign": "center" }}>
            {books.map((book) => (
                <Button variant="contained" key={book.name} onClick={() => book.onClick ? book.onClick() : (!book.isMissing && nav(book.name))}
                    style={{ margin: 3, padding: 3 }}
                >
                    <Card
                        sx={{ ...getCardStyle(book) }}
                    >
                        <img alt={book.name} style={{ ...getThumbnailStyle(book) }} src={`/cache/thumbnails/${book.thumbnail}`}></img>
                        <div style={{ ...getLegendStyle(book) }}>
                            {book.name}
                            {book.isMissing ? " - volume manquant" : ""}
                        </div>
                        {book.isCompleted && <DoneIcon style={{ position: "absolute", bottom: 5, right: 5, color: "#fff" }} />}
                        {(!book.isCompleted && book.isStarted) && <PendingIcon style={{ position: "absolute", bottom: 5, right: 5, color: "#fff" }} />}
                    </Card>
                </Button>
            ))}
        </div>
    );
}