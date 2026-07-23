public class GameSession : Singleton<GameSession>
{
    public bool HasStarted { get; private set; }

    public event System.Action OnGameStarted;

    public void StartGame()
    {
        if (HasStarted)
        {
            return;
        }

        HasStarted = true;
        OnGameStarted?.Invoke();
    }
}
