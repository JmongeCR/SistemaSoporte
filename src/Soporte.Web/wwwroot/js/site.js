// Botón para mostrar u ocultar la contraseña (pantallas de cuenta).
// Marcado: <button type="button" data-mostrar-contrasena="#id-del-campo">
document.querySelectorAll("[data-mostrar-contrasena]").forEach((boton) => {
    const campo = document.querySelector(boton.dataset.mostrarContrasena);
    boton.addEventListener("click", () => {
        const mostrar = campo.type === "password";
        campo.type = mostrar ? "text" : "password";
        boton.setAttribute("aria-pressed", String(mostrar));
        boton.setAttribute("aria-label", mostrar ? "Ocultar contraseña" : "Mostrar contraseña");
    });
});

// Zona de archivos reutilizable (HU-04 esc. 3, HU-11).
// Extrae en función para poder re-inicializar en contenido inyectado dinámicamente.
function initZonaArchivos(raiz) {
    raiz.querySelectorAll("[data-zona-archivos]:not([data-za-init])").forEach((zona) => {
        zona.dataset.zaInit = "1";
        const entrada = zona.querySelector("input[type=file]");
        const lista = raiz.querySelector(zona.dataset.lista) ?? document.querySelector(zona.dataset.lista);
        const error = raiz.querySelector(zona.dataset.error) ?? document.querySelector(zona.dataset.error);
        const extensiones = zona.dataset.extensiones.split(",");
        const maxBytes = Number(zona.dataset.maxMb) * 1024 * 1024;
        const maxArchivos = Number(zona.dataset.maxArchivos);
        let seleccionados = [];

        const tamano = (bytes) => bytes < 1024 * 1024 ? `${Math.ceil(bytes / 1024)} KB` : `${(bytes / 1024 / 1024).toFixed(1)} MB`;

        const sincronizar = () => {
            const transferencia = new DataTransfer();
            seleccionados.forEach((archivo) => transferencia.items.add(archivo));
            entrada.files = transferencia.files;

            lista.replaceChildren(...seleccionados.map((archivo, indice) => {
                const item = document.createElement("li");
                item.innerHTML = `<div><span class="archivo-nombre"></span><span class="archivo-tamano">${tamano(archivo.size)}</span></div>
                                  <button type="button" class="boton-quitar">✕ Quitar</button>`;
                item.querySelector(".archivo-nombre").textContent = archivo.name;
                item.querySelector("button").addEventListener("click", () => { seleccionados.splice(indice, 1); sincronizar(); });
                return item;
            }));
        };

        entrada.addEventListener("change", () => {
            const errores = [];
            for (const archivo of entrada.files) {
                const extension = archivo.name.slice(archivo.name.lastIndexOf(".")).toLowerCase();
                if (!extensiones.includes(extension)) errores.push(`"${archivo.name}" no es un formato permitido (válidos: PDF, PNG o JPG).`);
                else if (archivo.size > maxBytes) errores.push(`"${archivo.name}" supera el límite de ${zona.dataset.maxMb} MB.`);
                else if (seleccionados.length >= maxArchivos) errores.push(`Puede adjuntar hasta ${maxArchivos} archivos.`);
                else seleccionados.push(archivo);
            }
            error.textContent = [...new Set(errores)].join(" ");
            sincronizar();
        });

        ["dragenter", "dragover"].forEach((evento) => zona.addEventListener(evento, () => zona.classList.add("arrastrando")));
        ["dragleave", "drop"].forEach((evento) => zona.addEventListener(evento, () => zona.classList.remove("arrastrando")));
    });
}
initZonaArchivos(document);

// Formularios que no deben enviarse dos veces (doble clic): <form data-envio-unico>.
function initEnvioUnico(raiz) {
    raiz.querySelectorAll("form[data-envio-unico]:not([data-eu-init])").forEach((formulario) => {
        formulario.dataset.euInit = "1";
        formulario.addEventListener("submit", (evento) => {
            if (formulario.dataset.enviando) { evento.preventDefault(); return; }
            // Si la validación del navegador (jQuery Validate) encuentra errores, no se bloquea el botón.
            if (window.jQuery && jQuery(formulario).data("validator") && !jQuery(formulario).valid()) return;
            formulario.dataset.enviando = "1";
            formulario.querySelectorAll("button[type=submit]").forEach((boton) => {
                boton.disabled = true;
                boton.dataset.textoOriginal = boton.textContent;
                boton.textContent = "Enviando…";
            });
        });
    });
}
initEnvioUnico(document);

// Al volver con el botón "atrás" (caché del navegador), los botones quedan habilitados otra vez.
window.addEventListener("pageshow", () => {
    document.querySelectorAll("form[data-envio-unico]").forEach((formulario) => {
        delete formulario.dataset.enviando;
        formulario.querySelectorAll("button[type=submit]").forEach((boton) => {
            boton.disabled = false;
            if (boton.dataset.textoOriginal) boton.textContent = boton.dataset.textoOriginal;
        });
    });
});

// Campana de notificaciones: panel desplegable y contador actualizado periódicamente sin recargar.
(() => {
    const campana = document.querySelector("[data-campana]");
    if (!campana) return;

    const boton = campana.querySelector(".campana-boton");
    const contador = campana.querySelector(".campana-contador");
    const panel = campana.querySelector(".panel-notificaciones");
    const consultaAutomatica = { headers: { "X-Consulta-Automatica": "1" } };

    const pintar = (noLeidas) => {
        contador.textContent = noLeidas > 99 ? "99+" : String(noLeidas);
        contador.hidden = noLeidas === 0;
        boton.setAttribute("aria-label", noLeidas ? `Notificaciones: ${noLeidas} sin leer` : "Notificaciones");
    };

    const cargarPanel = async () => {
        panel.innerHTML = '<p class="panel-estado">Cargando…</p>';
        try {
            const respuesta = await fetch(campana.dataset.urlPanel);
            if (!respuesta.ok || respuesta.redirected) throw new Error();
            panel.innerHTML = await respuesta.text();
        } catch {
            panel.innerHTML = '<p class="panel-estado">No se pudieron cargar las notificaciones. <a class="enlace" href="/Notificaciones">Reintentar</a></p>';
        }
    };

    const cerrar = () => { panel.hidden = true; boton.setAttribute("aria-expanded", "false"); };

    boton.addEventListener("click", () => {
        const abrir = panel.hidden;
        panel.hidden = !abrir;
        boton.setAttribute("aria-expanded", String(abrir));
        if (abrir) cargarPanel();
    });
    document.addEventListener("click", (evento) => { if (!campana.contains(evento.target)) cerrar(); });
    document.addEventListener("keydown", (evento) => { if (evento.key === "Escape") cerrar(); });

    const consultar = async () => {
        if (document.hidden) return;
        try {
            const respuesta = await fetch(campana.dataset.urlResumen, consultaAutomatica);
            if (!respuesta.ok || respuesta.redirected) return;   // sesión vencida: el próximo clic llevará al login
            const datos = await respuesta.json();
            if (typeof datos.noLeidas === "number") pintar(datos.noLeidas);
        } catch { /* sin conexión: se reintenta en la próxima vuelta */ }
    };

    setInterval(consultar, Number(campana.dataset.intervalo) * 1000);
    document.addEventListener("visibilitychange", consultar);
})();

// Modal "Cambiar rol" (HU-15). Los datos del usuario vienen en los atributos data-* del botón.
(() => {
    const modal = document.getElementById("modal-cambiar-rol");
    if (!modal) return;

    const avisos = JSON.parse(document.getElementById("avisos-rol").textContent);
    const campo = (nombre) => modal.querySelector(`[data-campo="${nombre}"]`);
    const selector = modal.querySelector("select[name=nuevoRolId]");
    let rolActual = "";

    const actualizarAviso = () => {
        campo("aviso").textContent = avisos[selector.value] ?? "";
        // Guardar el mismo rol no tendría efecto.
        campo("guardar").disabled = selector.value === rolActual;
    };

    document.querySelectorAll("[data-cambiar-rol]").forEach((boton) => {
        boton.addEventListener("click", () => {
            const datos = boton.dataset;
            rolActual = datos.rolId;
            campo("usuario-id").value = datos.usuarioId;
            campo("rol-actual-id").value = datos.rolId;
            campo("nombre").textContent = datos.nombre;
            campo("correo").textContent = datos.correo;
            campo("iniciales").textContent = datos.nombre.split(" ").slice(0, 2).map((p) => p[0]).join("").toUpperCase();
            campo("iniciales").className = `avatar avatar-grande ${datos.claseRol}`;
            campo("rol-actual").textContent = datos.rol;
            campo("rol-actual").className = `pill-rol ${datos.claseRol}`;
            selector.value = datos.rolId;
            actualizarAviso();
            modal.showModal();
        });
    });

    selector.addEventListener("change", actualizarAviso);
    // Cancelar, ✕ o Escape cierran sin enviar nada (HU-15 esc. 3).
    modal.querySelectorAll("[data-cerrar-modal]").forEach((boton) => boton.addEventListener("click", () => modal.close()));
})();

// Avisos breves que se ocultan solos (ej. "Rol actualizado correctamente").
document.querySelectorAll("[data-autocerrar]").forEach((aviso) => {
    setTimeout(() => { aviso.style.opacity = "0"; setTimeout(() => aviso.remove(), 400); }, 4000);
});

// Modal "Crear ticket". Se carga el formulario de /Tickets/Crear?modal vía fetch.
(() => {
    const modal = document.getElementById("modal-crear-ticket");
    if (!modal) return;

    const contenido = document.getElementById("modal-crear-contenido");
    let cargado = false;

    const abrir = async () => {
        modal.showModal();
        if (cargado) return;
        contenido.innerHTML = '<div class="modal-cargando">Cargando formulario…</div>';
        try {
            const respuesta = await fetch("/Tickets/Crear?modal");
            if (!respuesta.ok || respuesta.redirected) throw new Error();
            const html = await respuesta.text();
            const doc = new DOMParser().parseFromString(html, "text/html");
            const bloque = doc.querySelector(".diseno-formulario");
            if (!bloque) throw new Error("formulario no encontrado");
            contenido.innerHTML = "";
            contenido.appendChild(document.importNode(bloque, true));
            // Re-inicializar comportamientos en el contenido inyectado.
            initZonaArchivos(contenido);
            initEnvioUnico(contenido);
            cargado = true;
        } catch {
            contenido.innerHTML = '<div class="modal-cargando">No se pudo cargar el formulario. <a class="enlace" href="/Tickets/Crear">Ir a la página</a></div>';
        }
    };

    const cerrar = () => modal.close();

    document.querySelectorAll("[data-abrir-modal-crear]").forEach((btn) => btn.addEventListener("click", abrir));
    // Usar delegación de eventos para el botón "Cancelar" inyectado dinámicamente.
    modal.addEventListener("click", (e) => {
        if (e.target.closest("[data-cerrar-modal-crear]")) { cerrar(); return; }
        if (e.target === modal) cerrar();
    });
})();

// Tema oscuro / claro. Persiste en localStorage.
(() => {
    const raiz = document.getElementById("html-raiz");
    const boton = document.getElementById("btn-tema");
    if (!raiz || !boton) return;

    const toggleTema = () => {
        const oscuro = raiz.getAttribute("data-tema") !== "oscuro";
        if (oscuro) {
            raiz.setAttribute("data-tema", "oscuro");
            localStorage.setItem("tema", "oscuro");
        } else {
            raiz.removeAttribute("data-tema");
            localStorage.removeItem("tema");
        }
    };

    boton.addEventListener("click", toggleTema);
})();

// Panel split usuarios (Roles/Usuarios): clic en usuario → navega a la misma página con UsuarioId en la URL.
(() => {
    const filas = document.querySelectorAll("[data-usuario-fila]");
    if (!filas.length) return;

    filas.forEach((fila) => {
        fila.addEventListener("click", () => {
            const url = new URL(window.location.href);
            url.searchParams.set("UsuarioId", fila.dataset.usuarioId);
            window.location.href = url.toString();
        });
    });
})();

// Modal "Cambiar estado" (lista de tickets).
(() => {
    const modal = document.getElementById("modal-cambiar-estado");
    if (!modal) return;

    const campo = (n) => modal.querySelector(`[data-campo-estado="${n}"]`);

    document.querySelectorAll("[data-cambiar-estado]").forEach((btn) => {
        btn.addEventListener("click", () => {
            campo("ticket-id").value = btn.dataset.ticketId;
            campo("asunto").textContent = `#${btn.dataset.ticketId} — ${btn.dataset.asunto}`;
            modal.querySelector("select[name='nuevoEstadoId']").value = btn.dataset.estadoId;
            modal.showModal();
        });
    });

    modal.querySelectorAll("[data-cerrar-modal]").forEach((btn) => btn.addEventListener("click", () => modal.close()));
    modal.addEventListener("click", (e) => { if (e.target === modal) modal.close(); });
})();

// Gráficas Chart.js del dashboard (si existen los canvas y los datos).
(() => {
    const datosSrc = document.getElementById("datos-graficas");
    if (!datosSrc || typeof Chart === "undefined") return;

    let datos;
    try { datos = JSON.parse(datosSrc.textContent); } catch { return; }

    const paleta = ["#2161EB", "#B87805", "#1A8C40", "#BF2E24"];
    const opcionesBase = {
        responsive: true,
        maintainAspectRatio: true,
        plugins: { legend: { position: "bottom", labels: { font: { family: "Inter, system-ui", size: 12 }, padding: 16 } } }
    };

    // Dona — estados
    const canvasEstados = document.getElementById("grafica-estados");
    if (canvasEstados && datos.estados) {
        const e = datos.estados;
        new Chart(canvasEstados, {
            type: "doughnut",
            data: {
                labels: ["Abiertos", "En progreso", "Resueltos hoy", "SLA vencido"],
                datasets: [{ data: [e.abiertos, e.enProgreso, e.resueltos, e.slaVencido], backgroundColor: paleta, borderWidth: 0 }]
            },
            options: { ...opcionesBase, cutout: "68%" }
        });
    }

    // Barras — carga por agente
    const canvasAgentes = document.getElementById("grafica-agentes");
    if (canvasAgentes && Array.isArray(datos.agentes) && datos.agentes.length) {
        new Chart(canvasAgentes, {
            type: "bar",
            data: {
                labels: datos.agentes.map((a) => a.Agente.split(" ")[0]),
                datasets: [
                    { label: "Abiertos", data: datos.agentes.map((a) => a.Abiertos), backgroundColor: "#2161EB", borderRadius: 4 },
                    { label: "En progreso", data: datos.agentes.map((a) => a.EnProgreso), backgroundColor: "#B87805", borderRadius: 4 }
                ]
            },
            options: {
                ...opcionesBase,
                maintainAspectRatio: false,
                scales: {
                    x: { stacked: false, grid: { display: false }, ticks: { font: { family: "Inter, system-ui", size: 11 } } },
                    y: { beginAtZero: true, ticks: { stepSize: 1, font: { family: "Inter, system-ui", size: 11 } }, grid: { color: "#E5E8ED" } }
                }
            }
        });
    }
})();

// ---------- Menú de usuario ----------
(function () {
    const contenedor = document.getElementById("menu-usuario");
    const boton = document.getElementById("menu-usuario-toggle");
    if (!contenedor || !boton) return;

    const abrir = () => { contenedor.classList.add("abierto"); boton.setAttribute("aria-expanded", "true"); };
    const cerrar = () => { contenedor.classList.remove("abierto"); boton.setAttribute("aria-expanded", "false"); };

    boton.addEventListener("click", () => contenedor.classList.contains("abierto") ? cerrar() : abrir());
    document.addEventListener("click", (e) => { if (!contenedor.contains(e.target)) cerrar(); });
    document.addEventListener("keydown", (e) => { if (e.key === "Escape") cerrar(); });
})();
