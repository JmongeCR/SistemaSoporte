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
// Marcado: <div class="zona-archivos" data-zona-archivos data-extensiones=".pdf,.png" data-max-mb="10"
//               data-max-archivos="5" data-lista="#id-ul" data-error="#id-p"> <input type="file" multiple> ...
// Acumula archivos entre selecciones, permite quitarlos y avisa formato/tamaño antes de enviar.
// Es solo ayuda visual: el API vuelve a validar todo (formato, tamaño y contenido real).
document.querySelectorAll("[data-zona-archivos]").forEach((zona) => {
    const entrada = zona.querySelector("input[type=file]");
    const lista = document.querySelector(zona.dataset.lista);
    const error = document.querySelector(zona.dataset.error);
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

// Formularios que no deben enviarse dos veces (doble clic): <form data-envio-unico>.
document.querySelectorAll("form[data-envio-unico]").forEach((formulario) => {
    formulario.addEventListener("submit", (evento) => {
        if (formulario.dataset.enviando) {
            evento.preventDefault();
            return;
        }
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
