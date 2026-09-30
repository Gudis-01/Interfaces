using UnityEngine;

// Pónselo a la Main Camera y arrastra el Cubo al campo "Objetivo".
// Sigue la posición del cubo con suavidad, pero NO copia su rotación.
public class CamaraSigue : MonoBehaviour
{
    public Transform objetivo;
    [Tooltip("Tiempo aproximado para alcanzar al cubo (0 = instantáneo)")]
    public float suavidad = 0.15f;
    [Tooltip("Desplazamiento respecto al cubo (ej. y = 1 para ver más arriba)")]
    public Vector2 desplazamiento = Vector2.zero;

    Vector3 velocidad;

    void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 destino = new Vector3(
            objetivo.position.x + desplazamiento.x,
            objetivo.position.y + desplazamiento.y,
            transform.position.z); // conserva la Z de la cámara (normalmente -10)

        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidad, suavidad);
    }
}
