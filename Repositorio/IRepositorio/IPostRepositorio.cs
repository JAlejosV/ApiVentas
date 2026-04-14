using ApiVentas.Modelos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IPostRepositorio
    {
        ICollection<Post> GetPosts();
        Post GetPost(int postId);
        bool ExistePost(string nombre);
        bool ExistePost(int id);
        bool CrearPost(Post post);
        bool ActualizarPost(Post post);
        bool BorrarPost(Post post);
        bool Guardar();
    }
}
