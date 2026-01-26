using UnityEngine;

namespace Frynity.ECS
{
    public class Block : Item
    {
        [SerializeField] Sprite _blockSprite;
        [SerializeField] Animator _blockAnimator;
    }

}