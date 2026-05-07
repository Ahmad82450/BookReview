using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookOasis.Models
{
    public class Reviews
    {
        [Key]
        public int ReviewID { get; set; }
        public string UserID { get; set; }

        public int BookID { get; set; }

        [ForeignKey(nameof(BookID))]
        public BooksModel? Book { get; set; }

        public string ReviewText { get; set; }
        public DateTime ReviewTimeStamp { get; set; }
        public int ReviewRating { get; set; }
    }
}
