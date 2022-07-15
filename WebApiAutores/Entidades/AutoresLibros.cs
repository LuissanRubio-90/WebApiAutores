namespace WebApiAutores.Entidades
{
    public class AutoresLibros
    {
        public int LibroId { get; set; }
        public int AutorId { get; set; }
        public int Orden { get; set; }
        public Libro oLibro { get; set; }
        public Autor oAutor { get; set; }
    }
}
