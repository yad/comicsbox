import * as React from 'react';
import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Dialog from '@mui/material/Dialog';
import Slide from '@mui/material/Slide';

const Transition = React.forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export default function Reader({ stopSpinner, startSpinner }) {
    const [img, setImg] = useState(null);
    const [lock, setLocked] = useState(false);
    const [open, setOpen] = useState(true);
    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        if (lock) {
            return
        };

        const [/*_*/, category, serie, book, page] = location.pathname.split('/');
        const link = `/api/reader/${category}/${serie}/${book}/${page}`;

        setLocked(true);
        startSpinner();

        fetch(link, { "method": "POST" })
            .then(async () => {
                const check = async () => {
                    const response = await fetch(link, { "method": "GET" });
                    if (response.status === 404) {
                        stopSpinner();
                        setLocked(false);
                        handleClose();

                        const parts = location.pathname.split('/');
                        parts.pop(); // remove page
                        parts.pop(); // remove book

                        navigate(`${parts.join("/")}`);
                        window.location.reload();
                    }
                    else if (response.status === 204) {
                        setTimeout(check, 3000)
                    }
                    else {
                        if (response.status === 200) {
                            stopSpinner();
                            setLocked(false);
                            setImg(`/temp/${category}/${serie}/${book}/${page}.jpg`);
                            //   saveAs(`/temp/${serie}.zip`, `${decodeURI(serie)}.zip`);
                        } else {
                            stopSpinner();
                            setLocked(false);
                            throw new Error(response);
                        }
                    }
                }

                await check();
            })
            .catch(() => {
                stopSpinner();
                setLocked(false);
            });
    }, [location]);

    const handleClickOpen = () => {
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
    };

    const getCardStyle = () => ({
        height: "100%",
        backgroundColor: "#424242"
    });

    const nav = (ev) => {
        const parts = location.pathname.split('/');
        const page = Number.parseInt(parts.pop());

        const halfScreenPosition = ev.view.innerWidth / 2;
        if (ev.clientX > halfScreenPosition) {
            const nextPage = page + 1;
            navigate(`${parts.join("/")}/${nextPage}`);
        }
        else {
            const previousPage = page - 1;
            if (previousPage === 0) {
                parts.pop(); // remove book
                navigate(`${parts.join("/")}`);
                window.location.reload();
            }
            else {
                navigate(`${parts.join("/")}/${previousPage}`);
            }
        }
    };

    return (
        <React.Fragment>
            <Dialog
                fullScreen
                open={open}
                onClose={handleClose}
                TransitionComponent={Transition}
            >
                <div style={{ "textAlign": "center", backgroundColor: "#424242" }}>
                    <Button
                        style={{ height: "100vh" }}
                        onClick={nav}
                    >
                        <Card
                            sx={{ ...getCardStyle() }}
                        >
                            <img src={img} style={{ height: "100%", maxWidth: "100%", objectFit: "contain" }}></img>
                        </Card>
                    </Button>
                </div>
            </Dialog>
        </React.Fragment>
    );
}