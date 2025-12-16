namespace Jacob_Yakesh.MonsterChaseLogic.SpriteLock
{
    using UnityEngine;

    public class SpriteRotationLock : MonoBehaviour
    {
        private Quaternion initialRotation;

        private void Awake()
        {
            initialRotation = transform.rotation; // Store the initial rotation
        }

        private void LateUpdate()
        {
            transform.rotation = initialRotation; // Reset rotation to the initial rotation every frame
        }
    }
}