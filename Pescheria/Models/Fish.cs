using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Pescheria.Models
{
    public class Fish
    {
        public int FishId { get; set; }
        public string Name { get; set; }

        [Display(Name = "Di mare")]
        public bool IsSeaFish { get; set; }
        public int Price { get; set; }
        public string? Image { get; set; } = null;
        public DateTime? DeletedAt { get; set; } = null;
    }
}
