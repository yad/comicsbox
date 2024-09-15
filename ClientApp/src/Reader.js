import * as React from 'react';
import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Dialog from '@mui/material/Dialog';
import Slide from '@mui/material/Slide';
import CloseIcon from '@mui/icons-material/Close';
import { saveCompleted, saveProgress } from "./progress";
import { getContext } from "./Locator";

const Transition = React.forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export default function Reader({ stopSpinner, startSpinner }) {
    const [img, setImg] = useState(null);
    const [open, setOpen] = useState(true);
    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        startSpinner();

        const { category, serie, book, page, jpgPage } = getContext(location);
        const link = `/api/reader/${category}/${serie}/${book}/${page}`;

        fetch(link, { "method": "POST" })
            .then(async () => {
                const check = async () => {
                    const response = await fetch(link, { "method": "GET" });
                    if (response.status === 404) {
                        saveCompleted(category, serie, book, page);
                        handleClose();
                    }
                    else if (response.status === 204) {
                        setTimeout(check, 1000)
                    }
                    else {
                        if (response.status === 200) {
                            stopSpinner();
                            saveProgress(category, serie, book, page);
                            setImg(jpgPage);
                        } else {
                            stopSpinner();
                            throw new Error(response);
                        }
                    }
                }

                await check();
            })
            .catch(() => {
                stopSpinner();
            });
    }, [location]);

    const handleClose = () => {
        stopSpinner();

        const { closeBook } = getContext(location);
        navigate(closeBook);
        window.location.reload();

        setOpen(false);
    };

    const getCardStyle = () => ({
        height: "100%",
        backgroundColor: "#424242"
    });

    const getCloseStyle = () => ({
        width: "48px",
        height: "48px",
        position: "absolute",
        color: "lightgrey",
        top: 0,
        right: 0
    });

    const nav = (ev) => {
        const { page, nextPageLocation, previousPageLocation, closeBook } = getContext(location);

        const halfScreenPosition = ev.view.innerWidth / 2;
        if (ev.clientX > halfScreenPosition) {
            navigate(nextPageLocation);
        }
        else {
            const previousPage = page - 1;
            if (previousPage === 0) {
                navigate(closeBook);
                window.location.reload();
            }
            else {
                navigate(previousPageLocation);
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
                    <Button style={{ height: "100vh" }} onClick={nav}>
                        {img && <Button style={{ ...getCloseStyle() }} onClick={handleClose}>
                            <CloseIcon />
                        </Button>}
                        <Card sx={{ ...getCardStyle() }}>
                            <img src={img} style={{ height: "100%", maxWidth: "100%", objectFit: "contain" }}></img>
                        </Card>
                    </Button>
                </div>
            </Dialog>
        </React.Fragment>
    );
}