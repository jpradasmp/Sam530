
using System.ComponentModel.DataAnnotations;

namespace DataStore.Models
{
    public class Setups
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "IP address is required")]
        [RegularExpression(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Dirección IP no válida.")]
        public string IpAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subnet Mask is required")]
        [RegularExpression(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Máscara de Subred no válida.")]
        public string SubnetMask { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gateway is required")]
        [RegularExpression(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Puerta de Enlace no válida.")]
        public string Gateway { get; set; } = string.Empty;
        

    }
}
