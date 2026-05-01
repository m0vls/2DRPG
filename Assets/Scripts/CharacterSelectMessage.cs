using Mirror;
using UnityEngine;

public struct CharacterSelectMessage : NetworkMessage
{
    public PlayerClass characterClass;
}

public enum PlayerClass
{
    Knight,
    Mage
}
