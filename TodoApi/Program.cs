using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;

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
                var todos = await db.Todos.Select(t => new ReadTodoDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsCompleted = t.IsCompleted
                }).ToListAsync();

                return TypedResults.Ok(todos);
            }
            
            static async Task<IResult> GetAllCompletedTasks(TodoDb db)
            {
                var completedTasks = await db.Todos.Where(t => t.IsCompleted).Select(t => new ReadTodoDto
                {
                    Name = t.Name,
                    IsCompleted = t.IsCompleted
                }).ToListAsync();
                return TypedResults.Ok(completedTasks);
            }


            static async Task<IResult> CreateTodo(CreateTodoDto todo, TodoDb db)
            {
                if (todo == null) return TypedResults.BadRequest();
                if (string.IsNullOrEmpty(todo.Name)) return TypedResults.Conflict("Name cant be empty here !");
                var existingTodo = db.Todos.Any(t => t.Name == todo.Name);

                if (existingTodo) return TypedResults.Conflict($"'{todo.Name}' is already in your tasks !");


                var newtodo = new Todo
                {
                    Name = todo.Name,
                    IsCompleted = todo.IsCompleted
                };

                db.Add(newtodo);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/v1/todoItems/{todo.Id}", todo);
               
            }

            static async Task<IResult> UpdateTodo(int id, UpdateTodoDto newtodo, TodoDb db)
            {
                var todo = await db.Todos.FindAsync(id);

                if (todo == null) return TypedResults.NotFound();

                if (string.IsNullOrEmpty(newtodo.Name)) return TypedResults.Conflict("Name can't be empty !");

                if (db.Todos.Any(t => t.Id != id && t.Name == newtodo.Name)) return TypedResults.Conflict($"A Task with this name {newtodo.Name} already exists ");

                todo.Name = newtodo.Name;
                todo.IsCompleted = newtodo.IsCompleted;

                await db.SaveChangesAsync();

                var updatedTodo = new ReadTodoDto
                {
                    Name = newtodo.Name,
                    IsCompleted = newtodo.IsCompleted
                };

                return TypedResults.Ok(updatedTodo);
            }

            static async Task<IResult> Delete(int id, TodoDb db)
            {
                var exists = await db.Todos.FindAsync(id);

                if (exists == null) return TypedResults.Conflict($"Not Found");

                var deleted = new ReadTodoDto
                {
                    Name = exists.Name,
                    IsCompleted = exists.IsCompleted
                };

                 db.Remove(exists);
                 await db.SaveChangesAsync();

                 return TypedResults.Ok(deleted);
            }
        }
    }
}
