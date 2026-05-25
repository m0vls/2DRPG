using Mirror;

public struct CharacterSelectMessage : NetworkMessage
{
    public PlayerClass characterClass;
    public string nickname;
    public int userId;
}

public enum PlayerClass
{
    Knight,
    Mage
}