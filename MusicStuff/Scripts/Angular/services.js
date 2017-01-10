var services = angular.module('services', []);

services.service('AudioAnalyser', function () {
    var timeout = null;
    var bind = this;
    var ctx = new AudioContext();
    var observers = [];
    var analyser = ctx.createAnalyser();

    var isPaused = true;

    var frequencyData = new Uint8Array(0);
    var timeData = new Uint8Array(0);

    this.connectTo = function(node) {
        node.connect(analyser);
    }

    this.connect = function(node) {
        analyser.connect(node);
    }

    this.getCtx = function() {
        return ctx;
    }

    function getData() {
        analyser.getByteFrequencyData(frequencyData);
        analyser.getByteTimeDomainData(timeData);
        notifyObservers();
        timeout = setTimeout(getData, 17);
    }

    this.start = function() {
        if (!isPaused) return;
        isPaused = false;
        getData();
    }

    this.stop = function () {
        if (isPaused) return;
        clearTimeout(timeout);
        isPaused = true;
    }

    this.addObserverCallback = function(f) {
        observers.push(f);
    }

    function notifyObservers() {
        var data = {
            timeData: timeData,
            frequencyData: frequencyData,
            sampleRate: ctx.sampleRate,
            fftSize: analyser.fftSize
        };
        angular.forEach(observers, function(callback) { callback(data); });
    }

    this.setFFTSize = function (size) {
        var wasStarted = !isPaused;
        bind.stop();
        analyser.fftSize = size;
        frequencyData = new Uint8Array(analyser.frequencyBinCount);
        timeData = new Uint8Array(analyser.frequencyBinCount);
        if (wasStarted) bind.start();
    }

    var notes = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B'];

    this.getNoteString = function(n) {
        var letter = notes[n % 12];
        var octave = Math.floor(n / 12);

        return letter + octave;
    }

    this.getNoteFrequency = function(n) {
        return 440 * Math.pow(2, (n - 57) / 12);
    }

    this.getFrequencyNote = function(f) {
        return Math.round(12 * Math.log2(f / 440) + 57);
    }

    this.getCents = function(f, n) {
        return Math.floor(1200 * Math.log2(f / bind.getNoteFrequency(n)));
    }

    this.setFFTSize(2048);
});