export const getContext = (location) => {
    const pathname = location.pathname.endsWith('/') ? location.pathname : `${location.pathname}/`;

    const parts = pathname.split('/').filter(Boolean);
    if (parts[0] !== "Comics%20Box") {
        parts.splice(0, 0, "");
    }

    const [virtualDir, category, serie, book, pageStr] = parts;
    const page = Number.parseInt(pageStr);

    const currentPath = `/${parts.join("/")}/`.replace('//', '/');

    const result = {
        virtualDir, category, serie, book, page,
        currentPath,
        "closeBook": `/${[virtualDir, category, serie].filter(Boolean).join("/")}/`.replace('//', '/'),
        "closeSerie": `/${[virtualDir, category].filter(Boolean).join("/")}/`.replace('//', '/'),
        "closeCategory": `/${[virtualDir].filter(Boolean).join("/")}/`.replace('//', '/'),
        "nextPageLocation": `/${[virtualDir, category, serie, book, page + 1].filter(Boolean).join("/")}/`.replace('//', '/'),
        "previousPageLocation": `/${[virtualDir, category, serie, book, page - 1].filter(Boolean).join("/")}/`.replace('//', '/'),
        "apiBook": `/${[virtualDir, "/api/book", category, serie, book, page].filter(Boolean).join("/")}`.replace('//', '/'),
        "apiDownload": `/${[virtualDir, "/api/download", category, serie, book, page].filter(Boolean).join("/")}`.replace('//', '/'),
        "nextLocation": (current) => `${currentPath}${current}`
    };

    return result;
}
