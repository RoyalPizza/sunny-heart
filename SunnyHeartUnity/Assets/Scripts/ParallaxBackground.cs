using Pizza.Runtime;
using UnityEngine;

namespace Pizza
{
    /// <summary>
    /// A class for moving the background with the camera at a speed.
    /// </summary>
    public sealed class ParallaxBackground : PizzaMonoBehaviour
    {
        [SerializeField]
        private Transform _cameraTransform;

        [SerializeField, Min(0f), Tooltip("Tweak this per layer, e.g., 0.5 for midground, 0.1 for background")]
        private float _parallaxSpeed = 0.1f;

        [SerializeField]
        private bool _moveVertical = false;

        private Vector3 _lastCameraPosition;

        private void Start()
        {
            _lastCameraPosition = _cameraTransform.position;
        }

        private void Update()
        {
            Vector3 movePosition = Vector3.zero;
            movePosition.x = (_cameraTransform.position.x - _lastCameraPosition.x) * _parallaxSpeed;
            movePosition.y = (_moveVertical) ? (_cameraTransform.position.y - _lastCameraPosition.y) * _parallaxSpeed : 0f;
            transform.position += movePosition;
            _lastCameraPosition = _cameraTransform.position;
        }
    }
}
