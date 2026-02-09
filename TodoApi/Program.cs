using Microsoft.EntityFrameworkCore;

namespace TodoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
            var app = builder.Build();


            var todoItems = app.MapGroup("/api/v1/todoItems");

            todoItems.MapGet("/", GetAllTodos);
            todoItems.MapPatch("/updatetodo/{id}", UpdateTodo);
            todoItems.MapGet("/completed", GetAllCompletedTasks);
            todoItems.MapPost("/createtodoItem", CreateTodo);
            todoItems.MapDelete("/delete/{id}", Delete);

            app.Run();

            static async Task<IResult> GetAllTodos(TodoDb db)
            {
                var todos = await db.Todos.ToListAsync();
                return TypedResults.Ok(todos);
            }
            
            static async Task<IResult> GetAllCompletedTasks(TodoDb db)
            {
                var completedTasks = await db.Todos.Where(t => t.IsCompleted).ToListAsync();
                return TypedResults.Ok(completedTasks);
            }


            static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
            {
                if (todo == null) return TypedResults.BadRequest();
                if (string.IsNullOrEmpty(todo.Name)) return TypedResults.Conflict("Name cant be empty here !");
                var existingTodo = db.Todos.Where(t => t.Name == todo.Name);

                if (existingTodo != null) return TypedResults.Conflict($"'{todo.Name}' is already in your tasks !");

                db.Add(todo);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/v1/todoItems/{todo.Id}", todo);
               
            }

            static async Task<IResult> UpdateTodo(int id, Todo newtodo, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);

                if (todo == null) return TypedResults.NotFound();

                if (string.IsNullOrEmpty(newtodo.Name)) return TypedResults.Conflict("Name can't be empty !");

                todo.Name = newtodo.Name;
                todo.IsCompleted = newtodo.IsCompleted;
                await db.SaveChangesAsync();
                return TypedResults.Ok(todo);
            }

            static async Task<IResult> Delete(int id, TodoDb db)
            {
                var exists = await db.Todos.FindAsync(id);

                if (exists == null) return TypedResults.Conflict($"Not Found");

                 db.Remove(exists);
                 await db.SaveChangesAsync();
                 return TypedResults.Ok("Deleted Succesfully");
            }
        }
    }
}
