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

    // Handle free training form submission
    initFreeTrainingValidation();
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
                    
                    document.getElementById('RestTime').value = data.restTime;
                    document.getElementById('RestInMinutes').value = data.restInMinutes;
                    updateRestUnitToggle();
                    document.getElementById('ShouldIncreaseLoad').checked = true;
                } else {
                    // Just pre-fill with same values
                    document.getElementById('Sets').value = data.sets;
                    document.getElementById('Reps').value = data.reps;
                    document.getElementById('Weight').value = data.weight.toFixed(2);
                    document.getElementById('RestTime').value = data.restTime;
                    document.getElementById('RestInMinutes').value = data.restInMinutes;
                    updateRestUnitToggle();
                    document.getElementById('ShouldIncreaseLoad').checked = false;
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
    
    const restUnit = data.restInMinutes ? 'min' : 'seg';
    
    dataContainer.innerHTML = `
        <div class="row g-3">
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat text-center">
                    <small class="text-muted d-block mb-1">Séries</small>
                    <strong class="text-success">${data.sets}</strong>
                </div>
            </div>
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat text-center">
                    <small class="text-muted d-block mb-1">Reps</small>
                    <strong class="text-success">${data.reps}</strong>
                </div>
            </div>
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat text-center">
                    <small class="text-muted d-block mb-1">Carga</small>
                    <strong class="text-success">${data.weight.toFixed(1)} kg</strong>
                </div>
            </div>
            <div class="col-6 col-sm-3">
                <div class="last-exercise-stat text-center">
                    <small class="text-muted d-block mb-1">Descanso</small>
                    <strong class="text-success">${data.restTime} ${restUnit}</strong>
                </div>
            </div>
        </div>
        ${data.observation ? `<p class="small text-muted mt-3 mb-0 text-center"><i class="fas fa-comment me-1"></i>${data.observation}</p>` : ''}
        ${increaseFlag ? `<div class="text-center mt-2">${increaseFlag}</div>` : ''}
    `;
    
    infoCard.style.display = 'none';
    setTimeout(() => {
        infoCard.style.display = 'block';
        infoCard.style.animation = 'slideDown 0.4s ease forwards';
    }, 100);
}

// Toggle card minimize/expand
function toggleCardMinimize(cardId) {
    const card = document.getElementById(cardId);
    const icon = document.getElementById(`${cardId}-icon`);
    const content = document.getElementById(`${cardId}-content`) || card.querySelector('.last-exercise-data');
    
    if (!card || !icon || !content) return;
    
    const isMinimized = content.classList.contains('minimized');
    
    if (isMinimized) {
        // Expand
        content.classList.remove('minimized');
        content.style.maxHeight = content.scrollHeight + 'px';
        icon.classList.remove('fa-chevron-down');
        icon.classList.add('fa-chevron-up');
        
        // Remove max-height after animation
        setTimeout(() => {
            if (!content.classList.contains('minimized')) {
                content.style.maxHeight = '';
            }
        }, 300);
    } else {
        // Set max-height for animation
        content.style.maxHeight = content.scrollHeight + 'px';
        
        // Force reflow
        content.offsetHeight;
        
        // Minimize
        content.classList.add('minimized');
        icon.classList.remove('fa-chevron-up');
        icon.classList.add('fa-chevron-down');
    }
}

// Close last exercise info card (kept for backward compatibility)
function closeLastExerciseInfo() {
    toggleCardMinimize('lastExerciseInfo');
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

// Set rest time unit (seconds or minutes)
function setRestUnit(isMinutes) {
    const restInMinutesInput = document.getElementById('RestInMinutes');
    const restUnitSeg = document.getElementById('restUnitSeg');
    const restUnitMin = document.getElementById('restUnitMin');
    const restTimeInput = document.getElementById('RestTime');
    
    if (!restInMinutesInput || !restUnitSeg || !restUnitMin || !restTimeInput) return;
    
    const currentIsMinutes = restInMinutesInput.value === 'true';
    
    // If already in the desired unit, do nothing
    if (currentIsMinutes === isMinutes) return;
    
    const currentValue = parseInt(restTimeInput.value) || 0;
    
    if (isMinutes) {
        // Convert from seconds to minutes
        restInMinutesInput.value = 'true';
        restTimeInput.value = Math.round(currentValue / 60);
        restTimeInput.step = 1;
        restUnitSeg.classList.remove('active');
        restUnitMin.classList.add('active');
    } else {
        // Convert from minutes to seconds
        restInMinutesInput.value = 'false';
        restTimeInput.value = currentValue * 60;
        restTimeInput.step = 15;
        restUnitMin.classList.remove('active');
        restUnitSeg.classList.add('active');
    }
}

// Update the rest unit toggle button state
function updateRestUnitToggle() {
    const restInMinutesInput = document.getElementById('RestInMinutes');
    const restUnitSeg = document.getElementById('restUnitSeg');
    const restUnitMin = document.getElementById('restUnitMin');
    const restTimeInput = document.getElementById('RestTime');
    
    if (!restInMinutesInput || !restUnitSeg || !restUnitMin || !restTimeInput) return;
    
    const isMinutes = restInMinutesInput.value === 'true';
    
    if (isMinutes) {
        restUnitSeg.classList.remove('active');
        restUnitMin.classList.add('active');
        restTimeInput.step = 1;
    } else {
        restUnitMin.classList.remove('active');
        restUnitSeg.classList.add('active');
        restTimeInput.step = 15;
    }
}

// Increment rest time value
function incrementRestTime() {
    const restTimeInput = document.getElementById('RestTime');
    const restInMinutesInput = document.getElementById('RestInMinutes');
    
    if (!restTimeInput || !restInMinutesInput) return;
    
    const isMinutes = restInMinutesInput.value === 'true';
    const step = isMinutes ? 1 : 15;
    const currentValue = parseInt(restTimeInput.value) || 0;
    const maxValue = parseInt(restTimeInput.max) || 3600;
    const newValue = Math.min(currentValue + step, maxValue);
    
    restTimeInput.value = newValue;
    
    // Add animation feedback
    restTimeInput.classList.add('value-changed');
    setTimeout(() => restTimeInput.classList.remove('value-changed'), 300);
}

// Decrement rest time value
function decrementRestTime() {
    const restTimeInput = document.getElementById('RestTime');
    const restInMinutesInput = document.getElementById('RestInMinutes');
    
    if (!restTimeInput || !restInMinutesInput) return;
    
    const isMinutes = restInMinutesInput.value === 'true';
    const step = isMinutes ? 1 : 15;
    const currentValue = parseInt(restTimeInput.value) || 0;
    const minValue = parseInt(restTimeInput.min) || 0;
    const newValue = Math.max(currentValue - step, minValue);
    
    restTimeInput.value = newValue;
    
    // Add animation feedback
    restTimeInput.classList.add('value-changed');
    setTimeout(() => restTimeInput.classList.remove('value-changed'), 300);
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
    initWorkoutPlanFormsValidation();
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

/**
 * View workout details
 * @param {number} sessionId - The workout session ID
 */
function viewWorkoutDetails(sessionId) {
    window.location.href = `/Training/ViewWorkout?sessionId=${sessionId}`;
}

/**
 * Initialize free training form validation
 */
function initFreeTrainingValidation() {
    const freeTrainingForm = document.getElementById('startFreeTrainingForm');
    if (freeTrainingForm) {
        freeTrainingForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            await validateAndSubmitTrainingForm(freeTrainingForm);
        });
    }
}

/**
 * Initialize workout plan forms validation
 */
function initWorkoutPlanFormsValidation() {
    const planForms = document.querySelectorAll('.start-plan-form');
    planForms.forEach(form => {
        form.addEventListener('submit', async function(e) {
            e.preventDefault();
            await validateAndSubmitTrainingForm(form);
        });
    });
}

/**
 * Common validation logic for training forms
 * @param {HTMLFormElement} form - The form element to validate and submit
 */
async function validateAndSubmitTrainingForm(form) {
    try {
        const response = await fetch('/Training/CheckTodayWorkout');
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data.hasWorkout) {
            // Show confirmation modal
            showMultipleWorkoutsModal(form);
        } else {
            // No workout today, proceed normally
            form.submit();
        }
    } catch (error) {
        console.error('Erro ao verificar treinos do dia:', error);
        // In case of error, allow the form to submit
        form.submit();
    }
}

/**
 * Show the multiple workouts confirmation modal
 * @param {HTMLFormElement} form - The form to submit on confirmation
 */
function showMultipleWorkoutsModal(form) {
    const modalElement = document.getElementById('multipleWorkoutsModal');
    const confirmButton = document.getElementById('confirmStartTraining');
    
    if (!modalElement || !confirmButton) return;
    
    const modal = new bootstrap.Modal(modalElement);
    
    // Remove any existing event listeners by cloning the button
    const newConfirmButton = confirmButton.cloneNode(true);
    confirmButton.parentNode.replaceChild(newConfirmButton, confirmButton);
    
    // Add new event listener
    newConfirmButton.addEventListener('click', function() {
        modal.hide();
        form.submit();
    });
    
    modal.show();
}
