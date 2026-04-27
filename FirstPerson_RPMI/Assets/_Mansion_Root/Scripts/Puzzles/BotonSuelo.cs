using UnityEngine;

public class BotonSuelo : MonoBehaviour
{
    public Cofre cofre;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Estatua"))
        {
            Debug.Log("Intentando abrir cofre");
            cofre.AbrirCofre();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Estatua"))
        {
            cofre.CerrarCofre(); // opcional
        }
    }
}