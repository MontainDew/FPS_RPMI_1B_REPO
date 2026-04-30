using UnityEngine;

public class Cofre : MonoBehaviour
{
    public Animator animator;
    public GameObject llave;
    public GameObject HColorCode;
    private bool abierto = false;

     void Start() 
     {
        llave.SetActive(false);
        HColorCode.SetActive(false);
    }
    
        
    
    
    public void AbrirCofre()
    {
        if (!abierto)
            AudioManager.Instance.Playsfx(10);
        {   Debug.Log("Intentando abrir cofre");
            animator.SetTrigger("Abrir");
            abierto = true;
            llave.SetActive(true);
            HColorCode.SetActive(true);
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