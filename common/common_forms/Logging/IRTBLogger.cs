namespace common_forms.Logging
{
    public interface IRTBLogger
    {
        string Log(string message);
        string Log(string message, bool newSection);
        string Log(string message, TextTag tag);
        string Log(string message, TextTag tag, bool newSection);
        void Clear();
    }
}
