using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header ("Health System Management")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;

    [Header("Feedback Configuration")]
    [SerializeField] Material damagedMat; //Material feedback daño
    [SerializeField] GameObject deathVfx;
    [SerializeField] MeshRenderer enemyRend;
    Material baseMat; //almacén del material base del enemigo

    private void Awake()
    {
        health = maxHealth;
        baseMat = enemyRend.material;
    }

    // Update is called once per frame
    void Update()
    {
       if (health <= 0)
        {
            health = 0;
            deathVfx.SetActive(true);
            deathVfx.transform.position = transform.position;
            gameObject.SetActive(false);
        }
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        enemyRend.material = damagedMat; //Se modifica el material por el de daño
        Invoke(nameof(ResetEnemyMaterial), 0.1f);
    }
    void ResetEnemyMaterial()
    {
        enemyRend.material = baseMat;
    }
}
