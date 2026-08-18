using System.Globalization;
using Godot;

namespace Mws.Client.Godot.Localization;

internal static class GameLocalization
{
    internal const string English = "en";
    internal const string Russian = "ru";

    private const string SettingsPath = "user://settings.cfg";
    private const string SettingsSection = "language";
    private const string LocaleSetting = "locale";

    private static readonly HashSet<Action> UiRefreshers = [];
    private static bool _initialized;
    private static string _currentLocale = English;

    internal static string CurrentLocale
    {
        get
        {
            Initialize();
            return _currentLocale;
        }
    }

    internal static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        ApplyLocale(NormalizeLocale(LoadRequestedLocale()), persist: false);
    }

    internal static void SetLocale(string locale)
    {
        Initialize();
        var normalized = NormalizeLocale(locale);
        var changed = !string.Equals(_currentLocale, normalized, StringComparison.Ordinal);
        ApplyLocale(normalized, persist: true);
        if (changed)
        {
            RefreshAllUi();
        }
    }

    internal static void RegisterUiRefresh(Action refresh)
    {
        ArgumentNullException.ThrowIfNull(refresh);
        UiRefreshers.Add(refresh);
    }

    internal static void UnregisterUiRefresh(Action refresh)
    {
        ArgumentNullException.ThrowIfNull(refresh);
        _ = UiRefreshers.Remove(refresh);
    }

    internal static void RefreshAllUi()
    {
        foreach (var refresh in UiRefreshers.ToArray())
        {
            refresh();
        }
    }

    internal static string Tr(string key)
    {
        Initialize();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return TranslationServer.Translate(new StringName(key)).ToString();
    }

    internal static string TrOr(string key, string fallback)
    {
        var translated = Tr(key);
        return string.Equals(translated, key, StringComparison.Ordinal) ? fallback : translated;
    }

    internal static string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.InvariantCulture, Tr(key), arguments);

    internal static string LanguageSelfName(string locale) => NormalizeLocale(locale) switch
    {
        Russian => "Русский",
        _ => "English",
    };

    internal static void ValidateCatalogs()
    {
        Initialize();
        var original = _currentLocale;
        try
        {
            TranslationServer.SetLocale(English);
            if (!string.Equals(
                    TranslationServer.Translate(new StringName("UI_SETTLEMENT")).ToString(),
                    "Settlement",
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("English localization catalog is not loaded correctly.");
            }

            TranslationServer.SetLocale(Russian);
            if (!string.Equals(
                    TranslationServer.Translate(new StringName("UI_SETTLEMENT")).ToString(),
                    "Поселение",
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Russian localization catalog is not loaded correctly.");
            }
        }
        finally
        {
            TranslationServer.SetLocale(original);
        }
    }

    private static string LoadRequestedLocale()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) != Error.Ok)
        {
            return OS.GetLocaleLanguage();
        }

        var stored = (string)config.GetValue(SettingsSection, LocaleSetting, string.Empty);
        return string.IsNullOrWhiteSpace(stored) ? OS.GetLocaleLanguage() : stored;
    }

    private static void ApplyLocale(string locale, bool persist)
    {
        _currentLocale = locale;
        TranslationServer.SetLocale(locale);

        if (!persist)
        {
            return;
        }

        var config = new ConfigFile();
        _ = config.Load(SettingsPath);
        config.SetValue(SettingsSection, LocaleSetting, locale);
        var error = config.Save(SettingsPath);
        if (error != Error.Ok)
        {
            GD.PushWarning($"MWS_LOCALE_SAVE_FAIL locale={locale} error={error}");
        }
    }

    private static string NormalizeLocale(string? locale) =>
        !string.IsNullOrWhiteSpace(locale)
        && locale.StartsWith(Russian, StringComparison.OrdinalIgnoreCase)
            ? Russian
            : English;
}
