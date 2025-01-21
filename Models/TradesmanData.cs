using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NixersDB.Models
{
    public class TradesmanData
    {
        public int UserId { get; set; }

        public string Trade { get; set; }

        public string Location { get; set; }

        public int NumberOfJobsCompleted { get; set; }

        public string TradeBio { get; set; }

        public double WorkDistance { get; set; }

        public DateTime DateJoined { get; set; }
    }
}