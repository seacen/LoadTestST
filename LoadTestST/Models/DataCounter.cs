namespace LoadTestST.Models
{
    public class DataCounter<T>
    {
        public DataCounter(T data)
        {
            Data = data;
            Count = 0;
        }
        public int Count { get; set; }
        public T Data { get; }
    }
}