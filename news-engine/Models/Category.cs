using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace news_engine.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        
    }

    public class CategoryDbContext : DbContext
    {
        public CategoryDbContext() : base("DbConnection") { }
        public DbSet<Category> Categories { get; set; }
    }
}