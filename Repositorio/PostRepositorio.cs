using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class PostRepositorio : IPostRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public PostRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarPost(Post post)
        {
            post.FechaActualizacion = DateTime.Now;
            var imagenDesdeBd = _bd.Post.AsNoTracking().FirstOrDefault(c => c.Id == post.Id);

            if (post.RutaImagen == null)
            {
                post.RutaImagen = imagenDesdeBd.RutaImagen;
            }

            _bd.Post.Update(post);
            return Guardar();
        }

        public bool BorrarPost(Post post)
        {
            _bd.Post.Remove(post);
            return Guardar();
        }

        public bool CrearPost(Post post)
        {
            post.FechaCreacion = DateTime.Now;
            _bd.Post.Add(post);
            return Guardar();
        }

        public bool ExistePost(string nombre)
        {
            bool valor = _bd.Post.Any(c => c.Titulo.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public bool ExistePost(int id)
        {
            return _bd.Post.Any(c => c.Id == id);
        }

        public Post GetPost(int postId)
        {
            return _bd.Post.FirstOrDefault(c => c.Id == postId);
        }

        public ICollection<Post> GetPosts()
        {
            return _bd.Post.OrderBy(c => c.Id).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }
    }
}
