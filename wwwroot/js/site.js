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

    if (!usernameInput) return; // Exit if not on register page

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

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initRegisterForm();
    initLoginForm();
});
