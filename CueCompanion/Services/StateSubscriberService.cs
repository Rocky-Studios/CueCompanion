namespace CueCompanion.Services;

public abstract class StateSubscriberService
{
    public event Action? OnStateChanged;
    
    public void UpdateState()
    {
        OnStateChanged?.Invoke();
    }
}