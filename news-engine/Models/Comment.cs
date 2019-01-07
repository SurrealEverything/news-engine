using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace news_engine.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now;
        public int ArticleId { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}