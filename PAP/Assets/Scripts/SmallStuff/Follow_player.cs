using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float fixmodel = 90f;

    void Update()
    {
        GameObject foundPlayer = GameObject.Find("Player");

        if (foundPlayer == null) return;

        player = foundPlayer.GetComponent<Player>().gameObject;
        if (player == null) return;

        RotateTowardsPlayer();
    }

    void RotateTowardsPlayer()
    {
        Vector3 playerPos = player.transform.position;
        playerPos.z = 0;

        Vector3 direction = playerPos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - fixmodel;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}