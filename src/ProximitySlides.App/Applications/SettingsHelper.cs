namespace ProximitySlides.App.Applications;

public static class SettingsHelper
{
    public static string GetSectionName<T>()
        where T : IBaseSettings
    {
        var settingsInstance = Activator.CreateInstance(typeof(T)) as IBaseSettings ??
                               throw new InvalidOperationException($"Failed to create instance of type {typeof(T)}");

        return settingsInstance.SectionName;
    }
}