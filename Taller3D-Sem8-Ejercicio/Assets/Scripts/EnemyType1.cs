using UnityEngine;

public class EnemyType1 : Enemy
{
    // Este tipo de enemigo no tiene un comportamiento de movimiento individual adicional.
    // Su movimiento está controlado por el EnemyFormationManager.
    protected override void Move()
    {

    }
}