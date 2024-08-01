using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoLogin.Models;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoLogin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsuarioContext _context;

        public HomeController(ILogger<HomeController> logger, UsuarioContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ClaimsPrincipal claimsUser = HttpContext.User;
            string nombreUsuario = "";
            string fotoPerfil = "";

            if (claimsUser.Identity.IsAuthenticated)
            {
                nombreUsuario = claimsUser.Claims.Where(c => c.Type == ClaimTypes.Name)
                    .Select(c => c.Value).SingleOrDefault();

                fotoPerfil = claimsUser.Claims.Where(c => c.Type == "FotoPerfil")
                    .Select(c => c.Value).SingleOrDefault();
            }

            ViewData["nombreUsuario"] = nombreUsuario;
            ViewData["fotoPerfil"] = fotoPerfil;
            ViewData["CurrentFilter"] = searchString;

            var posts = from p in _context.Posts.Include(p => p.Comments)
                        select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(s => s.UserName.Contains(searchString) || s.Content.Contains(searchString));
            }

            return View(await posts.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(string title, string content)
        {
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
            {
                var newPost = new Post
                {
                    Title = title,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    UserName = User.Identity?.Name // Aquí obtenemos el nombre del usuario autenticado
                };

                _context.Posts.Add(newPost);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var newComment = new Comment
                {
                    PostId = postId,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    UserName = User.Identity?.Name ?? "Anónimo"
                };

                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("IniciarSesion", "Login");
        }

        public IActionResult Chat()
        {
            ViewData["nombreUsuario"] = User.Identity?.Name ?? "Anonimo"; 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int postId, string newTitle, string newContent)
        {
            if (string.IsNullOrEmpty(newTitle) || string.IsNullOrEmpty(newContent))
            {
                // Puedes agregar un mensaje de error aquí si es necesario
                return RedirectToAction("Index");
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.Title = newTitle;
                post.Content = newContent;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
