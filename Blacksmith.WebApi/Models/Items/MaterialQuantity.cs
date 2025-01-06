using System.ComponentModel.DataAnnotations.Schema;

namespace Blacksmith.WebApi.Models.Items
{
    public class MaterialQuantity
    {
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
    }
}
