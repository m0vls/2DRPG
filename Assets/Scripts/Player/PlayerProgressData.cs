using System;

[Serializable]
public class AuthResponse
{
    public string token;
    public UserData user;
}

[Serializable]
public class AuthRequest
{
    public string nickname;
    public string password;
}

[Serializable]
public class UserData
{
    public int id;
    public string nickname;
}

[Serializable]
public class PlayerProgress
{
    public int currency;
    public bool meta_attack_unlocked;
    public bool meta_defense_unlocked;
    public bool meta_speed_unlocked;
    public bool meta_max_hp_unlocked;
    public int total_kills;
    public int total_games;
}

[Serializable]
public class ProgressUpdate
{
    public int currency;
    public bool meta_attack_unlocked;
    public bool meta_defense_unlocked;
    public bool meta_speed_unlocked;
    public bool meta_max_hp_unlocked;
    public int total_kills;
    public int total_games;
}

[Serializable]
public class RunRecord
{
    public double duration_seconds;
    public int kills;
    public bool victory;
    public int currency_earned;
    public int team_level;
    public int teammate_id;
}

[Serializable]
public class RunHistoryEntry
{
    public int id;
    public int user_id;
    public double duration_seconds;
    public int kills;
    public bool victory;
    public int currency_earned;
    public int team_level;
    public int teammate_id;
    public string created_at;
    public string teammate_nickname;
    public string host_nickname;
}

[Serializable]
public class RunHistoryList
{
    public RunHistoryEntry[] items;
}

[Serializable]
public class ApiError
{
    public string error;
}
