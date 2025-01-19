using UnityEngine;

namespace DaftAppleGames.Utilities
{
    public static class GizmoTools
    {
        public static void DrawConeGizmo(Transform startTransform, float fieldOfViewAngle, float range, Color color, int coneResolution)
        {
            Gizmos.color = color;
            if (startTransform == null)
                return;

            // Calculate the forward direction
            Vector3 forward = startTransform.forward * range;

            // Draw the center line of the cone
            Gizmos.DrawRay(startTransform.position, forward);

            // Calculate the left and right boundaries of the cone
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * forward;

            // Draw the left and right boundaries of the cone
            Gizmos.DrawRay(startTransform.position, leftBoundary);
            Gizmos.DrawRay(startTransform.position, rightBoundary);

            // Draw lines to form the cone edges
            for (int i = 0; i <= coneResolution; i++)
            {
                float stepAngle = -fieldOfViewAngle / 2 + (fieldOfViewAngle / coneResolution) * i;
                Vector3 coneEdge = Quaternion.Euler(0, stepAngle, 0) * forward;
                Gizmos.DrawRay(startTransform.position, coneEdge);
            }
        }

    }
}