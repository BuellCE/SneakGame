using UnityEngine;
using UnityEngine.AI;

//an enemy that always knows the players location and will slowly move towards it.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPursuer : MonoBehaviour, IEnemy {

    public float maxSpeed = 7;

    NavMeshAgent ai;
    Transform playerTransform;
    float startingSpeed;
    float startingRotationSpeed;
    bool canMove = false;

    void IEnemy.Initialize() {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        ai = GetComponent<NavMeshAgent>();
        startingSpeed = ai.speed;
        startingRotationSpeed = ai.angularSpeed;
    }

    void IEnemy.StartMovement() {
        canMove = true;
    }

    void IEnemy.StopMovement() {
        canMove = false;
    }

    void FixedUpdate() {

        if (playerTransform == null || !canMove) {
            return;
        }

        ai.SetDestination(playerTransform.position);

        if (GameUtility.HasLineOfSightWithTaggedObject(transform.position, playerTransform.position, "Player")) {
            ai.speed = maxSpeed;
            ai.angularSpeed = 0;
            var targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,  10 * Time.fixedDeltaTime);
        } else {
            ai.angularSpeed = startingRotationSpeed;
            if (ai.speed > startingSpeed) {
                ai.speed -= Time.fixedDeltaTime * 5;
            }
        }

    }
}
