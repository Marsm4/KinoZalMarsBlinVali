using KinoZalMarsBlinVali.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Data
{
    public static class AppDataContext
    {
        public static Employee? CurrentUser { get; set; }
        public static CinemaDbContext DbContext { get; set; } = new CinemaDbContext();
 
        public static bool IsCustomer => CurrentUser?.Role == "customer";
        public static bool IsEmployee => CurrentUser?.Role != "customer";
    }
}
