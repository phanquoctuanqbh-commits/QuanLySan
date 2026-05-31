const sportBackgrounds = {
    stadium: "/images/spick.jpg",
    football: "/images/Etihad.jpg",
    badminton: "/images/scl.jpg",
    tennis: "/images/stn.jpg",
    dashboard: "/images/sbr.jpg"
};

let backgroundIndex = 0;
const backgroundOrder = ["stadium", "football", "badminton", "tennis"];
let autoBackgroundTimer;

function setSportBackground(theme) {
    const nextImage = sportBackgrounds[theme] || sportBackgrounds.stadium;
    document.documentElement.style.setProperty("--page-bg-image", `url("${nextImage}")`);
    document.body.classList.add("bg-switching");

    window.clearTimeout(window.sportBgPulse);
    window.sportBgPulse = window.setTimeout(() => {
        document.body.classList.remove("bg-switching");
    }, 850);
}

function startAutoBackground() {
    window.clearInterval(autoBackgroundTimer);
    autoBackgroundTimer = window.setInterval(() => {
        backgroundIndex = (backgroundIndex + 1) % backgroundOrder.length;
        setSportBackground(backgroundOrder[backgroundIndex]);
    }, 6500);
}

document.addEventListener("DOMContentLoaded", () => {
    const path = window.location.pathname.toLowerCase();

    if (path.includes("/admin")) {
        setSportBackground("dashboard");
    } else if (path.includes("/datsan/calendar")) {
        setSportBackground("tennis");
    } else if (path.includes("/datsan/mybookings")) {
        setSportBackground("badminton");
    } else if (path.includes("/san")) {
        setSportBackground("football");
    } else {
        setSportBackground("stadium");
    }

    document.querySelectorAll("[data-bg-theme]").forEach((item) => {
        item.addEventListener("mouseenter", () => setSportBackground(item.dataset.bgTheme));
        item.addEventListener("click", () => setSportBackground(item.dataset.bgTheme));
    });

    startAutoBackground();
});
