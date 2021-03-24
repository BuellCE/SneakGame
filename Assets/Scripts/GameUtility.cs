using UnityEngine;
using UnityEngine.AI;

public static class GameUtility { 

    public static Vector3 GetRandomNavMeshPositionNearby(Vector3 nearbyLocation, float maxDistance) {
        Vector3 randomDirection = Random.insideUnitSphere * maxDistance;
        randomDirection += nearbyLocation;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, maxDistance * 2, -1);
        return hit.position;
    }

    public static bool HasLineOfSightWithTaggedObject(Vector3 origin, Vector3 target, string tag) {
        Ray ray = new Ray(origin, target - origin);
        if (Physics.Raycast(ray, out RaycastHit checkCover, 100)) {
            if (checkCover.collider.gameObject.CompareTag(tag)) {
                return true;
            }
        }
        return false;
    }


}
