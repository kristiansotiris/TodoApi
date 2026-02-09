namespace TodoApi.Dtos
{
    public class CreateTodoDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsCompleted { get; set; }

    }
}
