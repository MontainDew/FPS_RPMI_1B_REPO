using UnityEngine;

public class CofreSensores : MonoBehaviour
{
    public Animator animator;
    public GameObject ColorCode;
    private bool abierto = false;

    void Start()
    {
        ColorCode.SetActive(false);
    }




    public void AbrirCofre()
    {
        if (!abierto)
            AudioManager.Instance.Playsfx(10);
        {
            Debug.Log("Intentando abrir cofre");
            animator.SetTrigger("Abrir");
            abierto = true;
            ColorCode.SetActive(true);
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