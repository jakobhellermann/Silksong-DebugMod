namespace DebugMod.Utils;

public static class ToastManager {
    public static void Toast(object? message) {
        // todo display on screen
        Log.Info(message?.ToString() ?? "null");
    }
}