using Mirror;
using UnityEngine;

public class SelectClassUI : MonoBehaviour
{
    public void SelectClass(int classIndex)
    {
        PlayerClass selectedClass = (PlayerClass)classIndex;

        NetworkClient.Send(new CharacterSelectMessage { characterClass = selectedClass });
        UIManager.Instance.ToggleSelectUI();
    }
}
