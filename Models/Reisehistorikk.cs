using System.ComponentModel.DataAnnotations;
namespace Pendlerapp.Models
{
    public class Reisehistorikk
    {
        public int Id { get; set; }

        [Required]
        public int FavorittId { get; set; }

        [Display(Name = "Brukt")]
        public DateTime Brukt { get; set; } = DateTime.Now;

        [Display(Name = "Planlagt avgangstid")]
        public DateTime PlanlagtAvgangstid { get; set; }

        [Display(Name = "Faktisk avgangstid")]
        public DateTime FaktiskAvgangstid { get; set; }

        public Favoritt? Favoritt { get; set; } = null!;
    }
    
}