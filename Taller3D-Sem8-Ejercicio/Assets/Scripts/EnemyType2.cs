using UnityEngine;

public class EnemyType2 : Enemy
{
    private float rotationSpeed = 90f; // Velocidad de rotación

    protected override void Move()
    {
        // Hace que el enemigo rote sobre su eje Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}