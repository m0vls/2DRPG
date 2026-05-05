using Mirror;
using UnityEngine;

public class TestSelectButton : MonoBehaviour
{
    [SerializeField] private PlayerClass playerClass;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject manaBar;
    public void SelectCharacter()
    {
        NetworkClient.Send(new CharacterSelectMessage { characterClass = playerClass });
        selectPanel.SetActive(false);
        if (playerClass == PlayerClass.Mage)
            manaBar.SetActive(true);
    }
}