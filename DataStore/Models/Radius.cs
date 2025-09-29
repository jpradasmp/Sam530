
using System.ComponentModel.DataAnnotations;

namespace DataStore.Models
{
    public enum ActiveRadius
    {
        DEACTIVATED = 0,
        ACTIVATED = 1,
    }

    public enum AuthentificationMode
    {
        PAP = 0,
        CHAP = 1,
        EAP_TLS = 2
    }

    public class Radius
    {
        public int Id { get; set; }
        public ActiveRadius Active { get; set; }

        [Required(ErrorMessage = "IP address is required")]
        [RegularExpression(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Dirección IP no válida.")]
        public string IPAddress { get; set; } = string.Empty;

        public AuthentificationMode AuthentificationMode { get; set; }

        [Required(ErrorMessage = "SharedSecret is required")]
        public string SharedSecret { get; set; } = string.Empty;
        

    }
}
