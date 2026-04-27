using UnityEngine;

public class Cofre : MonoBehaviour
{
    public Animator animator;
    private bool abierto = false;

    public void AbrirCofre()
    {
        if (!abierto)
        {   Debug.Log("Intentando abrir cofre");
            animator.SetTrigger("Abrir");
            abierto = true;
        }
    }

    public void CerrarCofre()
    {
        if (abierto)
        {
            animator.SetTrigger("Cerrar");
            abierto = false;
        }
    }
}