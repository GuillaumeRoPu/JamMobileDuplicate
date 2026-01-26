using System.Collections.Generic;
using UnityEngine;

namespace Frynity.ECS
{
    public class Item : Entity
    {
        [SerializeField] Sprite _itemSprite;
        [SerializeField] Animator _itemAnimator;

        [SerializeField] List<Interactions> _interactions;
    }

    [System.Serializable]
    public class Interactions
    {
        [SerializeField] InteractionType interactionType;
        [SerializeField] List<Interaction> interactions;
    }
}
