using UnityEngine;
using UnityEngine.EventSystems;

// Controla qué cubo se juega. Pónselo a un objeto vacío (ej. "SelectorCubos").
// En cada botón: OnClick -> arrastra este objeto -> SelectorCubos.Seleccionar(int)
// y escribe 0 para el primer cubo, 1 para el segundo, etc.
//
// Funciona con cualquier cubo, tenga el script que tenga:
// - Si tiene CuboEscalador: lo activa/desactiva con su casilla "Activo".
// - Si tiene otro script de movimiento (ej. Movement): lo apaga/prende.
public class SelectorCubos : MonoBehaviour
{
    [Tooltip("Arrastra aquí tus cubos DESDE LA HIERARCHY (en el orden de los botones)")]
    public GameObject[] cubos;
    [Tooltip("La Main Camera con el script CamaraSigue")]
    public CamaraSigue camara;
    [Tooltip("Cubo que se controla al iniciar")]
    public int cuboInicial = 0;

    void Start()
    {
        Seleccionar(cuboInicial);
    }

    public void Seleccionar(int indice)
    {
        if (cubos == null || indice < 0 || indice >= cubos.Length || cubos[indice] == null) return;

        for (int i = 0; i < cubos.Length; i++)
            if (cubos[i] != null) ActivarCubo(cubos[i], i == indice);

        if (camara != null) camara.objetivo = cubos[indice].transform;

        // Quita el foco del botón; si no, al presionar ESPACIO se "vuelve a presionar" el botón
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void ActivarCubo(GameObject cubo, bool activo)
    {
        foreach (var script in cubo.GetComponents<MonoBehaviour>())
        {
            if (script is CuboEscalador escalador)
                escalador.Activar(activo);   // sigue funcionando (cae si queda en el aire) pero ignora el teclado
            else
                script.enabled = activo;     // otros scripts de movimiento se apagan por completo
        }

        // Si usa física normal, que no se quede deslizando al dejar de controlarlo
        if (!activo)
        {
            var rb = cubo.GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
}