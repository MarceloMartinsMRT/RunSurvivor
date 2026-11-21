using UnityEngine;

public class NearbyEnemyInfo : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject _enemy;
    private float _distanceToPlayer = float.MaxValue;
    private int _id;

    public GameObject Enemy { get => _enemy; set => _enemy = value; }
    public float DistanceToPlayer { get => _distanceToPlayer; set => _distanceToPlayer = value; }
    public int Id { get => _id; set => _id = value; }

    public void AtualizarDistancia(Vector3 playerPos) 
    {
        _distanceToPlayer =(playerPos- _enemy.transform.position).sqrMagnitude;
    }
}
