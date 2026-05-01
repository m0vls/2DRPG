using Mirror;
using UnityEngine;

public class TestSelectButton : MonoBehaviour
{
    [SerializeField] private PlayerClass playerClass;
    [SerializeField] private GameObject selectPanel;
    public void SelectCharacter()
    {
        NetworkClient.Send(new CharacterSelectMessage { characterClass = playerClass });
        selectPanel.SetActive(false);
    }
}