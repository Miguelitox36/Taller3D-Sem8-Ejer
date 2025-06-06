using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab; 
    public Transform shootPoint; 
    public float bulletSpeed = 15f; 
    public float fireRate = 0.3f; 

    private float nextFireTime; 

    void Update()
    {       
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate; 
            Fire(); 
        }
    }

    // Lanza una bala
    void Fire()
    {        
        InstantiateBullet(Vector3.forward);
    }
        
    void InstantiateBullet(Vector3 dir)
    {
        
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(dir));
                
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = dir * bulletSpeed; 
            rb.useGravity = false;
        }
        else
        {
            Debug.LogWarning("La bala no tiene un Rigidbody. Añadiendo uno automáticamente.");
            rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = dir * bulletSpeed;
        }
    }
}