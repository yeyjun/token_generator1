namespace TokenGenerator.Services;

public static class PetOverlayService
{
    static PetOverlay? _overlay;
    static bool _running;

    public static async void RunPetAcrossScreen()
    {
        if (_running)
            return;

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is null)
            return;

        _running = true;

        _overlay ??= new PetOverlay(window);
        window.AddOverlay(_overlay);

        await _overlay.RunAcrossScreenAsync();

        window.RemoveOverlay(_overlay);
        _running = false;
    }
}
