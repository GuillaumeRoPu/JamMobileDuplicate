using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TrapUIContent
{
    [field: SerializeField] public Button TrapButton {  get; private set; }
    [field: SerializeField] public Image TrapIcon {  get; private set; }
    [field: SerializeField] public TextMeshProUGUI RequiredTxt { get; private set; }

    [field: SerializeField] public Trap CurrentTrap { get; private set; }
    [field: SerializeField, MinValue(1)] public int RequiredMaterials { get; private set; }
}
public class TrapUI : MonoBehaviour
{
    [SerializeField] Player _player;

    [SerializeField] TrapUIContent _trap1;
    [SerializeField] TrapUIContent _trap2;
    [SerializeField] TrapUIContent _trap3;

    private void OnEnable()
    {
        _player.MaterialChange += CheckIfCanPlaceATrap;
    }

    private void OnDisable()
    {
        _player.MaterialChange -= CheckIfCanPlaceATrap;
    }

    private void Start()
    {
        _trap1.RequiredTxt.text = string.Format("x{0}", _trap1.RequiredMaterials);
        _trap2.RequiredTxt.text = string.Format("x{0}", _trap2.RequiredMaterials);
        _trap3.RequiredTxt.text = string.Format("x{0}", _trap3.RequiredMaterials);
    }

    void CheckIfCanPlaceATrap()
    {
        _trap1.TrapIcon.color = _player.Materials >= _trap1.RequiredMaterials ? Color.white : Color.black;
        _trap2.TrapIcon.color = _player.Materials >= _trap2.RequiredMaterials ? Color.white : Color.black;
        _trap3.TrapIcon.color = _player.Materials >= _trap3.RequiredMaterials ? Color.white : Color.black;
    }

    public void ChooseTrapByIndex(int index)
    {
        if (index == 0)
        {
            if (_player.Materials >= _trap1.RequiredMaterials)
            {
                _player.ChooseTrapToPlace(_trap1.CurrentTrap, _trap1.RequiredMaterials);
            }
        }

        if (index == 1)
        {
            if (_player.Materials >= _trap2.RequiredMaterials)
            {
                _player.ChooseTrapToPlace(_trap2.CurrentTrap, _trap2.RequiredMaterials);
            }
        }

        if (index == 2)
        {
            if (_player.Materials >= _trap3.RequiredMaterials)
            {
                _player.ChooseTrapToPlace(_trap3.CurrentTrap, _trap3.RequiredMaterials);
            }
        }
    }
}
