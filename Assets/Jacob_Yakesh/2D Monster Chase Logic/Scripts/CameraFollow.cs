namespace Jacob_Yakesh.MonsterChaseLogic.CameraLogic
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class CameraFollow : MonoBehaviour
    {
        public Transform player;
        public Vector3 offset;
        public float smoothSpeed = 0.03f;

        private Camera cam;
        private float originalSize;
        public float idleZoomFactor = 2f;
        private float idleTime;
        public float idleThreshold = 2f;  // Time in seconds for idle zoom to trigger

        private Vector3 lastPlayerPosition;
        private bool isMapView = false; // Track if in map view mode
        public float mapViewSize = 10f;  // Set this to desired map view size in Inspector
        public Vector3 mapCenterPosition;  // Set this in Inspector to the center of your map

        void Start()
        {
            cam = GetComponent<Camera>();
            originalSize = cam.orthographicSize;  // For a 2D camera with orthographic view
            lastPlayerPosition = player.position;
        }

        void LateUpdate()
        {
            // Toggle map view on "M" key press
            if (Input.GetKeyDown(KeyCode.M))
            {
                isMapView = !isMapView;
                if (isMapView)
                {
                    cam.orthographicSize = mapViewSize;  // Set camera size to map view size
                    transform.position = mapCenterPosition;  // Move camera to center of map
                }
                else
                {
                    cam.orthographicSize = originalSize;  // Revert to original size
                }
            }

            // Follow player only if not in map view
            if (!isMapView && player != null)
            {
                Vector3 targetPosition = player.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

                // Check if player is moving
                if (player.position != lastPlayerPosition)
                {
                    idleTime = 0f;  // Reset idle timer if the player has moved
                    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, originalSize, Time.deltaTime * 2f);
                }
                else
                {
                    idleTime += Time.deltaTime;

                    if (idleTime >= idleThreshold)
                    {
                        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, originalSize * idleZoomFactor, Time.deltaTime * 2f);
                    }
                }

                lastPlayerPosition = player.position;  // Update last player position for the next frame
            }
        }
    }

}