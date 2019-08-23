using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Quack.Models
{
    public class BotModel
    {
        [Key]
        public int ID { get; set; }

        public int? userID { get; set; }

        public virtual string seed { get; set; }

        public int minWords { get; set; }

        public int maxWords { get; set; }

        public float postProbability { get; set; }
    }
}
