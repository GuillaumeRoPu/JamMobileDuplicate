using UnityEngine;

public class TurnManager : MonoBehaviour
{
    int _currentTurn;

    public float Turn => _currentTurn;
    public void NextTurn()
    {
        _currentTurn++;
    }

    
}