using UnityEngine;

namespace Frynity.Heal
{
    [System.Serializable]
    public class HealSystem
    {
        [SerializeField, ReadOnly] float _currentHP;
        [SerializeField, MinValue(1)] float _maxHP;

        public float CurrentHP => _currentHP;
        public float MaxHP => _maxHP;
        public bool isDied => _currentHP <= 0;
        public float Ratio => _currentHP / _maxHP;

        public void InitHeal()
        {
            _currentHP = _maxHP;
        }

        public void TakeDamange(float damage)
        {
            _currentHP = Mathf.Max(0, _currentHP - damage);
        }

        public void Heal(float hp)
        {
            _currentHP = Mathf.Min(_maxHP, _currentHP + hp);
        }

        public void SetMaxHP(float maxHP, bool full)
        {
            _maxHP = maxHP;
            _currentHP = full ? _maxHP : _currentHP;
        }
    }
}