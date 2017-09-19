var controllers = angular.module('controllers', []);

controllers.controller('MidiDisplayController',
[
    '$scope', '$http',
    function($scope, $http) {
        var bind = this;
        this.midi = null;
        this.uploadProgress = -1;

        var xhrProgress = function(e) {
            if (e.lengthComputable) {
                bind.uploadProgress = Math.round(e.loaded * 100 / e.total);
            }
        }
        var xhrComplete = function(e) {
            if (e.target.status !== 200) {

            } else {
                bind.midi = JSON.parse(e.target.responseText);
                console.log(bind.midi);
            }
        }

        this.submitAndLoadFile = function() {
            if (bind.file != null) {
                var fd = new FormData(document.getElementById("uploadFile"));
                var xhr = new XMLHttpRequest();
                xhr.contentType = "";
                xhr.upload.addEventListener("progress", xhrProgress, false);
                xhr.addEventListener("load", xhrComplete, false);
                //xhr.addEventListener("error", xhrError, false);
                //xhr.addEventListener("abort", uploadAbort, false);
                xhr.open("POST", "parse/midi");
                xhr.send(fd);
                console.log("Uploading...");
            } else {
                console.log("File not set");
            }
        }
    }
]);
//controllers.controller('OscillatorController',
//[
//    'AudioAnalyser',
//    function (analyser) {
//        var bind = this;

//        this.note = 57;
//        this.noteString = analyser.getNoteString(this.note);
//        this.oscillatorFreq = analyser.getNoteFrequency(this.note);

//        var oscillator = {
//            isPlaying: false,
//            node: analyser.getCtx().createOscillator(),
//            start: function () {
//                if (this.node == null) {
//                    console.log("node not initialized");
//                }
//                this.node.start();
//                this.isStarted = true;
//            },
//            stop: function () {
//                if (this.node == null) {
//                    console.log("node not initialized");
//                }
//                this.node.stop();
//                this.isStarted = false;
//            }
//        }

//        this.updateDesiredNote = function () {
//            bind.oscillatorFreq = analyser.getNoteFrequency(bind.note);
//            bind.noteString = analyser.getNoteString(bind.note);
//            oscillator.node.frequency.value = oscillator.isPlaying ? bind.note : 0;
//        }

//        this.togglePlayback = function() {
//            if (oscillator.isPlaying) {
//                oscillator.node.frequency.value = bind.oscillatorFreq;
//            } else {
//                oscillator.node.frequency.value = 0;
//            }

//            oscillator.isPlaying = !oscillator.isPlaying;
//        }

//        this.getOsc = function () {
//            return oscillator;
//        }
//    }
//]);

window.oscillatorInput = {
    kind: "audioinput",
    deviceId: "oscillator",
    label: "Generated Signal",
    groupId: "oscillator"
};

controllers.controller('TunerController',
[
    '$scope',
    '$timeout',
    'AudioAnalyser',
    function($scope, $timeout, analyser) {
        
        var isPermitted = false;
        var bind = this;
        var stream = null;
        this.isPlaying = false;
        this.selectedSource = null;
        this.audioSources = [];
        this.currentNote = -1;
        this.currentNoteString = "";
        this.peakFrequency = -1;
        this.cents = -1;

        this.volume = 10;
        this.oscillatorFreq = 440;
        this.desiredNote = 57;
        this.desiredNoteString = analyser.getNoteString(this.desiredNote);
        var oscillator = {
            isStarted: false,
            node: analyser.getCtx().createOscillator(),
            start: function () {
                if (this.node == null) {
                    console.log("node not initialized");
                }
                this.node.frequency.value = bind.oscillatorFreq;
                this.isStarted = true;
            },
            stop: function () {
                if (this.node == null) {
                    console.log("node not initialized");
                }
                this.node.frequency.value = 0;
                this.isStarted = false;
            }
        }

        oscillator.node.start();

        this.updateDesiredNote = function () {
            bind.oscillatorFreq = analyser.getNoteFrequency(bind.desiredNote);
            bind.desiredNoteString = analyser.getNoteString(bind.desiredNote);
            oscillator.node.frequency.value = oscillator.isStarted ? bind.oscillatorFreq : 0;
        }

        this.updateSources = function () {
            if (!isPermitted) {
                navigator.getUserMedia({ audio: true },
                    function (stream) {
                        if(stream != null) stream.getTracks()[0].stop();
                        isPermitted = true;
                        navigator.mediaDevices.enumerateDevices()
                            .then(function(devices) {
                                bind.audioSources = devices.filter(function(d) {
                                    return d.kind === "audioinput";
                                });
                                bind.audioSources.push(window.oscillatorInput);
                                bind.selectedSource = window.oscillatorInput.deviceId;
                                $scope.$apply();
                            });
                    },
                    function() {
                        console.error("microphone permissions denied");
                    });
            } else {
                navigator.mediaDevices.enumerateDevices()
                    .then(function(devices) {
                        bind.audioSources = devices.filter(function(d) {
                            return d.kind === "audioinput";
                        });
                        bind.selectedSource = bind.selectedSource === null ? bind.audioSources[0].deviceId : bind.selectedSource;
                        $scope.$apply();
                    });
            }
            
            //$scope.$apply();
        }

        this.toggleRecording = function() {
            bind.isPlaying = !bind.isPlaying;
            if (bind.isPlaying) {
                analyser.start();
                if (bind.selectedSource === window.oscillatorInput.deviceId) {
                    analyser.connectTo(oscillator.node);
                    oscillator.start();
                } else {
                    navigator.getUserMedia(
                        { audio: true, devideId: bind.selectedSource },
                        function(s) {
                            stream = s;
                            var node = analyser.getCtx().createMediaStreamSource(stream);
                            analyser.connectTo(node);
                        },
                        console.error);
                }
            } else {
                analyser.stop();
                if (stream != null) stream.getTracks()[0].stop();
                oscillator.stop();
            }
        }

        function updateInfo(data) {
            var max = 0;
            var maxIndex = 0;
            for (var i = 0; i < data.frequencyData.length; i++) {
                if (data.frequencyData[i] > max) {
                    max = data.frequencyData[i];
                    maxIndex = i;
                }
            }

            bind.peakFrequency = data.sampleRate / data.fftSize * maxIndex;
            bind.currentNote = analyser.getFrequencyNote(bind.peakFrequency);
            bind.currentNoteString = analyser.getNoteString(bind.currentNote);
            bind.cents = analyser.getCents(bind.peakFrequency, bind.currentNote);
            bind.cents = bind.cents <= 0 ? bind.cents : "+" + bind.cents;
            $timeout(function() { $scope.$apply(); });
            //console.log([bind.peakFrequency + " Hz", bind.currentNote, bind.currentNoteString, bind.cents].join(", "));
        }

        this.updateSource = function() {
            bind.toggleRecording();
            bind.toggleRecording();
        }

        this.updateSources();
        this.updateDesiredNote();
        analyser.addObserverCallback(updateInfo);
        analyser.connect(analyser.getCtx().destination);
    }
]);