using System.Collections;
using UnityEngine;

// Cubo 2D que se mueve rodando (rotando sobre sus esquinas) y se pega a las superficies:
// - Suelo plano: rueda 90° sobre la esquina de adelante.
// - Pared enfrente: gira en su lugar y se pega a la pared (ahora "abajo" es la pared).
// - Borde (se acaba la plataforma): rodea la esquina (180°) y queda pegado al costado.
// - ESPACIO: se suelta de la superficie y cae hacia abajo del mundo.
// - Si no hay nada debajo: cae hasta tocar algo.
// No usa física: todo es movimiento directo del Transform.
public class CuboEscalador : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Tamaño del cubo en unidades (1 = un tile)")]
    public float tamano = 1f;
    [Tooltip("Segundos que tarda un giro de 90°")]
    public float duracionGiro = 0.2f;
    [Tooltip("Segundos por cada casilla que cae")]
    public float duracionCaida = 0.08f;
    [Tooltip("Capa donde están tus plataformas")]
    public LayerMask capaPlataformas;

    [Header("Control")]
    [Tooltip("Solo el cubo activo responde al teclado (lo cambia SelectorCubos)")]
    public bool activo = true;

    Vector2 abajo = Vector2.down;   // hacia dónde está la superficie a la que está pegado
    bool ocupado;
    bool soltarPendiente;           // guarda el ESPACIO si se presionó a mitad de un giro
    Vector3 origenGrid;

    void Start()
    {
        origenGrid = transform.position;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic; // ignorar física

        Pegar(); // si empieza flotando un poco, lo baja hasta tocar la superficie
    }

    void Update()
    {
        // Se lee siempre, aunque esté girando, para no perder el ESPACIO
        if (activo && PresionoSoltar()) soltarPendiente = true;

        if (ocupado) return;

        // ESPACIO: soltarse de la pared o del techo
        if (soltarPendiente)
        {
            soltarPendiente = false;
            if (abajo != Vector2.down)
            {
                StartCoroutine(Caer());
                return;
            }
        }

        // Sin superficie a la que agarrarse -> caer
        if (!Tocando(abajo))
        {
            StartCoroutine(Caer());
            return;
        }

        // Los cubos inactivos se quedan quietos (pero sí caen si no tienen dónde apoyarse)
        if (!activo) return;

        float h = LeerHorizontal();
        if (h == 0) return;

        // "Adelante" es perpendicular a la superficie; así, al pegarse a una pared,
        // la misma tecla lo sigue haciendo avanzar (subir).
        Vector2 adelante = h > 0 ? RotarCCW(abajo) : RotarCW(abajo);
        StartCoroutine(Moverse(adelante));
    }

    public void Activar(bool valor)
    {
        activo = valor;
        if (!valor) soltarPendiente = false;
    }

    IEnumerator Moverse(Vector2 adelante)
    {
        ocupado = true;
        Vector2 centro = transform.position;
        float mitad = tamano * 0.5f;
        float signo = -Mathf.Sign(Cross(abajo, adelante)); // sentido de rodar
        Vector2 esquina = centro + (adelante + abajo) * mitad;

        if (HaySolido(centro + adelante * tamano))
        {
            // PARED ENFRENTE: gira y se pega a la pared
            yield return Girar(centro, -90f * signo);
            abajo = adelante;
        }
        else if (HaySolido(centro + (adelante + abajo) * tamano))
        {
            // SUELO CONTINÚA: rodar normal una casilla
            yield return Girar(esquina, 90f * signo);
        }
        else
        {
            // BORDE: rodea la esquina y queda pegado al costado
            yield return Girar(esquina, 180f * signo);
            abajo = -adelante;
        }

        Ajustar();
        Pegar(); // cierra cualquier espacio con la superficie nueva
        ocupado = false;
    }

    // Gira el cubo alrededor de un punto (pivote) de forma animada
    IEnumerator Girar(Vector2 pivote, float angulo)
    {
        Vector3 posInicial = transform.position;
        Quaternion rotInicial = transform.rotation;
        Vector3 offset = posInicial - (Vector3)pivote;
        float dur = duracionGiro * Mathf.Abs(angulo) / 90f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float a = Mathf.Lerp(0f, angulo, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            Quaternion q = Quaternion.Euler(0f, 0f, a);
            transform.position = (Vector3)pivote + q * offset;
            transform.rotation = q * rotInicial;
            yield return null;
        }
    }

    IEnumerator Caer()
    {
        ocupado = true;
        abajo = Vector2.down;

        AjustarRotacion();
        float velocidad = tamano / duracionCaida;
        float recorrido = 0f;

        // Cae de forma continua y se detiene EXACTAMENTE al tocar la superficie
        while (true)
        {
            float paso = velocidad * Time.deltaTime;
            if (CastSuperficie(Vector2.down, paso, out float mov))
            {
                transform.position += Vector3.down * mov;
                break;
            }
            transform.position += Vector3.down * paso;
            recorrido += paso;
            if (recorrido > 200f) break; // seguridad: cayó al vacío
            yield return null;
        }

        origenGrid = transform.position; // la cuadrícula se alinea con donde aterrizó
        soltarPendiente = false;
        ocupado = false;
    }

    // Acerca el cubo a la superficie de "abajo" hasta tocarla, sin importar
    // si la plataforma está alineada a la cuadrícula o no.
    void Pegar()
    {
        if (CastSuperficie(abajo, tamano, out float mov))
            transform.position += (Vector3)(abajo * mov);

        origenGrid = transform.position;
    }

    // ¿Está tocando una superficie en esa dirección?
    bool Tocando(Vector2 dir)
    {
        return CastSuperficie(dir, tamano * 0.05f, out _);
    }

    // Lanza una caja del tamaño del cubo en una dirección y dice cuánto puede avanzar
    // antes de tocar algo. La caja es un poco más chica para que NO detecte
    // las superficies que ya está tocando por los lados o por detrás (ej. el techo al soltarse).
    bool CastSuperficie(Vector2 dir, float distancia, out float movimiento)
    {
        float m = tamano * 0.05f;
        Vector2 size = Mathf.Abs(dir.x) > 0.5f
            ? new Vector2(tamano - 2f * m, tamano * 0.9f)
            : new Vector2(tamano * 0.9f, tamano - 2f * m);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, size, 0f, dir, distancia + m, capaPlataformas);
        foreach (var hit in hits)
        {
            if (hit.distance <= 0.0001f) continue; // ya estaba encimado: ignorarlo
            movimiento = Mathf.Max(0f, hit.distance - m);
            return true;
        }

        movimiento = distancia;
        return false;
    }

    // Corrige errores de redondeo: posición en la cuadrícula y rotación múltiplo de 90°
    void Ajustar()
    {
        Vector3 rel = (transform.position - origenGrid) / tamano;
        rel.x = Mathf.Round(rel.x);
        rel.y = Mathf.Round(rel.y);
        Vector3 p = origenGrid + rel * tamano;
        p.z = transform.position.z;
        transform.position = p;

        AjustarRotacion();
        abajo = new Vector2(Mathf.Round(abajo.x), Mathf.Round(abajo.y));
    }

    void AjustarRotacion()
    {
        float z = Mathf.Round(transform.eulerAngles.z / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }

    bool HaySolido(Vector2 punto)
    {
        return Physics2D.OverlapBox(punto, Vector2.one * tamano * 0.8f, 0f, capaPlataformas) != null;
    }

    float LeerHorizontal()
    {
#if ENABLE_INPUT_SYSTEM
        var k = UnityEngine.InputSystem.Keyboard.current;
        if (k == null) return 0f;
        float v = 0f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) v += 1f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) v -= 1f;
        return v;
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    bool PresionoSoltar()
    {
#if ENABLE_INPUT_SYSTEM
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && k.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }

    static Vector2 RotarCCW(Vector2 v) => new Vector2(-v.y, v.x);
    static Vector2 RotarCW(Vector2 v) => new Vector2(v.y, -v.x);
    static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    // Dibuja en la escena hacia dónde está "abajo" (útil para depurar)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + abajo * tamano * 0.6f);
    }
}