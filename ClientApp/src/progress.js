export const saveProgress = (category, serie, book, page) => {
    const db = _load(category, serie, book);
    const currentBook = db[category][serie][book];
    currentBook.currentPage = Number.parseInt(page);
    _save(db);
};

export const saveCompleted = (category, serie, book) => {
    const db = _load(category, serie, book);
    const currentBook = db[category][serie][book];
    currentBook.isCompleted = true;
    currentBook.currentPage = 1;
    _save(db);
};

export const getCurrentPage = (category, serie, book) => {
    const db = _load(category, serie, book);
    const currentBook = db[category][serie][book];
    return currentBook.currentPage;
};

export const getBookStatus = (category, serie, book) => {
    const db = _load(category, serie, book);
    const currentBook = db[category][serie][book];
    return {
        isCompleted: currentBook.isCompleted,
        isStarted: currentBook.currentPage > 1
    };
};

const _key = "db";
const _save = (db) => localStorage.setItem(_key, JSON.stringify(db));
const _load = (category, serie, book) => {
    const data = localStorage.getItem(_key) || "{}";
    const db = JSON.parse(data);

    if (!db[category]) {
        db[category] = {};
    }

    if (!db[category][serie]) {
        db[category][serie] = {};
    }

    if (!db[category][serie][book]) {
        db[category][serie][book] = {
            currentPage: 1,
            isCompleted: false
        };
    }

    return db;
}

