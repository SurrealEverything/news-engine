using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace news_engine.Models
{
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
     
    }

    public class ArticleDbContext : DbContext
    {
        public ArticleDbContext() : base("DefaultConnection") { }
        public DbSet<Article> Articles { get; set; }
    }
}
