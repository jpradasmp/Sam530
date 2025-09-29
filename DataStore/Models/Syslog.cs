
using System.ComponentModel.DataAnnotations;

namespace DataStore.Models
{
    public enum Active
    {
        DEACTIVATED = 0,
        ACTIVATED = 1,
    }
    public enum ProtocolType
    {
        UDP = 0,
        TCP = 1,
        LOCAL = 2,
    }

    public class Syslog
    {
        public int Id { get; set; }
        public Active Active { get; set; }

        [Required(ErrorMessage = "IP address is required")]
        [RegularExpression(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Dirección IP no válida.")]
        public string IPAddress { get; set; } = string.Empty;

        public ProtocolType ProtocolType { get; set; }
        public Active UseTLS { get; set; }        

    }
}
