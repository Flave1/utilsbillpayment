// Toggle the sidebar when the menu icon is clicked
const menuIcon = document.querySelector('.menu-icon');
const sidebar = document.querySelector('.sidebar');
const dropdowns = document.querySelectorAll('.has-dropdown');

menuIcon.addEventListener('click', () => {
    sidebar.classList.toggle('active');
});

// Toggle the dropdown when clicking on a parent menu item
dropdowns.forEach((dropdown) => {
    dropdown.addEventListener('click', () => {
        dropdown.classList.toggle('open');
    });
});

//COUNT UP
const countUpElements = document.querySelectorAll('.count-up');

function animateCount(element, countTo, duration) {
    const start = performance.now();
    let countFrom = parseInt(element.textContent);

    function updateCount() {
        const now = performance.now();
        const elapsed = now - start;
        const progress = Math.min(elapsed / duration, 1);
        const currentCount = Math.floor(progress * (countTo - countFrom) + countFrom);
        element.textContent = currentCount.toLocaleString();

        if (progress < 1) {
            requestAnimationFrame(updateCount);
        }
    }

    requestAnimationFrame(updateCount);
}

countUpElements.forEach(element => {
    const countTo = parseInt(element.getAttribute('data-count'));
    const duration = 2000; // Animation duration in milliseconds
    animateCount(element, countTo, duration);
});


//PROFILE DROP
const userProfileDropdown = document.querySelector('.user-profile');

userProfileDropdown.addEventListener('click', () => {
    userProfileDropdown.classList.toggle('show');
});