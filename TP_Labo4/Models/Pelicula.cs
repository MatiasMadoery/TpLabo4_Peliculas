namespace TP_Labo4.Models
{
    public class Pelicula
    {
        public int Id { get; set; }
        public int GeneroId { get; set; }
        public Genero? Genero { get; set; }
        public string? Titulo { get; set; }
        public DateOnly FechaEstreno { get; set; }
        public string? Portada { get; set; }
        public string? Trailer{ get; set; }
        public string? Resumen { get; set; }

        public List<PeliculaActores>? PeliculaActores { get; set; }
    }
}
