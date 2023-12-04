import * as React from 'react';
import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTheme } from "@mui/material/styles";
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Autocomplete from '@mui/material/Autocomplete';

export default function Search({ stopSpinner, startSpinner }) {
    const theme = useTheme();
    const [series, setSeries] = useState([]);
    const location = useLocation();
    const navigate = useNavigate();

    const check = async () => {
        const url = `/api/search`;

        startSpinner();
        fetch(url)
            .then((response) => response.json())
            .then((data) => {
                const collection = data.collection;

                if (collection.length === 0) {
                    setTimeout(check, 3000);
                } else {
                    setSeries(collection);
                    stopSpinner();
                }
            })
            .catch(() => {
                stopSpinner();
            })
    };

    useEffect(() => {
        check();
    }, [location]);

    const filterOptions = (options, state) => {
        if (state.inputValue.length > 2) {
            return options.filter((item) =>
                String(item.name).toLowerCase().includes(state.inputValue.toLowerCase())
            );
        }
        return [];
    };

    const nav = (serie) => {
        const url = `/${encodeURI(serie.category)}/${encodeURI(serie.name)}`;
        navigate(url);
    };

    return (
        <div style={{ "textAlign": "center", width: "100%", padding: 10 }}>
            <Autocomplete
                style={{ height: "80vh", width: "100%" }}
                options={series}
                autoHighlight
                ListboxProps={{ style: { maxHeight: "100%" } }}
                noOptionsText="Pas de résultats"
                filterOptions={filterOptions}
                getOptionLabel={(serie) => serie.name}
                renderOption={(props, serie) => (
                    <Box component="li" sx={{ '& > img': { mr: 2, flexShrink: 0 } }} {...props} onClick={() => nav(serie)}>
                        <img
                            loading="lazy"
                            width="20"
                            srcSet={`${`/cache/thumbnails/${serie.thumbnail}`} 2x`}
                            src={`${`/cache/thumbnails/${serie.thumbnail}`}`}
                            alt=""
                        />
                        {serie.name}
                    </Box>
                )}
                renderInput={(params) => (
                    <TextField
                        {...params}
                        color="primary"
                        label="Recherche, à partir de 3 caractères..."
                        inputProps={{
                            ...params.inputProps,
                            style: { color: theme.palette.primary.contrastText },
                            autoComplete: 'off', // disable autocomplete and autofill
                        }}
                    />
                )}
            />
        </div>
    );
};
