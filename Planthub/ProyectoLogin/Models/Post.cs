// Models/Post.cs
using System;

namespace ProyectoLogin.Models
{
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } 
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}


}