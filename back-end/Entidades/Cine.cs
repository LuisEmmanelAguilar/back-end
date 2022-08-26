using back_end.Validaciones;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace back_end.Entidades
{
    public class Cine
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 75)]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public Point Ubicacion { get; set; }
        public List<PeliculasCines> PeliculasCines { get; set; }
    }
}
