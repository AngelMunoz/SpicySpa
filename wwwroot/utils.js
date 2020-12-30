

export const get = (url) =>
    fetch(url, {
        headers: { 'Content-Type': 'application/json', "Accept": 'application/json' },
        mode: "same-origin",
        redirect: "error",
        credentials: "include",
        method: 'GET'
    })
        .then(result => result.ok ? result.json() : Promise.reject(result.statusText));