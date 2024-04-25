using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }
        
        public ICollection<ProductWarehouse> Inventory { get; set; }
        
        public int Capacity { get; set; }

        public Warehouse()
        {
            Inventory = new HashSet<ProductWarehouse>();
        }
    }
}
