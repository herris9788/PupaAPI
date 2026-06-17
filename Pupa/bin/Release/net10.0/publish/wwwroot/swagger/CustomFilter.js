(function () {
    const originalFetch = window.fetch;
    // Intersep saat Swagger mulai render
    window.addEventListener("load", function () {
        const checkExist = setInterval(function () {
            const filterInput = document.querySelector('.operation-filter-input input');
            if (filterInput) {
                clearInterval(checkExist);

                filterInput.addEventListener('keyup', function () {
                    // Paksa pencarian ulang dengan memicu event input
                    // Beberapa versi Swagger UI memperbaiki ini jika kita 
                    // memicu event secara manual
                    const event = new Event('input', { bubbles: true });
                    filterInput.dispatchEvent(event);
                });

                console.log("Swagger filter patch applied");
            }
        }, 500);
    });
})();