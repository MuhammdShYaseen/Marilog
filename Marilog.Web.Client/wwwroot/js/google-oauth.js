// wwwroot/js/google-oauth.js
window.openGoogleAuthPopup = function (authorizeUrl) {
    return new Promise((resolve, reject) => {
        const popup = window.open(authorizeUrl, "google-oauth", "width=520,height=650");

        if (!popup) {
            reject("Popup blocked. Please allow popups for this site.");
            return;
        }

        const timer = setInterval(() => {
            if (popup.closed) {
                clearInterval(timer);
                window.removeEventListener("message", listener);
                reject("Connection window was closed before completing.");
            }
        }, 500);

        function listener(event) {
            if (event.origin !== window.location.origin) return;
            if (!event.data || !event.data.accessToken) return;

            clearInterval(timer);
            window.removeEventListener("message", listener);
            resolve(event.data);
        }

        window.addEventListener("message", listener);
    });
};