using UnityEngine;
public enum EnemyState
{
    Idle,
    Run,
    Walk,
    Attack,
    Hurt,
    Death,
    Chase
}
public class Enemy : MonoBehaviour
{
    public EnemyState currentState;
    public int health;
    public string enemyName;
    public int baseAttack;
    public float moveSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
