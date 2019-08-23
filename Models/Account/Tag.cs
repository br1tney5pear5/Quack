using System;
using Microsoft.AspNetCore.Identity;
using Quack.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quack.Models
{
    public class Tag
    {
        public int ID { get; set; }
        public string name { get; set; }
    }
}
