const buscarUsuario =
    document.getElementById("buscarUsuario");

const filtroPlan =
    document.getElementById("filtroPlan");

const usuarios =
    document.querySelectorAll(".usuario-item");

const checkboxes =
    document.querySelectorAll(".usuario-checkbox");

const contador =
    document.getElementById("contadorMiembros");


// FILTRAR USUARIOS
function filtrarUsuarios() {

    const texto =
        buscarUsuario.value
            .toLowerCase()
            .trim();

    const plan =
        filtroPlan.value
            .toLowerCase()
            .trim();

    let visibles = 0;

    usuarios.forEach(usuario => {

        const busqueda =
            usuario.dataset.busqueda
                .toLowerCase();

        const planUsuario =
            usuario.dataset.plan
                .toLowerCase();

        const coincideBusqueda =
            busqueda.includes(texto);

        const coincidePlan =
            !plan ||
            planUsuario === plan;

        const coincide =
            coincideBusqueda && coincidePlan;

        usuario.classList.toggle("d-none", !coincide);

        if (coincide) visibles++;

    });

    // Mostrar/ocultar mensaje de "sin resultados"
    document
        .getElementById("sinResultados")
        .classList.toggle("d-none", visibles > 0);
}


// BUSCAR POR NOMBRE, APELLIDO O CORREO
buscarUsuario.addEventListener(
    "input",
    filtrarUsuarios
);


// FILTRAR POR PLAN
filtroPlan.addEventListener(
    "change",
    filtrarUsuarios
);


// ACTUALIZAR SELECCIÓN
function actualizarSeleccion() {

    let cantidad = 0;

    checkboxes.forEach(checkbox => {

        const usuario =
            checkbox.closest(".usuario-item");

        const icono =
            usuario.querySelector(".check-icon");

        if (checkbox.checked) {

            cantidad++;

            usuario.classList.add("seleccionado");

            icono.classList.remove("d-none");

        }
        else {

            usuario.classList.remove("seleccionado");

            icono.classList.add("d-none");

        }

    });

    contador.textContent =
        cantidad === 1
            ? "1 seleccionado"
            : `${cantidad} seleccionados`;
}


// DETECTAR CAMBIOS EN LOS CHECKBOX
checkboxes.forEach(checkbox => {

    checkbox.addEventListener(
        "change",
        actualizarSeleccion
    );

});


actualizarSeleccion();
// EVITAR QUE ENTER ENVÍE EL FORMULARIO AL FILTRAR
buscarUsuario.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        e.preventDefault();
    }
});

filtroPlan.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        e.preventDefault();
    }
});