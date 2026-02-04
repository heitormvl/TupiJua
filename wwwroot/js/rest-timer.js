/**
 * RestTimer - Temporizador de descanso resiliente à suspensão do navegador
 * 
 * Usa performance.now() para cálculo baseado em timestamp final
 * Implementa Silent Audio Loop para manter execução ativa
 * Usa Web Audio API para notificação sonora
 */
class RestTimer {
    constructor() {
        this.endTime = null;
        this.totalDuration = 0;
        this.remainingSeconds = 0;
        this.isPaused = false;
        this.pausedAt = null;
        this.pausedRemaining = 0;
        this.intervalId = null;
        this.onTickCallback = null;
        this.onCompleteCallback = null;
        this.soundLoopId = null;
        
        // Web Audio API para Silent Audio Loop e notificação
        this.audioContext = null;
        this.silentNode = null;
        this.initAudioContext();
    }

    /**
     * Inicializa o AudioContext e cria o Silent Audio Loop
     */
    initAudioContext() {
        try {
            window.AudioContext = window.AudioContext || window.webkitAudioContext;
            this.audioContext = new AudioContext();
            this.createSilentLoop();
        } catch (e) {
            console.warn('Web Audio API não disponível:', e);
        }
    }

    /**
     * Cria um loop de áudio silencioso para manter o script ativo
     * 
     * Trade-off: Usa MediaStreamAudioDestinationNode para garantir silêncio total
     * sem emitir áudio pelos alto-falantes. Isso mantém o AudioContext ativo
     * evitando suspensão pelo navegador, mas consome recursos mesmo sem output audível.
     * 
     * Alternativa considerada: gain=0.00001 com destination direto, mas pode ser
     * audível em alguns dispositivos/fones sensíveis.
     */
    createSilentLoop() {
        if (!this.audioContext) return;

        try {
            // Usa MediaStreamAudioDestinationNode para garantir silêncio real
            // sem conectar aos alto-falantes do dispositivo
            const silentDestination = this.audioContext.createMediaStreamDestination();
            this._setupOscillator(silentDestination, 0);
        } catch (e) {
            // Fallback: se MediaStreamAudioDestinationNode não estiver disponível,
            // usa o método anterior com volume muito baixo (gain=0.00001).
            // IMPORTANTE: Este fallback pode ser audível em alguns dispositivos/fones sensíveis.
            console.warn('MediaStreamAudioDestinationNode não disponível, usando fallback com gain=0.00001:', e);
            this._setupOscillator(this.audioContext.destination, 0.00001);
        }
    }

    /**
     * Configura o oscillator silencioso com o destino e ganho especificados
     * @param {AudioNode} destination - Nó de destino do áudio
     * @param {number} gainValue - Valor do ganho (0 para silêncio real)
     */
    _setupOscillator(destination, gainValue) {
        const oscillator = this.audioContext.createOscillator();
        const gainNode = this.audioContext.createGain();
        
        oscillator.connect(gainNode);
        gainNode.connect(destination);
        
        gainNode.gain.value = gainValue;
        
        oscillator.start();
        this.silentNode = oscillator;
    }

    /**
     * Resume o AudioContext (necessário após interação do usuário)
     */
    async resumeAudioContext() {
        if (this.audioContext && this.audioContext.state === 'suspended') {
            try {
                await this.audioContext.resume();
            } catch (e) {
                console.warn('Erro ao resumir AudioContext:', e);
            }
        }
    }

    /**
     * Gera um bipe sonoro usando Web Audio API
     * @param {number} frequency - Frequência em Hz
     * @param {number} duration - Duração em segundos
     */
    playBeep(frequency = 800, duration = 0.3) {
        if (!this.audioContext) return;

        try {
            const oscillator = this.audioContext.createOscillator();
            const gainNode = this.audioContext.createGain();
            
            oscillator.connect(gainNode);
            gainNode.connect(this.audioContext.destination);
            
            oscillator.frequency.value = frequency;
            oscillator.type = 'sine';
            
            // Envelope ADSR para som mais agradável
            const now = this.audioContext.currentTime;
            gainNode.gain.setValueAtTime(0, now);
            gainNode.gain.linearRampToValueAtTime(0.3, now + 0.01);
            gainNode.gain.exponentialRampToValueAtTime(0.01, now + duration);
            
            oscillator.start(now);
            oscillator.stop(now + duration);
        } catch (e) {
            console.warn('Erro ao tocar bipe:', e);
        }
    }

    /**
     * Toca uma sequência de bipes ao final do timer
     */
    playNotificationSound() {
        // Sequência de 3 bipes
        this.playBeep(800, 0.2);
        setTimeout(() => this.playBeep(800, 0.2), 250);
        setTimeout(() => this.playBeep(1000, 0.4), 500);
    }

    /**
     * Inicia loop contínuo de som até ser interrompido
     */
    startSoundLoop() {
        this.stopSoundLoop(); // Para qualquer loop anterior
        
        const playLoop = () => {
            this.playNotificationSound();
        };
        
        // Toca imediatamente
        playLoop();
        
        // Continua tocando a cada 2 segundos
        this.soundLoopId = setInterval(playLoop, 2000);
    }

    /**
     * Para o loop de som
     */
    stopSoundLoop() {
        if (this.soundLoopId) {
            clearInterval(this.soundLoopId);
            this.soundLoopId = null;
        }
    }

    /**
     * Inicia o temporizador
     * @param {number} seconds - Duração em segundos
     */
    async start(seconds) {
        if (this.intervalId) {
            this.stop();
        }

        await this.resumeAudioContext();

        this.totalDuration = seconds;
        this.remainingSeconds = seconds;
        this.isPaused = false;
        this.pausedAt = null;
        this.pausedRemaining = 0;
        
        // Define o tempo final baseado no timestamp absoluto
        const now = Date.now();
        this.endTime = now + (seconds * 1000);
        
        // Inicia o loop de atualização
        this.tick();
        this.intervalId = setInterval(() => this.tick(), 100);
    }

    /**
     * Pausa o temporizador
     */
    pause() {
        if (!this.intervalId || this.isPaused) return;

        this.isPaused = true;
        this.pausedAt = Date.now();
        this.pausedRemaining = this.remainingSeconds;
        
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
    }

    /**
     * Resume o temporizador após pausa
     */
    async resume() {
        if (!this.isPaused) return;

        await this.resumeAudioContext();

        this.isPaused = false;
        const now = Date.now();
        this.endTime = now + (this.pausedRemaining * 1000);
        
        this.tick();
        this.intervalId = setInterval(() => this.tick(), 100);
    }

    /**
     * Para o temporizador completamente
     */
    stop() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        
        this.endTime = null;
        this.remainingSeconds = 0;
        this.isPaused = false;
        this.pausedAt = null;
        this.pausedRemaining = 0;
    }

    /**
     * Atualização do timer (resiliente à suspensão)
     */
    tick() {
        if (this.isPaused || !this.endTime) return;

        const now = Date.now();
        const remaining = this.endTime - now;

        if (remaining <= 0) {
            this.remainingSeconds = 0;
            this.complete();
            return;
        }

        this.remainingSeconds = Math.ceil(remaining / 1000);

        if (this.onTickCallback) {
            this.onTickCallback(this.remainingSeconds);
        }
    }

    /**
     * Callback quando o timer completa
     */
    complete() {
        this.stop();
        this.playNotificationSound();
        
        if (this.onCompleteCallback) {
            this.onCompleteCallback();
        }
    }

    /**
     * Define o callback para atualização da UI
     * @param {Function} callback - Função que recebe remainingSeconds
     */
    onTick(callback) {
        this.onTickCallback = callback;
    }

    /**
     * Define o callback para conclusão do timer
     * @param {Function} callback - Função executada ao completar
     */
    onComplete(callback) {
        this.onCompleteCallback = callback;
    }

    /**
     * Retorna o tempo restante em segundos
     * @returns {number}
     */
    getRemaining() {
        return this.remainingSeconds;
    }

    /**
     * Retorna se o timer está pausado
     * @returns {boolean}
     */
    isPausedState() {
        return this.isPaused;
    }

    /**
     * Retorna se o timer está rodando
     * @returns {boolean}
     */
    isRunning() {
        return this.intervalId !== null && !this.isPaused;
    }

    /**
     * Formata segundos para MM:SS
     * @param {number} seconds
     * @returns {string}
     */
    static formatTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }

    /**
     * Limpa recursos (chamado ao destruir o componente)
     */
    destroy() {
        this.stop();
        this.stopSoundLoop();
        
        if (this.silentNode) {
            this.silentNode.stop();
            this.silentNode = null;
        }
        
        if (this.audioContext) {
            this.audioContext.close();
            this.audioContext = null;
        }
    }
}

// Exporta para uso global
window.RestTimer = RestTimer;
