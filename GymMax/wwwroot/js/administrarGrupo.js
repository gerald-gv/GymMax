
    const buscarUsuario =
    document.getElementById("buscarUsuario");
    const filtroPlan =
    document.getElementById("filtroPlan");
    const usuarios =
    document.querySelectorAll(".usuario-item");
    const sinResultados =
    document.getElementById("sinResultados");
    function filtrarUsuarios() {
            const texto =
    buscarUsuario.value
    .trim()
    .toLowerCase();
    const plan =
    filtroPlan.value
    .trim()
    .toLowerCase();
    let encontrados = 0;
    usuarios.forEach(function(usuario) {
                const nombre =
    usuario.dataset.nombre || "";
    const planUsuario =
    usuario.dataset.plan || "";
    const coincideNombre =
    nombre.includes(texto);
    const coincidePlan =
    !plan ||
    planUsuario === plan;
    if (coincideNombre && coincidePlan) {
        usuario.classList.remove("d-none");
    encontrados++;
                }
    else {

        usuario.classList.add("d-none");
                }
            });
    if (sinResultados) {
                if (encontrados === 0) {
        sinResultados.classList.remove("d-none");
                }
    else {
        sinResultados.classList.add("d-none");
                }
            }
        }
    if (buscarUsuario) {

        buscarUsuario.addEventListener(
            "input",
            filtrarUsuarios
        );

        }
    if (filtroPlan) {

        filtroPlan.addEventListener(
            "change",
            filtrarUsuarios
        );
        }