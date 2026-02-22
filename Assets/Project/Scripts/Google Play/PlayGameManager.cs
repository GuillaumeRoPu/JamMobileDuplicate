using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Security.Authentication;
using UnityEngine;

public class PlayGameManager : MonoBehaviour
{
#if UNITY_ANDROID

    static PlayGameManager _instance;
    public static PlayGameManager Of => _instance;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    void Start()
    {
        //PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        //Social.localUser.Authenticate(SocialProcessAuthentication);
    }

    void SocialProcessAuthentication(bool status)
    {
        if (status)
        {
            Debug.Log("GPGS connecté ! Achievements OK");
        }
        else
        {
            Debug.Log("GPGS échec: " + status);
        }
    }

    void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS connecté ! Achievements OK");
        }
        else
        {
            Debug.Log("GPGS échec: " + status);
        }
    }

    // Bouton UI : Connexion manuelle
    public void SignIn()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }

    // Débloque achievement STANDARD (100%)
    public void UnlockAchievement(string id)
    {
        PlayGamesPlatform.Instance.ReportProgress(id, 100.0f, success => {
            Debug.Log(id + (success ? " débloqué!" : " échec"));
        });
    }

    // Achievement INCREMENTIEL
    public void IncrementAchievement(string id, int steps)
    {
        PlayGamesPlatform.Instance.IncrementAchievement(id, steps, success => { });
    }

    // Montre UI achievements
    public void ShowAchievements()
    {
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }
#endif
}
