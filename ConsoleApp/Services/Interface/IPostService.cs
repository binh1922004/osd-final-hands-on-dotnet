using ConsoleApp.Models;

namespace ConsoleApp.Services.Interface;

public interface IPostService
{
    List<Post> GetPostsByUserId(List<Post> posts, int userId);
    
    List<Post> FilterPostsByKeyword(List<Post> posts, string keyword);
    
    void InteractWithPosts(List<Post> posts);
}