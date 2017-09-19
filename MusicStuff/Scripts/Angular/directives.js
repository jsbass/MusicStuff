var directives = angular.module('directives', []);

directives.directive("fileread", [function () {
    return {
        scope: {
            fileread: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var reader = new FileReader();
                reader.onload = function (loadEvent) {
                    scope.$apply(function () {
                        scope.fileread = loadEvent.target.result;
                    });
                }
                reader.readAsDataURL(changeEvent.target.files[0]);
            });
        }
    }
}]);

directives.directive('frequencyChart',
[
    'AudioAnalyser', function(analyser) {
        function link(scope, element, attrs) {
            console.log([scope, element, attrs]);
            var canvasCtx = element[0].getContext("2d");
            var width = element[0].width;
            var height = element[0].height;
            var timeout = null;

            element.on(
                '$destory',
                function() {
                    cancelRequestAnimationFrame(timeout);
                }
            );

            function updateChart(data) {
                var dataArray = data.frequencyData;
                var bufferLength = dataArray.length;
                canvasCtx.fillStyle = "#000000";
                canvasCtx.fillRect(0, 0, width, height);

                var barWidth = (width / bufferLength);
                var barHeight;

                for (var i = 0; i < bufferLength; i++) {
                    barHeight = dataArray[i] / 256 * height;

                    canvasCtx.fillStyle = 'rgb(256,100,100)';
                    canvasCtx.fillRect(i * barWidth, height - barHeight, barWidth, barHeight);
                }
            }

            analyser.addObserverCallback(updateChart);
        }

        return { link: link };
    }
]);