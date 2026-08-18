const chatMessages = document.getElementById("chatMessages");
const mensajeInput = document.getElementById("mensajeInput");
const formEnviarMensaje = document.getElementById("formEnviarMensaje");
// DESPLAZAR AL FINAL
function desplazarAlFinal() {
    chatMessages.scrollTop = chatMessages.scrollHeight;
}
desplazarAlFinal();
// AJUSTAR ALTURA DEL TEXTAREA
mensajeInput.addEventListener("input", function () {
    this.style.height = "auto";
    this.style.height = this.scrollHeight + "px";
});
// CONEXIÓN SIGNALR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();
// RECIBIR MENSAJE
connection.on("RecibirMensaje", function (mensaje) {

    // Si pertenece a otra conversación, ignorarlo
    if (mensaje.conversacionId !== conversacionId) {
        return;
    }

    // Eliminar mensaje de "chat vacío"
    const emptyChat = document.getElementById("emptyChat");

    if (emptyChat) {
        emptyChat.remove();
    }

    const esMio = mensaje.usuarioId === usuarioId;

    // Fila del mensaje
    const row = document.createElement("div");

    row.id = "mensaje-" + mensaje.mensajeId;

    row.className =
        "message-row " +
        (esMio ? "mine" : "theirs");

    // Burbuja
    const bubble = document.createElement("div");

    bubble.className = "message-bubble";

    // Nombre del usuario
    if (!esMio) {

        const nombre = document.createElement("div");

        nombre.className = "message-user";

        nombre.textContent = mensaje.nombreUsuario;

        bubble.appendChild(nombre);
    }

    // Contenedor del contenido
    const contenido = document.createElement("div");

    contenido.className = "message-content";

    // Mensaje eliminado
    if (mensaje.eliminado) {

        const eliminado = document.createElement("div");

        eliminado.className =
            "fst-italic opacity-75";

        eliminado.textContent =
            "Este mensaje fue eliminado.";

        contenido.appendChild(eliminado);

    }
    else {

        // Texto del mensaje
        const texto = document.createElement("div");

        texto.className = "message-text";

        texto.textContent = mensaje.contenido;

        contenido.appendChild(texto);

        // Indicador de editado
        if (mensaje.editado) {

            const editado = document.createElement("span");

            editado.className =
                "small fst-italic opacity-75 mensaje-editado";

            editado.textContent = " (editado)";

            contenido.appendChild(editado);
        }
    }

    bubble.appendChild(contenido);

    // Fecha
    const fecha = document.createElement("div");

    fecha.className =
        "message-date text-end";

    const fechaMensaje =
        new Date(mensaje.fechaEnvio);

    fecha.textContent =
        fechaMensaje.toLocaleDateString("es-PE") +
        " " +
        fechaMensaje.toLocaleTimeString(
            "es-PE",
            {
                hour: "2-digit",
                minute: "2-digit"
            }
        );

    bubble.appendChild(fecha);

    // Botones para mensajes propios
    if (esMio && !mensaje.eliminado) {

        const acciones = document.createElement("div");

        acciones.className =
            "message-actions mt-2 d-flex justify-content-end gap-2";

        // Botón editar
        const botonEditar =
            document.createElement("button");

        botonEditar.type = "button";

        botonEditar.className =
            "btn btn-sm btn-light rounded-circle shadow-sm d-flex align-items-center justify-content-center";

        botonEditar.style.width = "30px";
        botonEditar.style.height = "30px";

        botonEditar.title =
            "Editar mensaje";

        botonEditar.innerHTML =
            '<i class="bi bi-pencil-fill text-dark"></i>';

        botonEditar.addEventListener(
            "click",
            function () {
                editarMensaje(mensaje.mensajeId);
            }
        );

        // Botón eliminar
        const botonEliminar =
            document.createElement("button");

        botonEliminar.type = "button";

        botonEliminar.className =
            "btn btn-sm btn-light rounded-circle shadow-sm d-flex align-items-center justify-content-center";

        botonEliminar.style.width = "30px";
        botonEliminar.style.height = "30px";

        botonEliminar.title =
            "Eliminar mensaje";

        botonEliminar.innerHTML =
            '<i class="bi bi-trash-fill text-danger"></i>';

        botonEliminar.addEventListener(
            "click",
            function () {
                eliminarMensaje(mensaje.mensajeId);
            }
        );

        acciones.appendChild(botonEditar);
        acciones.appendChild(botonEliminar);

        bubble.appendChild(acciones);
    }
    // Construir mensaje
    row.appendChild(bubble);

    chatMessages.appendChild(row);

    // Desplazar al último mensaje
    desplazarAlFinal();
});
connection.on("MensajeEditado", function (mensaje) {

    const elemento = document.getElementById(
        "mensaje-" + mensaje.mensajeId
    );
    if (!elemento) {
        return;
    }
    const texto = elemento.querySelector(".message-text");
    if (texto) {
        texto.textContent = mensaje.contenido;
    }
    // Buscar si ya existe la etiqueta "(editado)"
    const contenido = elemento.querySelector(".message-content");
    if (contenido && !contenido.querySelector(".mensaje-editado")) {

        const editado = document.createElement("span");
        editado.className =
            "small fst-italic opacity-75 mensaje-editado";
        editado.textContent = " (editado)";
        contenido.appendChild(editado);
    }
});
connection.on("MensajeEliminado", function (mensaje) {
    const elemento = document.getElementById(
        "mensaje-" + mensaje.mensajeId
    );
    if (!elemento) {
        return;
    }
    const contenido = elemento.querySelector(".message-content");
    if (!contenido) {
        return;
    }
    contenido.innerHTML = `
                <div class="fst-italic opacity-75">
                    Este mensaje fue eliminado.
                </div>`;
    // Eliminar los botones de editar/eliminar
    const acciones =
        elemento.querySelector(".message-actions");
    if (acciones) {
        acciones.remove();
    }
});
async function editarMensaje(mensajeId) {
    const elemento = document.getElementById(
        "mensaje-" + mensajeId
    );
    if (!elemento) {
        return;
    }
    const textoElemento =
        elemento.querySelector(".message-text");
    if (!textoElemento) {
        return;
    }
    const contenidoActual =
        textoElemento.textContent;
    const nuevoContenido = prompt(
        "Editar mensaje:",
        contenidoActual
    );
    if (nuevoContenido === null) {
        return;
    }
    if (!nuevoContenido.trim()) {
        alert("El mensaje no puede estar vacío.");
        return;
    }
    try {
        await connection.invoke(
            "EditarMensaje",
            mensajeId,
            nuevoContenido
        );
    }
    catch (error) {
        console.error(
            "Error al editar mensaje:",
            error
        );
        alert(error.message || "No se pudo editar el mensaje.");
    }
}
async function eliminarMensaje(mensajeId) {
    const confirmar = confirm(
        "¿Estás seguro de que deseas eliminar este mensaje?"
    );
    if (!confirmar) {
        return;
    }
    try {
        await connection.invoke(
            "EliminarMensaje",
            mensajeId
        );
    }
    catch (error) {
        console.error(
            "Error al eliminar mensaje:",
            error
        );
        alert(
            error.message ||
            "No se pudo eliminar el mensaje."
        );
    }
}
// INICIAR CONEXIÓN
async function iniciarChat() {
    try {
        await connection.start();
        console.log("SignalR conectado.");
        await connection.invoke(
            "UnirseAConversacion",
            conversacionId
        );
        console.log(
            "Unido a la conversación:",
            conversacionId
        );
    }
    catch (error) {
        console.error(
            "Error al conectar con SignalR:",
            error
        );
    }
}
iniciarChat();
// ENVIAR MENSAJE
async function enviarMensaje() {
    const contenido =
        mensajeInput.value.trim();
    if (!contenido) {
        return;
    }
    try {
        await connection.invoke(
            "EnviarMensaje",
            conversacionId,
            contenido
        );
        mensajeInput.value = "";
        mensajeInput.style.height = "auto";
    }
    catch (error) {
        console.error(
            "Error al enviar mensaje:",
            error
        );
        alert(
            "No se pudo enviar el mensaje."
        );
    }
}
// FORMULARIO
formEnviarMensaje.addEventListener(
    "submit",
    function (event) {
        event.preventDefault();
        enviarMensaje();
    }
);
mensajeInput.addEventListener(
    "keydown",
    function (event) {
        if (
            event.key === "Enter" &&
            !event.shiftKey
        ) {
            event.preventDefault();
            enviarMensaje();
        }
    }
);