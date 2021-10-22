namespace NfhcModel.Logger
{
    public interface InGameLogger
    {
        void Log(object message);
        void Log(string message);
    }
}