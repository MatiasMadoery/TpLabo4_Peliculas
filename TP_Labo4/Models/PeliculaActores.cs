namespace TP_Labo4.Models
{
    public class PeliculaActores
    {
        public int Id { get; set; }
        public int PeliculaId { get; set; }
        public int ActorId { get; set; }
        public Pelicula? Pelicula { get; set; }
        public Actor? Actor { get; set; } 
        
    }
}
