using UnityEngine;

public class TurnManager : MonoBehaviour
{
    int _currentTurn;
    public float Turn => _currentTurn;

    [SerializeField] float _durationTurn;
    [SerializeField] int _maxTurn;

    [SerializeField] TurnChronoUI _ui;

    bool _notNext;

    int _nbPass;

    public void PassTurn(int nbTurnToPass)
    {
        _nbPass = nbTurnToPass;
    }

    public void NextTurn()
    {
        if (_notNext) return;

        _notNext = true;
        PlayTurn();
        
    }

    void PlayTurn()
    {
        _currentTurn++;
        GameManager.Of.Player.MoveToNextTileInPath(_durationTurn);
        _ui.UpdateTurn(_durationTurn, _currentTurn, _maxTurn);

        WaitEndOfTurn();
    }

    async void WaitEndOfTurn()
    {
        await Awaitable.WaitForSecondsAsync(_durationTurn);

        if (_nbPass > 0)
        {
            PlayTurn();
            _nbPass--;
        }
        else
        {
            _notNext = false;
        }
    }
    
}