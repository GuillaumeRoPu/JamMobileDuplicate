using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class PlayGameManager : MonoBehaviour
{
#if UNITY_ANDROID
    void Start()
    {
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate((SignInStatus status) => {
            if (status == SignInStatus.Success)
            {
                Debug.Log("You're connected !");
            }
            else
            {
                Debug.Log("Bahahah Loser");
            }
        });
    }
#endif
}
