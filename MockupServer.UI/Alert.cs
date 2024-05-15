
using Serilog;

namespace MockupServer.UI
{
    public static class Alert
    {
        public static void Show(string content, string header = "提示", int maxSize = -1)
        {
            Log.Logger.Information($"【Alert.Show】{content}");
            MessageBox.Show(maxSize > 0 ? content.Substring(0, maxSize) : content, header);
        }
    }
}
