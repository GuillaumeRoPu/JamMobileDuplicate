using Frynity.Heal;
using UnityEngine;

namespace Frynity.ECS
{
    public class Entity : MonoBehaviour
    {
        [SerializeField] HealSystem _heal;
        public HealSystem GetHeal => _heal;

        [SerializeField] float _speed;
        public float GetSpeed => _speed;

        Rigidbody2D GetRigidbody => GetComponent<Rigidbody2D>();

        public void MoveEntity(Vector2 direction)
        {
            if (GetRigidbody)
            {
                GetRigidbody.linearVelocity = new Vector2(direction.x * _speed, direction.y * _speed);
            }
        }
    }

}
