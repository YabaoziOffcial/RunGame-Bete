public class GameModel
{
    public int KillEnemyCount { get; private set; }
    public float StartGameTime { get; private set; }

    public bool KillEnemyCountChanged { get; set; }

    public GameModel()
    {
        KillEnemyCount = 0;
        StartGameTime = 0f;
        KillEnemyCountChanged = false;
    }

    public void AddKillEnemyCount()
    {
        KillEnemyCount++;
        KillEnemyCountChanged = true;
    }

    public void ResetData()
    {
        KillEnemyCount = 0;
    }

    public void SetStartGameTime(float startGameTime)
    {
        StartGameTime = startGameTime;
    }
}
