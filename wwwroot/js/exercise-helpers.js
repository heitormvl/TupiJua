/**
 * Funções auxiliares para manipulação de exercícios
 */

/**
 * Escapa HTML para prevenir XSS
 * @param {string} text - Texto a ser escapado
 * @returns {string} Texto com caracteres especiais escapados
 */
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Agrupa exercícios por grupo muscular primário
 * @param {Array} exercisesList - Lista de exercícios com seus grupos musculares
 * @returns {Object} Objeto com categorias como chaves e arrays de exercícios como valores
 */
function groupExercisesByPrimaryMuscle(exercisesList) {
    const grouped = {};
    
    exercisesList.forEach(exercise => {
        const primaryMuscle = exercise.MuscleGroups.find(mg => mg.IsPrimary);
        const category = primaryMuscle ? primaryMuscle.Name : 'Sem Categoria';
        
        if (!grouped[category]) {
            grouped[category] = [];
        }
        grouped[category].push(exercise);
    });
    
    // Sort categories alphabetically and exercises within each category
    const sortedCategories = Object.keys(grouped).sort();
    const result = {};
    sortedCategories.forEach(category => {
        result[category] = grouped[category].sort((a, b) => a.Name.localeCompare(b.Name));
    });
    
    return result;
}

/**
 * Gera HTML de opções de exercícios com optgroups
 * @param {Array} exercisesList - Lista de exercícios com seus grupos musculares
 * @returns {string} HTML com optgroups e options
 */
function generateExerciseOptionsHTML(exercisesList) {
    const groupedExercises = groupExercisesByPrimaryMuscle(exercisesList);
    let html = '<option value="">Selecione um exercício</option>';
    
    Object.keys(groupedExercises).forEach(category => {
        html += `<optgroup label="${escapeHtml(category)}">`;
        groupedExercises[category].forEach(exercise => {
            html += `<option value="${exercise.Id}">${escapeHtml(exercise.Name)}</option>`;
        });
        html += '</optgroup>';
    });
    
    return html;
}
