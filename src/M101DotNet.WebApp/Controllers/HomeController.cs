using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var todoListContext = new TodoListContext();
            var todos = await todoListContext.Todos.Find(x => true && x.Author == User.Identity.Name)
                .SortByDescending(x => x.CreatedAtUtc)
                .Limit(10)
                .ToListAsync();

            var tags = await todoListContext.Todos.Aggregate()
                .Project(x => new { _id = x.Id, Tags = x.Tags })
                .Unwind(x => x.Tags)
                .Group<TagProjection>("{ _id: '$Tags', Count: { $sum: 1 } }")
                .ToListAsync();

            var model = new IndexModel
            {
                Todos = todos,
                Tags = tags
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewTodo()
        {
            return View(new NewTodoModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewTodo(NewTodoModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var todoListContext = new TodoListContext();
            var todo = new Todo
            {
                Author = User.Identity.Name,
                Title = model.Title,
                Content = model.Content,
                Tags = model.Tags.Split(' ', ',', ';'),
                CreatedAtUtc = DateTime.UtcNow
            };

            await todoListContext.Todos.InsertOneAsync(todo);

            return RedirectToAction("TodoList", new { id = todo.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Todo(string id)
        {
            var todoListContext = new TodoListContext();

            var post = await todoListContext.Todos.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post,
                NewComment = new NewCommentModel
                {
                    PostId = id
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> TodoList(string tag = null)
        {
            var todoListContext = new TodoListContext();

            Expression<Func<Todo, bool>> filter = x => true;

            if (tag != null)
            {
                filter = x => x.Author == User.Identity.Name;
            }

            var posts = await todoListContext.Todos.Find(filter)
                .SortByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return View(posts);
        }
        
    }
}