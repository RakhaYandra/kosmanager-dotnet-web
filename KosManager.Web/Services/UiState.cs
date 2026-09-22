namespace KosManager.Web.Services;

public class UiState
{
    public bool DrawerOpen { get; private set; }
    public event Action? Changed;

    public void ToggleDrawer()
    {
        DrawerOpen = !DrawerOpen;
        Changed?.Invoke();
    }
}
