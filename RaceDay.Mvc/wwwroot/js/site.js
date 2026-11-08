// RaceDay - small progressive enhancements (the app works without them).
(function () {
    // Confirm before destructive actions: <form data-confirm="Are you sure?">
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!window.confirm(form.getAttribute('data-confirm'))) {
                e.preventDefault();
            }
        });
    });

    // Auto-submit pickers: <select data-autosubmit>
    document.querySelectorAll('select[data-autosubmit]').forEach(function (select) {
        select.addEventListener('change', function () { select.form.submit(); });
    });

    // Image preview: <input type="file" data-preview="#imgId">
    document.querySelectorAll('input[type=file][data-preview]').forEach(function (input) {
        input.addEventListener('change', function () {
            var target = document.querySelector(input.getAttribute('data-preview'));
            if (!target || !input.files || !input.files[0]) return;
            var reader = new FileReader();
            reader.onload = function (ev) {
                target.src = ev.target.result;
                target.classList.remove('d-none');
            };
            reader.readAsDataURL(input.files[0]);
        });
    });

    // Prevent double submits.
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (e.defaultPrevented) return;
            var btn = form.querySelector('button[type=submit][data-busy]');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>' + btn.getAttribute('data-busy');
            }
        });
    });
})();
