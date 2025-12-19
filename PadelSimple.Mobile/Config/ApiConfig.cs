namespace PadelSimple.Mobile.Services;

public static class ApiConfig
{
    // Zet je WEB API aan op http://localhost:5009 (Kestrel)
    // MAUI moet afhankelijk van platform een andere host gebruiken.

    public static string BaseUrl
    {
        get
        {
#if ANDROID
            // Android emulator -> host machine = 10.0.2.2
            return "http://10.0.2.2:5009/";
#elif WINDOWS
            // Windows desktop MAUI kan localhost gebruiken
            return "http://localhost:5009/";
#else
            // iOS simulator -> host machine
            return "http://localhost:5009/";
#endif
        }
    }
}
