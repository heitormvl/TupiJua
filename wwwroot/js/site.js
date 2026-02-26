// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Register Form Progressive Reveal
function initRegisterForm() {
    const usernameInput = document.getElementById('Username');
    const emailInput = document.getElementById('Email');
    const passwordInput = document.getElementById('Password');
    const confirmPasswordInput = document.getElementById('ConfirmPassword');

    const emailField = document.getElementById('emailField');
    const passwordField = document.getElementById('passwordField');
    const confirmPasswordField = document.getElementById('confirmPasswordField');
    const submitButton = document.getElementById('submitButton');
    const loginLink = document.getElementById('loginLink');

    if (!confirmPasswordInput) return; // Exit if not on register page

    // Progressive field reveal
    function showNextField(currentField, nextField, delay = 400) {
        setTimeout(() => {
            nextField.style.display = 'block';
            // Force reflow para garantir que a transição aconteça
            nextField.offsetHeight;
            setTimeout(() => {
                nextField.classList.add('show');
            }, 10);
        }, delay);
    }

    usernameInput.addEventListener('input', function() {
        if (this.value.length >= 3 && emailField.style.display === 'none') {
            showNextField(usernameInput, emailField);
        }
    });

    emailInput.addEventListener('input', function() {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (emailRegex.test(this.value) && passwordField.style.display === 'none') {
            showNextField(emailInput, passwordField);
        }
    });

    passwordInput.addEventListener('input', function() {
        if (this.value.length >= 6 && confirmPasswordField.style.display === 'none') {
            showNextField(passwordInput, confirmPasswordField);
        }
    });

    confirmPasswordInput.addEventListener('input', function() {
        if (this.value.length >= 6) {
            if (submitButton.style.display === 'none') {
                showNextField(confirmPasswordInput, submitButton, 200);
            }
        }
    });

    // Toggle password visibility
    function setupPasswordToggle(buttonId, inputId) {
        const toggleButton = document.getElementById(buttonId);
        const input = document.getElementById(inputId);
        
        if (!toggleButton || !input) return;
        
        toggleButton.addEventListener('click', function() {
            const icon = this.querySelector('i');
            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        });
    }

    setupPasswordToggle('togglePassword', 'Password');
    setupPasswordToggle('toggleConfirmPassword', 'ConfirmPassword');

    // Show all fields on form validation error
    if (document.querySelector('.alert-danger')) {
        [emailField, passwordField, confirmPasswordField, submitButton].forEach(field => {
            field.style.display = 'block';
            field.classList.add('show');
        });
    }
}

// Login Form
function initLoginForm() {
    const passwordInput = document.getElementById('Password');
    const toggleButton = document.getElementById('togglePassword');
    
    if (!passwordInput || !toggleButton) return; // Exit if not on login page
    
    // Toggle password visibility
    toggleButton.addEventListener('click', function() {
        const icon = this.querySelector('i');
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            passwordInput.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    });
}

// User Profile Page
function initUserProfile() {
    const profileCard = document.querySelector('.profile-info');
    
    if (!profileCard) return; // Exit if not on user profile page
    
    // Add scale-in animation to card
    const card = document.querySelector('.card');
    if (card) {
        card.style.animation = 'scaleIn 0.3s ease';
    }

    const progressBar = document.querySelector('[data-profile-progress]');
    if (progressBar) {
        const percent = Number(progressBar.dataset.profileProgress) || 0;
        progressBar.style.width = '0%';
        progressBar.setAttribute('aria-valuenow', '0');
        const progressLabel = document.querySelector('[data-profile-progress-label]');
        setTimeout(() => {
            progressBar.style.width = `${percent}%`;
            progressBar.setAttribute('aria-valuenow', percent.toString());
            if (progressLabel) {
                progressLabel.textContent = `${percent}%`;
            }
        }, 160);
    }
}

// View Workout Page
function initViewWorkout() {
    // ── Delete modal ────────────────────────────────────────────────────
    const deleteModal = document.getElementById('deleteLoggedExerciseModal');
    if (!deleteModal) return; // Exit if not on ViewWorkout page

    const deleteForm = document.getElementById('deleteLoggedExerciseForm');
    const deleteBaseUrl = deleteForm?.dataset.deleteUrl ?? '';

    deleteModal.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const id = button.getAttribute('data-id');
        const name = button.getAttribute('data-name');

        deleteForm.action = deleteBaseUrl + '/' + id;
        document.getElementById('exerciseNameToDelete').textContent = name;
    });

    // ── Share feature ────────────────────────────────────────────────────
    const shareNameModal = document.getElementById('shareNameModal');
    if (!shareNameModal) return;

    const planName = shareNameModal.dataset.planName || null;

    // Radio option highlighting
    shareNameModal.querySelectorAll('input[name="shareNameOption"]').forEach(radio => {
        radio.addEventListener('change', () => {
            shareNameModal.querySelectorAll('.share-name-option').forEach(opt => {
                opt.style.borderColor = '';
                opt.style.background = '';
            });
            const selected = shareNameModal.querySelector('input[name="shareNameOption"]:checked');
            if (selected) {
                const label = selected.closest('.share-name-option');
                label.style.borderColor = '#0d6efd';
                label.style.background = 'rgba(13,110,253,0.06)';
            }

            const isCustom = document.getElementById('radioNameCustom')?.checked;
            const wrapper = document.getElementById('customNameWrapper');
            if (wrapper) {
                wrapper.classList.toggle('is-visible', isCustom);
                if (isCustom) setTimeout(() => document.getElementById('customShareName')?.focus(), 200);
            }
        });
    });

    // Highlight the initially checked option
    const initialChecked = shareNameModal.querySelector('input[name="shareNameOption"]:checked');
    if (initialChecked) {
        const label = initialChecked.closest('.share-name-option');
        if (label) { label.style.borderColor = '#0d6efd'; label.style.background = 'rgba(13,110,253,0.06)'; }
    }

    // Whole option card toggles the radio
    shareNameModal.querySelectorAll('.share-name-option').forEach(card => {
        card.addEventListener('click', function (e) {
            if (e.target.tagName !== 'INPUT') {
                const radio = this.querySelector('input[type="radio"]');
                if (radio) { radio.checked = true; radio.dispatchEvent(new Event('change', { bubbles: true })); }
            }
        });
    });

    // ── Generate image ───────────────────────────────────────────────────
    let capturedBlob = null;
    let capturedDataUrl = null;

    document.getElementById('btnGenerateImage')?.addEventListener('click', () => {
        const selectedOption = shareNameModal.querySelector('input[name="shareNameOption"]:checked');
        let name;
        if (selectedOption && selectedOption.value === 'plan') {
            name = planName || 'Treino Livre';
        } else {
            name = (document.getElementById('customShareName')?.value?.trim()) || 'Treino Livre';
        }

        const nameEl = document.getElementById('imgWorkoutName');
        if (nameEl) nameEl.textContent = name;

        const nameModalInstance = bootstrap.Modal.getInstance(shareNameModal);
        const imgModal = new bootstrap.Modal(document.getElementById('shareImageModal'));
        nameModalInstance?.hide();

        document.getElementById('generatingSpinner').style.display = '';
        document.getElementById('generatedImageWrapper').style.display = 'none';
        document.getElementById('btnDownloadImage').style.display = 'none';
        document.getElementById('btnShareImage').style.display = 'none';

        setTimeout(() => imgModal.show(), 300);

        setTimeout(() => {
            const tpl = document.getElementById('workoutImageTemplate');
            html2canvas(tpl, {
                scale: 2,
                useCORS: true,
                allowTaint: false,
                backgroundColor: null,
                width: 540,
                height: 720,
                logging: false
            }).then(canvas => {
                capturedDataUrl = canvas.toDataURL('image/png');
                document.getElementById('generatedImage').src = capturedDataUrl;

                document.getElementById('generatingSpinner').style.display = 'none';
                document.getElementById('generatedImageWrapper').style.display = '';
                document.getElementById('btnDownloadImage').style.display = '';

                if (navigator.share && navigator.canShare) {
                    canvas.toBlob(blob => {
                        capturedBlob = blob;
                        const file = new File([blob], 'treino-tupijua.png', { type: 'image/png' });
                        if (navigator.canShare({ files: [file] })) {
                            document.getElementById('btnShareImage').style.display = '';
                        }
                    }, 'image/png');
                }
            }).catch(err => {
                console.error('html2canvas error:', err);
                document.getElementById('generatingSpinner').innerHTML =
                    '<p class="text-danger"><i class="fas fa-exclamation-circle me-2"></i>Erro ao gerar a imagem.</p>';
            });
        }, 600);
    });

    // Download
    document.getElementById('btnDownloadImage')?.addEventListener('click', () => {
        if (!capturedDataUrl) return;
        const a = document.createElement('a');
        a.href = capturedDataUrl;
        a.download = 'treino-tupijua.png';
        a.click();
    });

    // Share via Web Share API
    document.getElementById('btnShareImage')?.addEventListener('click', async () => {
        if (!capturedBlob) return;
        const file = new File([capturedBlob], 'treino-tupijua.png', { type: 'image/png' });
        try {
            await navigator.share({ files: [file], title: 'Meu treino no TupiJua' });
        } catch (err) {
            if (err.name !== 'AbortError') console.error('Share error:', err);
        }
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initRegisterForm();
    initLoginForm();
    initUserProfile();
    initViewWorkout();
});
