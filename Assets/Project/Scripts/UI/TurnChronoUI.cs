using UnityEngine;

public class TurnChronoUI : MonoBehaviour
{
    [SerializeField] GameObject _playerTurn;

    [SerializeField] GameObject _zombieTurn;

    bool _changing;
    float _duration;
    float _currentTime;

    int _currentTurn;
    int _turnMax;

    float _turnPos;

    private void Update()
    {
        if (_changing)
        {
            float ratio = _currentTime / _duration;

            float now = _currentTurn - 1;
            now = Mathf.Clamp(now, 0, _turnMax);

            float taretTurn = Mathf.Min(_currentTurn, _turnMax);

            _turnPos = Mathf.Lerp(now / _turnMax, taretTurn / _turnMax, ratio);

            _playerTurn.GetComponent<RectTransform>().anchorMin = new Vector2(_turnPos, 0.5f);
            _playerTurn.GetComponent<RectTransform>().anchorMax = new Vector2(_turnPos, 0.5f);

            if (_currentTime >= _duration)
            {
                _changing = false;
            }

            _currentTime += Time.deltaTime;
        }
    }

    public void UpdateTurn(float duration, int currentTurn, int turnMax)
    {
        _duration = duration;
        _changing = true;
        _currentTurn = currentTurn;
        _turnMax = turnMax;
        _currentTime = 0;
    }

    public void PlaceZombieTurn(int turn, int turnMax)
    {
        GameObject zombie = Instantiate(_zombieTurn, _playerTurn.transform.parent);
        zombie.transform.SetAsFirstSibling();
        float pos = (float)turn / turnMax;
        zombie.GetComponent<RectTransform>().anchorMin = new Vector2(pos, 0.5f);
        zombie.GetComponent<RectTransform>().anchorMax = new Vector2(pos, 0.5f);
    }
}