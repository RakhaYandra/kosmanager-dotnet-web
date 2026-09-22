using MudBlazor;

namespace KosManager.Web.Services;

public static class Snack
{
    public static async Task Try(Func<Task<string>> act, ISnackbar snack)
    {
        try { snack.Add(await act(), Severity.Success); }
        catch (Exception ex) { snack.Add(ex.Message, Severity.Error); }
    }
}
