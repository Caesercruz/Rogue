using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float fixmodel = 90f;

    private void Start()
    {
        player = FindFirstObjectByType<Player>().gameObject;
    }
    void Update()
    {
        RotateTowardsPlayer();
    }

    void RotateTowardsPlayer()
    {
        Vector3 playerPos = player.transform.position;

        Vector3 direction = playerPos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - fixmodel;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}