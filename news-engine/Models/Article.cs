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
        public DateTime Date { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }

    public class ArticleDbContext : DbContext
    {
        public ArticleDbContext() : base("DbConnection") { }
        public DbSet<Article> Articles { get; set; }
    }
}
