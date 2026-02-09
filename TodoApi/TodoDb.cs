using Microsoft.EntityFrameworkCore;

namespace TodoApi
{
    public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
    {
        public DbSet<Todo> Todos => Set<Todo>();
    }
}
