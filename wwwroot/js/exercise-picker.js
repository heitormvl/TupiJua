/**
 * ExercisePicker - Componente de seleção de exercícios com filtros por grupo muscular.
 * Substitui o <select> padrão por uma UI mobile-first com busca e tags de filtragem.
 */
class ExercisePicker {
    /**
     * @param {HTMLSelectElement} selectEl - Elemento select original (será ocultado)
     * @param {Array} exercises - Lista de exercícios com propriedades Id, Name e MuscleGroups
     */
    constructor(selectEl, exercises) {
        this.selectEl = selectEl;
        this.exercises = exercises;
        this.activeFilter = '';
        this.searchText = '';
        this.isOpen = false;

        this._init();
    }

    /** @private */
    _init() {
        this.categories = [...new Set(
            this.exercises.map(ex => {
                const pm = ex.MuscleGroups.find(mg => mg.IsPrimary);
                return pm ? pm.Name : 'Sem Categoria';
            })
        )].sort((a, b) => a.localeCompare(b));

        this._createUI();
        this._bindEvents();

        if (this.selectEl.value) {
            const ex = this.exercises.find(e => String(e.Id) === String(this.selectEl.value));
            if (ex) this._setDisplay(ex.Name);
        }
    }

    /** @private */
    _createUI() {
        const wrapper = document.createElement('div');
        wrapper.className = 'exercise-picker-wrapper';
        this.selectEl.parentNode.insertBefore(wrapper, this.selectEl);
        wrapper.appendChild(this.selectEl);
        this.selectEl.classList.add('exercise-picker-hidden-select');

        const tagsHtml = this.categories.map(cat =>
            `<button type="button" class="picker-tag" data-filter="${_epEscape(cat)}">${_epEscape(cat)}</button>`
        ).join('');

        wrapper.insertAdjacentHTML('afterbegin', `
            <div class="exercise-picker-ui">
                <div class="exercise-picker-trigger" tabindex="0" role="combobox" aria-expanded="false" aria-haspopup="listbox">
                    <i class="fas fa-dumbbell picker-dumbbell-icon"></i>
                    <span class="picker-display-text picker-placeholder">Selecione um exercício</span>
                    <i class="fas fa-chevron-down picker-chevron"></i>
                </div>
                <div class="exercise-picker-panel" style="display:none">
                    <div class="picker-search-wrap">
                        <i class="fas fa-search picker-search-icon"></i>
                        <input type="text" class="picker-search-input" placeholder="Buscar exercício..." autocomplete="off">
                    </div>
                    <div class="picker-muscle-tags">
                        <button type="button" class="picker-tag active" data-filter="">Todos</button>
                        ${tagsHtml}
                    </div>
                    <div class="picker-exercises-list" role="listbox">
                        ${this._buildListHTML()}
                    </div>
                </div>
            </div>
        `);

        this.wrapper = wrapper;
        this.trigger = wrapper.querySelector('.exercise-picker-trigger');
        this.panel = wrapper.querySelector('.exercise-picker-panel');
        this.searchInput = wrapper.querySelector('.picker-search-input');
        this.listEl = wrapper.querySelector('.picker-exercises-list');
        this.displayText = wrapper.querySelector('.picker-display-text');
        this.chevron = wrapper.querySelector('.picker-chevron');
    }

    /** @private */
    _buildListHTML() {
        let items = this.exercises;

        if (this.activeFilter) {
            items = items.filter(ex => {
                const pm = ex.MuscleGroups.find(mg => mg.IsPrimary);
                return pm && pm.Name === this.activeFilter;
            });
        }

        if (this.searchText) {
            const q = this.searchText.toLowerCase();
            items = items.filter(ex => ex.Name.toLowerCase().includes(q));
        }

        items = [...items].sort((a, b) => a.Name.localeCompare(b.Name));

        if (!items.length) {
            return '<div class="picker-empty-state"><i class="fas fa-search me-2"></i>Nenhum exercício encontrado</div>';
        }

        const currentId = String(this.selectEl.value);
        return items.map(ex => {
            const pm = ex.MuscleGroups.find(mg => mg.IsPrimary);
            const cat = pm ? pm.Name : '';
            const isSelected = String(ex.Id) === currentId;
            return `<div class="picker-exercise-item${isSelected ? ' selected' : ''}" data-id="${ex.Id}" role="option" aria-selected="${isSelected}">
                <span class="picker-exercise-name">${_epEscape(ex.Name)}</span>
                ${cat ? `<span class="picker-exercise-category">${_epEscape(cat)}</span>` : ''}
            </div>`;
        }).join('');
    }

    /** @private */
    _refreshList() {
        this.listEl.innerHTML = this._buildListHTML();
        this._bindListItemEvents();
    }

    /** @private */
    _bindEvents() {
        this.trigger.addEventListener('click', () => this.toggle());
        this.trigger.addEventListener('keydown', e => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.toggle();
            }
        });

        this.searchInput.addEventListener('input', e => {
            this.searchText = e.target.value;
            this._refreshList();
        });

        this.wrapper.querySelectorAll('.picker-muscle-tags .picker-tag').forEach(btn => {
            btn.addEventListener('click', () => {
                this.wrapper.querySelectorAll('.picker-muscle-tags .picker-tag')
                    .forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                this.activeFilter = btn.dataset.filter;
                this._refreshList();
            });
        });

        document.addEventListener('click', e => {
            if (this.isOpen && !this.wrapper.contains(e.target)) this.close();
        });

        this._bindListItemEvents();
    }

    /** @private */
    _bindListItemEvents() {
        this.listEl.querySelectorAll('.picker-exercise-item').forEach(item => {
            item.addEventListener('click', () => {
                const ex = this.exercises.find(e => String(e.Id) === item.dataset.id);
                if (ex) this._select(ex);
            });
        });
    }

    /** @private */
    _select(exercise) {
        this.selectEl.value = exercise.Id;
        this._setDisplay(exercise.Name);
        this.close();
        this._refreshList();
        this.selectEl.dispatchEvent(new Event('change', { bubbles: true }));
    }

    /** @private */
    _setDisplay(name) {
        this.displayText.textContent = name;
        this.displayText.classList.remove('picker-placeholder');
        this.trigger.classList.add('has-selection');
    }

    /** Abre o painel de seleção. */
    open() {
        this.isOpen = true;
        this.panel.style.display = 'block';
        this.trigger.classList.add('is-open');
        this.trigger.setAttribute('aria-expanded', 'true');
        this.chevron.classList.add('rotated');
        requestAnimationFrame(() => this.searchInput.focus());
    }

    /** Fecha o painel de seleção. */
    close() {
        this.isOpen = false;
        this.panel.style.display = 'none';
        this.trigger.classList.remove('is-open');
        this.trigger.setAttribute('aria-expanded', 'false');
        this.chevron.classList.remove('rotated');
    }

    /** Alterna o painel de seleção. */
    toggle() {
        if (this.isOpen) this.close();
        else this.open();
    }
}

/**
 * Escapa caracteres especiais HTML.
 * @param {string} text
 * @returns {string}
 */
function _epEscape(text) {
    const d = document.createElement('div');
    d.textContent = String(text);
    return d.innerHTML;
}

/**
 * Inicializa o ExercisePicker em um elemento select.
 * @param {HTMLSelectElement} selectEl - Elemento select original
 * @param {Array} exercisesList - Lista de exercícios com MuscleGroups
 * @returns {ExercisePicker|null}
 */
function initExercisePicker(selectEl, exercisesList) {
    if (!selectEl || selectEl._epInitialized) return null;
    selectEl._epInitialized = true;
    return new ExercisePicker(selectEl, exercisesList);
}
