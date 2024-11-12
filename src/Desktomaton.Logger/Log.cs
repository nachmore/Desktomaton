using System.Diagnostics;
using System.Text;

namespace Desktomaton.Logger
{
  public class Log
  {

    private const int LOG_SIZE = 50000;

    public static event EventHandler? NewLogs;

    private static StringBuilder _debugBuffer;

    static Log()
    {
      _debugBuffer = new StringBuilder(LOG_SIZE);
    }

    public static string LogBuffer
    {
      get { return _debugBuffer.ToString(); }
    }

    public static void Write(object message)
    {
      if (message == null)
        return;

      AddDebugText(message.ToString());

      Debug.Write(message);
    }

    public static void WriteLine(object message)
    {
      if ((message == null))
        return;

      AddDebugText(message.ToString() + "\n");

      Debug.WriteLine(message);
    }

    private static void AddDebugText(string text)
    {
      text = $"[{DateTime.Now}] {text}";

      if (_debugBuffer.Length + text.Length > _debugBuffer.MaxCapacity)
      {
        _debugBuffer.Remove(_debugBuffer.Length, -text.Length);
      }

      _debugBuffer.Insert(0, text);

      // Don't send the the whole buffer as the event arg, so that if it isn't used
      // we don't invoke the cost of comparing to a string
      NewLogs?.Invoke(null, EventArgs.Empty);
    }
  }
}
