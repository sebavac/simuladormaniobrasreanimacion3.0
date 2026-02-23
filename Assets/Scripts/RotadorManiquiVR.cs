using UnityEngine;

public class RotadorManiquiVR : MonoBehaviour
{
    [Header("Referencias")]
    public PoseSkeletonVisualizer visualizador;

    [Header("Configuración de Rotación")]
    [Tooltip("Cuántos grados girará por cada toque del botón")]
    public float gradosPorToque = 15f;

    [Tooltip("Pon un 1 en el eje que hace que gire como un reloj en el suelo (Normalmente Z o Y)")]
    public Vector3 ejeDeGiro = new Vector3(0, 0, 1);

    public void GirarIzquierda()
    {
        if (visualizador != null)
        {
            visualizador.rotacionManiqui -= ejeDeGiro * gradosPorToque;
        }
    }

    public void GirarDerecha()
    {
        if (visualizador != null)
        {
            visualizador.rotacionManiqui += ejeDeGiro * gradosPorToque;
        }
    }
}