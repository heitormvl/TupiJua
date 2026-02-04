/**
 * RestTimerUI - Interface visual para o temporizador de descanso
 * Gerencia cookies, UI e integração com RestTimer
 */
class RestTimerUI {
    constructor() {
        this.timer = new RestTimer();
        this.defaultSeconds = this.loadLastDuration();
        this.isExpanded = false;
        this.isActive = false;
        this.lastSaveTime = 0; // Timestamp da última gravação no localStorage
        this.saveThrottleMs = 1000; // Throttle de 1 segundo
        
        this.initializeUI();
        this.attachEventListeners();
        this.setupTimerCallbacks();
        this.setupVisibilityListener();
        this.restoreActiveTimer();
    }

    /**
     * Restaura timer ativo do localStorage
     */
    restoreActiveTimer() {
        const savedState = localStorage.getItem('restTimerState');
        if (!savedState) return;

        try {
            const state = JSON.parse(savedState);
            const now = Date.now();
            
            // Verifica se o timer ainda está válido
            if (state.endTime && state.endTime > now) {
                // Restaura o timer
                this.isActive = true;
                document.getElementById('restTimerBtn').style.display = 'none';
                document.getElementById('restTimerPill').classList.add('active');
                
                // Calcula tempo restante
                const remainingMs = state.endTime - now;
                const remainingSeconds = Math.ceil(remainingMs / 1000);
                
                // Restaura no objeto timer
                this.timer.endTime = state.endTime;
                this.timer.totalDuration = state.totalDuration;
                this.timer.remainingSeconds = remainingSeconds;
                this.timer.isPaused = false;
                
                // Reinicia o loop
                this.timer.tick();
                this.timer.intervalId = setInterval(() => this.timer.tick(), 100);
            } else if (state.isPaused && state.pausedRemaining > 0) {
                // Restaura timer pausado
                this.isActive = true;
                document.getElementById('restTimerBtn').style.display = 'none';
                document.getElementById('restTimerPill').classList.add('active');
                
                this.timer.isPaused = true;
                this.timer.pausedRemaining = state.pausedRemaining;
                this.timer.remainingSeconds = state.pausedRemaining;
                this.timer.totalDuration = state.totalDuration;
                
                // Atualiza display
                this.updatePillDisplay(state.pausedRemaining);
                this.updatePauseBtnIcon(true);
            } else {
                // Timer expirado, limpa localStorage
                localStorage.removeItem('restTimerState');
            }
        } catch (e) {
            console.error('Erro ao restaurar timer:', e);
            localStorage.removeItem('restTimerState');
        }
    }

    /**
     * Salva estado do timer no localStorage
     * @param {boolean} immediate - Se true, salva imediatamente ignorando throttle
     */
    saveTimerState(immediate = false) {
        if (!this.isActive) {
            localStorage.removeItem('restTimerState');
            this.lastSaveTime = 0;
            return;
        }

        const now = Date.now();
        
        // Se não é imediato, verifica throttle
        if (!immediate && (now - this.lastSaveTime) < this.saveThrottleMs) {
            return;
        }

        const state = {
            endTime: this.timer.endTime,
            totalDuration: this.timer.totalDuration,
            isPaused: this.timer.isPaused,
            pausedRemaining: this.timer.pausedRemaining
        };

        localStorage.setItem('restTimerState', JSON.stringify(state));
        this.lastSaveTime = now;
    }

    /**
     * Configura listener para salvar estado quando a visibilidade da página muda
     */
    setupVisibilityListener() {
        document.addEventListener('visibilitychange', () => {
            if (document.hidden && this.isActive) {
                // Salva imediatamente quando a aba fica oculta
                this.saveTimerState(true);
            }
        });
    }

    /**
     * Carrega a última duração do cookie (padrão: 60s)
     */
    loadLastDuration() {
        const saved = this.getCookie('restTimerDuration');

        if (!saved) {
            return 60;
        }

        const parsed = parseInt(saved, 10);

        if (!Number.isFinite(parsed) || parsed < 1) {
            return 60;
        }

        return parsed;
    }

    /**
     * Salva a duração no cookie
     */
    saveDuration(seconds) {
        this.setCookie('restTimerDuration', seconds, 365);
    }

    /**
     * Define um cookie
     */
    setCookie(name, value, days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        const expires = `expires=${date.toUTCString()}`;
        document.cookie = `${name}=${value};${expires};path=/;SameSite=Lax`;
    }

    /**
     * Obtém um cookie
     */
    getCookie(name) {
        const nameEQ = name + "=";
        const ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    /**
     * Inicializa a UI
     */
    initializeUI() {
        // Cria o HTML do componente
        const html = `
            <!-- Botão Flutuante -->
            <button id="restTimerBtn" class="rest-timer-fab" aria-label="Abrir temporizador de descanso">
                <i class="fas fa-clock"></i>
            </button>

            <!-- Modal de Configuração -->
            <div id="restTimerModal" class="rest-timer-modal">
                <div class="rest-timer-modal-content">
                    <div class="rest-timer-modal-header">
                        <h5 class="rest-timer-modal-title">
                            <i class="fas fa-stopwatch"></i> Temporizador de Descanso
                        </h5>
                        <button class="rest-timer-close" aria-label="Fechar">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <div class="rest-timer-modal-body">
                        <label for="restTimerInput" class="form-label">Tempo de descanso (segundos)</label>
                        <input type="number" 
                               id="restTimerInput" 
                               class="form-control" 
                               min="1" 
                               max="9999" 
                               value="${this.defaultSeconds}"
                               placeholder="Segundos">
                        <div class="rest-timer-presets">
                            <button class="btn btn-sm btn-outline-primary rest-timer-preset" data-seconds="30">30s</button>
                            <button class="btn btn-sm btn-outline-primary rest-timer-preset" data-seconds="60">1min</button>
                            <button class="btn btn-sm btn-outline-primary rest-timer-preset" data-seconds="90">1:30</button>
                            <button class="btn btn-sm btn-outline-primary rest-timer-preset" data-seconds="120">2min</button>
                            <button class="btn btn-sm btn-outline-primary rest-timer-preset" data-seconds="180">3min</button>
                        </div>
                    </div>
                    <div class="rest-timer-modal-footer">
                        <button id="restTimerStartBtn" class="btn btn-primary w-100">
                            <i class="fas fa-play"></i> Iniciar
                        </button>
                    </div>
                </div>
            </div>

            <!-- Pílula no Topo (ativa durante o timer) -->
            <div id="restTimerPill" class="rest-timer-pill">
                <div class="rest-timer-pill-progress"></div>
                <div class="rest-timer-pill-content">
                    <span class="rest-timer-pill-icon">
                        <i class="fas fa-stopwatch"></i>
                    </span>
                    <span id="restTimerPillTime" class="rest-timer-pill-time">00:00</span>
                    <div class="rest-timer-pill-controls">
                        <button id="restTimerPauseBtnPill" class="rest-timer-pill-btn" title="Pausar">
                            <i class="fas fa-pause"></i>
                        </button>
                        <button id="restTimerStopBtnPill" class="rest-timer-pill-btn" title="Parar">
                            <i class="fas fa-stop"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;

        // Adiciona ao body
        const container = document.createElement('div');
        container.innerHTML = html;
        document.body.appendChild(container);

        // Posiciona o botão conforme existência do card ativo
        this.updateButtonPosition();
        
        // Observa mudanças no DOM para reposicionar quando necessário
        this.observeActiveSessionCard();
    }

    /**
     * Atualiza a posição do botão conforme a existência do card ativo
     */
    updateButtonPosition() {
        const activeSessionCard = document.querySelector('.active-session-card');
        const restTimerBtn = document.getElementById('restTimerBtn');
        
        if (!restTimerBtn) return;

        if (activeSessionCard) {
            // Move para dentro do card
            restTimerBtn.classList.add('inside-active-session');
            const actionsDiv = activeSessionCard.querySelector('.active-session-actions');
            if (actionsDiv && !actionsDiv.contains(restTimerBtn)) {
                actionsDiv.insertBefore(restTimerBtn, actionsDiv.firstChild);
            }
        } else {
            // Volta para posição flutuante
            restTimerBtn.classList.remove('inside-active-session');
            if (restTimerBtn.parentElement.classList.contains('active-session-actions')) {
                document.body.appendChild(restTimerBtn);
            }
        }
    }

    /**
     * Observa mudanças no card de sessão ativa
     */
    observeActiveSessionCard() {
        const observer = new MutationObserver(() => {
            this.updateButtonPosition();
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    /**
     * Anexa event listeners
     */
    attachEventListeners() {
        // Botão flutuante
        document.getElementById('restTimerBtn').addEventListener('click', () => {
            this.toggleModal();
        });

        // Fechar modal
        document.querySelector('.rest-timer-close').addEventListener('click', () => {
            this.closeModal();
        });

        // Fechar modal ao clicar fora
        document.getElementById('restTimerModal').addEventListener('click', (e) => {
            if (e.target.id === 'restTimerModal') {
                this.closeModal();
            }
        });

        // Presets
        document.querySelectorAll('.rest-timer-preset').forEach(btn => {
            btn.addEventListener('click', () => {
                const seconds = parseInt(btn.dataset.seconds);
                document.getElementById('restTimerInput').value = seconds;
            });
        });

        // Botão iniciar
        document.getElementById('restTimerStartBtn').addEventListener('click', () => {
            this.startTimer();
        });

        // Botões na pílula
        document.getElementById('restTimerPauseBtnPill').addEventListener('click', () => {
            this.togglePause();
        });

        document.getElementById('restTimerStopBtnPill').addEventListener('click', () => {
            this.stopTimer();
        });
    }

    /**
     * Configura callbacks do timer
     */
    setupTimerCallbacks() {
        this.timer.onTick((remainingSeconds) => {
            this.updatePillDisplay(remainingSeconds);
        });

        this.timer.onComplete(() => {
            this.onTimerComplete();
        });
    }

    /**
     * Abre/fecha o modal
     */
    toggleModal() {
        if (this.isActive) return; // Não abre se timer está ativo
        
        this.isExpanded = !this.isExpanded;
        const modal = document.getElementById('restTimerModal');
        
        if (this.isExpanded) {
            modal.classList.add('show');
            document.getElementById('restTimerInput').focus();
        } else {
            modal.classList.remove('show');
        }
    }

    /**
     * Fecha o modal
     */
    closeModal() {
        this.isExpanded = false;
        document.getElementById('restTimerModal').classList.remove('show');
    }

    /**
     * Inicia o timer
     */
    startTimer() {
        const input = document.getElementById('restTimerInput');
        const seconds = parseInt(input.value);

        if (isNaN(seconds) || seconds < 1) {
            input.classList.add('is-invalid');
            setTimeout(() => input.classList.remove('is-invalid'), 2000);
            return;
        }

        this.saveDuration(seconds);
        this.closeModal();
        
        this.isActive = true;
        document.getElementById('restTimerBtn').style.display = 'none';
        document.getElementById('restTimerPill').classList.add('active');
        
        this.timer.start(seconds);
        this.saveTimerState(true); // Salva imediatamente ao iniciar
    }

    /**
     * Para o timer
     */
    stopTimer() {
        this.timer.stop();
        this.timer.stopSoundLoop();
        this.isActive = false;
        
        document.getElementById('restTimerBtn').style.display = 'flex';
        document.getElementById('restTimerPill').classList.remove('active');
        
        this.updatePauseBtnIcon(false);
        localStorage.removeItem('restTimerState');
    }

    /**
     * Pausa ou continua o timer de forma assíncrona.
     * Garante que o resume seja aguardado e evita cliques repetidos durante a transição.
     * @returns {Promise<void>}
     */
    async togglePause() {
        const pauseBtn = document.getElementById('restTimerPauseBtnPill');

        if (pauseBtn) {
            pauseBtn.disabled = true;
        }

        try {
            if (this.timer.isPausedState()) {
                await this.timer.resume();
                this.updatePauseBtnIcon(false);
            } else {
                this.timer.pause();
                this.updatePauseBtnIcon(true);
            }

            this.saveTimerState(true); // Salva imediatamente em pause/resume
        } catch (error) {
            // Mantém o estado visual atual em caso de erro ao retomar
            // para evitar indicar "rodando" quando o resume falhar.
            // eslint-disable-next-line no-console
            console.error('Erro ao retomar o temporizador de descanso:', error);
        } finally {
            if (pauseBtn) {
                pauseBtn.disabled = false;
            }
        }
    }

    /**
     * Atualiza o ícone do botão de pausa
     */
    updatePauseBtnIcon(isPaused) {
        const btn = document.getElementById('restTimerPauseBtnPill');
        const icon = btn.querySelector('i');
        
        if (isPaused) {
            icon.className = 'fas fa-play';
            btn.title = 'Continuar';
        } else {
            icon.className = 'fas fa-pause';
            btn.title = 'Pausar';
        }
    }

    /**
     * Atualiza o display da pílula
     */
    updatePillDisplay(remainingSeconds) {
        const timeDisplay = document.getElementById('restTimerPillTime');
        timeDisplay.textContent = RestTimer.formatTime(remainingSeconds);
        
        // Atualiza a barra de progresso (diminui de 100% para 0%)
        const totalSeconds = this.timer.totalDuration;
        const progress = (remainingSeconds / totalSeconds) * 100;
        document.querySelector('.rest-timer-pill-progress').style.width = `${progress}%`;
        
        // Salva estado com throttle (máximo 1x por segundo)
        if (this.isActive) {
            this.saveTimerState(false);
        }
    }

    /**
     * Callback quando o timer completa
     */
    onTimerComplete() {
        // Inicia loop de som contínuo
        this.timer.startSoundLoop();
        
        // Vibra se disponível
        if ('vibrate' in navigator) {
            navigator.vibrate([200, 100, 200, 100, 200]);
        }

        // Notificação do navegador
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification('TupiJua - Descanso Completo!', {
                body: 'Seu tempo de descanso acabou. Bora treinar! 💪',
                icon: '/images/icon-192.png',
                badge: '/images/icon-192.png',
                tag: 'rest-timer',
                requireInteraction: false
            });
        }

        // Mostra alerta visual com dismiss
        this.showCompletionAlert();
        
        // Limpa estado salvo
        localStorage.removeItem('restTimerState');
    }

    /**
     * Mostra um alerta visual de conclusão
     */
    showCompletionAlert() {
        const alert = document.createElement('div');
        alert.className = 'rest-timer-completion-alert';
        alert.innerHTML = `
            <div class="rest-timer-completion-content">
                <div class="rest-timer-completion-header">
                    <i class="fas fa-check-circle"></i>
                    <span>Descanso completo!</span>
                </div>
                <button class="btn btn-light rest-timer-dismiss-btn">
                    <i class="fas fa-hand-paper"></i> Encerrar
                </button>
            </div>
        `;
        
        document.body.appendChild(alert);
        
        // Botão de dismiss
        const dismissBtn = alert.querySelector('.rest-timer-dismiss-btn');
        dismissBtn.addEventListener('click', () => {
            this.dismissAlert(alert);
        });
        
        // Também dismiss ao clicar fora
        alert.addEventListener('click', (e) => {
            if (e.target === alert) {
                this.dismissAlert(alert);
            }
        });
        
        setTimeout(() => {
            alert.classList.add('show');
        }, 10);
    }

    /**
     * Dispensa o alerta e para o som
     */
    dismissAlert(alert) {
        // Para o loop de som
        this.timer.stopSoundLoop();
        
        // Para vibração se estiver ativa
        if ('vibrate' in navigator) {
            navigator.vibrate(0);
        }
        
        // Remove o alerta
        alert.classList.remove('show');
        setTimeout(() => {
            alert.remove();
            // Reseta a UI do timer
            this.stopTimer();
        }, 300);
    }

    /**
     * Solicita permissão para notificações
     */
    static async requestNotificationPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            await Notification.requestPermission();
        }
    }
}

// Inicializa quando o DOM estiver pronto
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.restTimerUI = new RestTimerUI();
    });
} else {
    window.restTimerUI = new RestTimerUI();
}
