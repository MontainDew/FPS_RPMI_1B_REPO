using UnityEngine;

public class CofreSensores : MonoBehaviour
{
    public Animator animator;
    public GameObject ColorCode;

    void Start()
    {
        ColorCode.SetActive(false);
    }

    public void AbrirCofre()
    {
        AudioManager.Instance.Playsfx(10);
        Debug.Log("Intentando abrir cofre");
        animator.SetTrigger("Abrir");
        ColorCode.SetActive(true);
    }
}