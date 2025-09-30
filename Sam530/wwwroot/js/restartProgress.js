window.restartProgress = {
    startPolling: function (healthUrl) {
        let progress = 0;
        let bar = document.getElementById("progressBar");

        function updateBar(val) {
            if (bar) {
                bar.style.width = val + "%";
                bar.innerText = val + "%";
            }
        }

        updateBar(0);

        let interval = setInterval(() => {
            progress = Math.min(progress + 2, 95); // Subir hasta 95% máx
            updateBar(progress);
            if (progress > 60) { //A partir del primer minuto comprueba si se ha iniciado el servicio
                fetch(healthUrl, { cache: "no-cache" })
                    .then(r => {
                        if (r.ok) {
                            clearInterval(interval);
                            updateBar(100);
                            setTimeout(() => window.location.href = "/", 1000);
                        }
                    })
                    .catch(_ => {
                        // aún no responde, seguimos
                    });
            }
        }, 2000); // cada 2s
    }
};