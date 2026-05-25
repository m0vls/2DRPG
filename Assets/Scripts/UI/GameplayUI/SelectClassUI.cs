using Mirror;
using UnityEngine;

public class SelectClassUI : MonoBehaviour
{
    public void SelectClass(int classIndex)
    {
        PlayerClass selectedClass = (PlayerClass)classIndex;
        string nick = AuthManager.Instance != null ? AuthManager.Instance.PlayerNickname : "";
        int uid = AuthManager.Instance?.CurrentUser?.id ?? 0;
        NetworkClient.Send(new CharacterSelectMessage
        {
            characterClass = selectedClass,
            nickname = nick,
            userId = uid,
        });
        UIManager.Instance.ToggleSelectUI();
    }
}
