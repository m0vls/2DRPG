using System;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public UserData CurrentUser { get; private set; }
    public string AuthToken => PlayerPrefs.GetString("auth_token", "");
    public bool IsLoggedIn => !string.IsNullOrEmpty(AuthToken);

    private const string TokenKey = "auth_token";
    private const string UserIdKey = "user_id";
    private const string UserNicknameKey = "user_nickname";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (IsLoggedIn)
        {
            CurrentUser = new UserData
            {
                id = PlayerPrefs.GetInt(UserIdKey, 0),
                nickname = PlayerPrefs.GetString(UserNicknameKey, ""),
            };
        }
    }

    public void SaveSession(AuthResponse response)
    {
        CurrentUser = response.user;
        ApiService.Instance.AuthToken = response.token;

        PlayerPrefs.SetString(TokenKey, response.token);
        PlayerPrefs.SetInt(UserIdKey, response.user.id);
        PlayerPrefs.SetString(UserNicknameKey, response.user.nickname);
        PlayerPrefs.Save();
    }

    public void ClearSession()
    {
        CurrentUser = null;
        ApiService.Instance.AuthToken = null;

        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(UserIdKey);
        PlayerPrefs.DeleteKey(UserNicknameKey);
        PlayerPrefs.Save();
    }

    public string PlayerNickname => CurrentUser?.nickname ?? "";
}
