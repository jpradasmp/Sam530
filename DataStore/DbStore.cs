using DataStore.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DataStore
{
    public class DbStore
    {
        public static ILogger? logger { get; set; }

        public DbStore() { }

        /// <summary>
        /// Get Setup
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Setups? GetSetup(int Id) { 

            logger!.LogInformation($"Get Setup {Id}");
            
            SEContext Db = new SEContext();

            return (from s in Db.Setups 
                    where s.Id == Id
                    select s).FirstOrDefault();
        }

        /// <summary>
        /// Get Setup
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <summary>
        /// Get Setup
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
         public async Task<Setups?> GetSetupAsync(int Id)
        {
            logger!.LogInformation($"Get Setup {Id}");

            SEContext Db = new SEContext();

            return await Db.Setups.FirstOrDefaultAsync(s => s.Id == Id);

        }

        /// <summary>
        /// Update Setup
        /// </summary>
        /// <param name="setup"></param>
        /// <returns></returns>
        public async Task<bool> UpdateSetupAsync(Setups setup)
        {

            logger!.LogInformation($"Update Setup {setup.Id}");

            SEContext Db = new SEContext();

            var SetUp= (from s in Db.Setups
                    where s.Id == setup.Id
                    select s).FirstOrDefault();
            if(SetUp!= null) 
            {
                SetUp.Name = setup.Name;
                SetUp.IpAddress = setup.IpAddress;
                SetUp.SubnetMask = setup.SubnetMask;
                SetUp.Gateway = setup.Gateway;

                await Db.SaveChangesAsync();

                return true;
            }

            return false;
        }


        /// <summary>
        /// Get Radius
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <summary>
        /// Get Radius
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Radius?> GetRadiusAsync(int Id)
        {
            logger!.LogInformation($"Get Radius {Id}");

            SEContext Db = new SEContext();

            return await Db.Radius.FirstOrDefaultAsync(s => s.Id == Id);

        }

        /// <summary>
        /// Update Radius
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRadiusAsync(Radius radius)
        {

            logger!.LogInformation($"Update Radius {radius.Id}");

            SEContext Db = new SEContext();

            var rad = (from s in Db.Radius
                       where s.Id == radius.Id
                         select s).FirstOrDefault();
            if (rad != null)
            {
                rad.Active = radius.Active;
                rad.IPAddress = radius.IPAddress;
                rad.AuthentificationMode = radius.AuthentificationMode;
                rad.SharedSecret = radius.SharedSecret;

                await Db.SaveChangesAsync();

                return true;
            }

            return false;
        }


        /// <summary>
        /// Get Syslog
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <summary>
        /// Get Syslog
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Syslog?> GetSyslogAsync(int Id)
        {
            logger!.LogInformation($"Get Syslog {Id}");

            SEContext Db = new SEContext();

            return await Db.Syslog.FirstOrDefaultAsync(s => s.Id == Id);

        }

        /// <summary>
        /// Update Syslog
        /// </summary>
        /// <param name="syslog"></param>
        /// <returns></returns>
        public async Task<bool> UpdateSyslogAsync(Syslog syslog)
        {

            logger!.LogInformation($"Update Syslog {syslog.Id}");

            SEContext Db = new SEContext();

            var sys = (from s in Db.Syslog
                         where s.Id == syslog.Id
                         select s).FirstOrDefault();
            if (sys != null)
            {
                sys.Active = syslog.Active;
                sys.IPAddress = syslog.IPAddress;
                sys.ProtocolType = syslog.ProtocolType;
                sys.UseTLS = syslog.UseTLS;

                await Db.SaveChangesAsync();

                return true;
            }

            return false;
        }


    }
}
