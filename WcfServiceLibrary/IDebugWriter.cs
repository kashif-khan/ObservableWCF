namespace WcfServiceLibrary
{
    public interface IWriter
    {
        void WriteLine(string v);
        void WriteLine(string v, State color);
    }

    public enum State
    {
        Red,
        Yellow,
        Green
    }
}