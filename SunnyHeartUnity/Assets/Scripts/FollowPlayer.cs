using Pizza.Runtime;
using UnityEngine;

namespace Pizza
{
    public class FollowPlayer : PizzaMonoBehaviour
    {
        [SerializeField]
        private Transform _playerTransform;

        private void Update()
        {
            transform.position = new Vector3(_playerTransform.position.x, _playerTransform.position.y, -10f);
        }
    }
}

