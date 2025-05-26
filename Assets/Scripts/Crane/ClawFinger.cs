using UnityEngine;

public class ClawFinger : MonoBehaviour
{
    public ClawController clawController; // Assign in Inspector

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Or OnTriggerEnter2D if using triggers and Rigidbodies are on both
        if (clawController != null)
        {
            clawController.AttemptGrab(collision.gameObject, collision.contacts[0].point);
        }
    }
}