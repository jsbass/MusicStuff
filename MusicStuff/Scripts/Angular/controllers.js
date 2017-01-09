var controllers = angular.module('controllers', []);

controllers.controller('MidiDisplayController',['$scope','$http',
    function ($scope, $http) {
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
                
            }
            else {
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
                xhr.open("POST", "/parse");
                xhr.send(fd);
                console.log("Uploading...");
            } else {
                console.log("File not set");
            }
        }
    }
])