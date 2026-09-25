using UnityEngine;

public class CharacterChanger : MonoBehaviour
{
    public GameObject personajeActual;
    public GameObject[] personajes;

    public void CambiarPersonaje(int indice)
    {
        if (indice < 0 || indice >= personajes.Length)
            return;

        // Guardamos posición y rotación
        Vector3 posicion = personajeActual.transform.position;
        Quaternion rotacion = personajeActual.transform.rotation;

        // Eliminamos el personaje actual
        Destroy(personajeActual);

        // Creamos el nuevo personaje
        personajeActual = Instantiate(
            personajes[indice],
            posicion,
            rotacion
        );
    }
}
