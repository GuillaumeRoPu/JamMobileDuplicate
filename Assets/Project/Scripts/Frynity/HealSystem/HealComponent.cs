using UnityEngine;

namespace Frynity.Heal
{
    public class HealComponent : MonoBehaviour
    {
        [SerializeField] HealSystem healSystem;

        public HealSystem Heal => healSystem;

        private void Awake()
        {
            healSystem.InitHeal();
        }
    }
}