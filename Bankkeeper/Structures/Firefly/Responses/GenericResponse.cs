namespace Bankkeeper.Structures.Firefly.Responses
{
    public class GenericResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? Exception { get; set; }
    }
}