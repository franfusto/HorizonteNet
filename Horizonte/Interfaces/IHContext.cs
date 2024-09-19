namespace Horizonte
{
    public interface IHContext
    {
        T? Get<T>(string contextname);
        T? Get<T>();
        void Update<T>(T value, string contexname);
        void Update<T>(T value);
        public void Update<T>(Action<T> update, string contexname);
        public void Update<T>(Action<T> update);
    }
}