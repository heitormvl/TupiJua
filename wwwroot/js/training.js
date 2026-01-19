/// <reference path="../lib/jquery/dist/jquery.js" />

/**
 * Training page functionality
 */

// Initialize Training Index page
function initTrainingIndex() {
    const heroCard = document.querySelector('.hero-card');
    
    if (!heroCard) return; // Exit if not on training index page
    
    // Animate stats cards on scroll
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });

    // Animate timeline items
    const timelineItems = document.querySelectorAll('.timeline-item');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, observerOptions);

    timelineItems.forEach(item => {
        observer.observe(item);
    });
}

// Initialize Add Exercise page
function initAddExercise() {
    const exerciseSelect = document.getElementById('exerciseSelect');
    
    if (!exerciseSelect) return; // Exit if not on add exercise page
    
    // Check if exercise is pre-selected (from plan)
    if (exerciseSelect.value && exerciseSelect.options[exerciseSelect.selectedIndex]) {
        loadLastExerciseData(exerciseSelect.value);
    }
    
    // Load last exercise data when exercise is selected
    exerciseSelect.addEventListener('change', function() {
        const exerciseId = this.value;
        if (exerciseId) {
            loadLastExerciseData(exerciseId);
        } else {
            closeLastExerciseInfo();
        }
    });

    // Initialize form validation
    const form = document.getElementById('exerciseForm');
    if (form) {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    }

    // Add animation to form sections
    const formSections = document.querySelectorAll('.form-section');
    formSections.forEach((section, index) => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(20px)';
        setTimeout(() => {
            section.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
            section.style.opacity = '1';
            section.style.transform = 'translateY(0)';
        }, index * 80);
    });
}

// Load last exercise data via AJAX
function loadLastExerciseData(exerciseId) {
    fetch(`/Training/GetLastLoggedExercise?exerciseId=${exerciseId}`)
        .then(response => response.json())
        .then(data => {
            if (data) {
                displayLastExerciseInfo(data);
                
                // Auto-fill form with last exercise data if ShouldIncreaseLoad is true
                if (data.shouldIncreaseLoad) {
                    document.getElementById('Sets').value = data.sets;
                    document.getElementById('Reps').value = data.reps;
                    
                    // Suggest increased weight (5% increase or +0.5kg, whichever is greater)
                    const suggestedWeight = Math.max(
                        Math.round((data.weight * 1.05) * 2) / 2, // 5% increase, rounded to .5
                        data.weight + 0.5
                    );
                    document.getElementById('Weight').value = suggestedWeight.toFixed(2);
                    
                    document.getElementById('RestSeconds').value = data.restSeconds;
                    document.getElementById('ShouldIncreaseLoad').checked = true;
                } else {
                    // Just pre-fill with same values
                    document.getElementById('Sets').value = data.sets;
                    document.getElementById('Reps').value = data.reps;
                    document.getElementById('Weight').value = data.weight.toFixed(2);
                    document.getElementById('RestSeconds').value = data.restSeconds;
                }
            }
        })
        .catch(error => {
            console.error('Erro ao carregar dados do último exercício:', error);
        });
}

// Display last exercise info card
function displayLastExerciseInfo(data) {
    const infoCard = document.getElementById('lastExerciseInfo');
    const dataContainer = document.getElementById('lastExerciseData');
    
    if (!infoCard || !dataContainer) return;

    const increaseFlag = data.shouldIncreaseLoad 
        ? '<span class="badge bg-warning text-dark ms-2"><i class="fas fa-arrow-up me-1"></i>Aumentar</span>' 
        : '';
    
    dataContainer.innerHTML = `
        <div class="row g-2">
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat">
                    <small class="text-muted">Séries</small>
                    <strong>${data.sets}</strong>
                </div>
            </div>
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat">
                    <small class="text-muted">Reps</small>
                    <strong>${data.reps}</strong>
                </div>
            </div>
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat">
                    <small class="text-muted">Carga</small>
                    <strong>${data.weight.toFixed(1)} kg</strong>
                </div>
            </div>
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat">
                    <small class="text-muted">Descanso</small>
                    <strong>${data.restSeconds}s</strong>
                </div>
            </div>
        </div>
        ${data.observation ? `<p class="small text-muted mt-2 mb-0"><i class="fas fa-comment me-1"></i>${data.observation}</p>` : ''}
        ${increaseFlag}
    `;
    
    infoCard.style.display = 'none';
    setTimeout(() => {
        infoCard.style.display = 'block';
        infoCard.style.animation = 'slideDown 0.4s ease forwards';
    }, 100);
}

// Close last exercise info card
function closeLastExerciseInfo() {
    const infoCard = document.getElementById('lastExerciseInfo');
    if (infoCard) {
        infoCard.style.animation = 'slideUp 0.3s ease forwards';
        setTimeout(() => {
            infoCard.style.display = 'none';
        }, 300);
    }
}

// Increment number input value
function incrementValue(inputId, step) {
    const input = document.getElementById(inputId);
    if (!input) return;
    
    const currentValue = parseFloat(input.value) || 0;
    const maxValue = parseFloat(input.max) || Infinity;
    const newValue = Math.min(currentValue + step, maxValue);
    
    input.value = inputId === 'Weight' ? newValue.toFixed(2) : newValue;
    
    // Add animation feedback
    input.classList.add('value-changed');
    setTimeout(() => input.classList.remove('value-changed'), 300);
}

// Decrement number input value
function decrementValue(inputId, step) {
    const input = document.getElementById(inputId);
    if (!input) return;
    
    const currentValue = parseFloat(input.value) || 0;
    const minValue = parseFloat(input.min) || 0;
    const newValue = Math.max(currentValue - step, minValue);
    
    input.value = inputId === 'Weight' ? newValue.toFixed(2) : newValue;
    
    // Add animation feedback
    input.classList.add('value-changed');
    setTimeout(() => input.classList.remove('value-changed'), 300);
}

// View workout details (placeholder - implement modal or navigate to details page)
function viewWorkoutDetails(sessionId) {
    // TODO: Implement workout details view
    alert(`Visualizar detalhes da sessão ${sessionId} (implementar)`);
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initTrainingIndex();
    initAddExercise();
});

// Keyframes for animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateY(-20px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @keyframes slideUp {
        from {
            opacity: 1;
            transform: translateY(0);
        }
        to {
            opacity: 0;
            transform: translateY(-20px);
        }
    }

    .value-changed {
        animation: pulse 0.3s ease;
    }

    @keyframes pulse {
        0%, 100% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.05);
        }
    }
`;
document.head.appendChild(style);
