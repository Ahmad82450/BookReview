using System.ComponentModel.DataAnnotations;

namespace BookOasis.Models
{
    public class BooksDisplayModel
    {
        [Key]
        public int BookID { get; set; }
        public string bookName {  get; set; }
        public string bookISBN { get; set; }
        public string bookAuthor { get; set; }
        public string bookDescription { get; set; }
        public DateTime bookReleaseDate { get; set; }
    }
}
