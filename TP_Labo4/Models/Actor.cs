namespace TP_Labo4.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string? Foto { get; set; }

        public List<PeliculaActores>? PeliculaActores { get; set; }
    }
}
