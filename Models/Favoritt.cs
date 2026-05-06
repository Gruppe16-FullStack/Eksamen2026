using System.ComponentModel.DataAnnotations;

namespace Pendlerapp.Models
{
    public class Favoritt
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Navn")]
        public string Navn { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fra stoppested")]
        public string FraStoppested { get; set; } = string.Empty;

        [Required]
        public string FraStoppestedId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Til stoppested")]
        public string TilStoppested { get; set; } = string.Empty;

        [Required]
        public string TilStoppestedId { get; set; } = string.Empty;

        [Display(Name = "Opprettet")]
        public DateTime Opprettet { get; set; } = DateTime.Now;

        public string BrukerId { get; set; } = string.Empty;

        public ICollection<Reisehistorikk> Reisehistorikker { get; set; } = new List<Reisehistorikk>();
    }
    
}