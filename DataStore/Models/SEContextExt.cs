using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
   
    public partial class SEContext : DbContext
    {
        public static string SqliteConnectionString { get; set; } = string.Empty;
    }
}
