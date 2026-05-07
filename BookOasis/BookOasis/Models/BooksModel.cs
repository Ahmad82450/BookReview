using System.ComponentModel.DataAnnotations;

namespace BookOasis.Models
{
    public class BooksModel
    {
        [Key]
        public int BookID { get; set; }
        public string bookName {  get; set; }
        public string bookISBN { get; set; }
        public string bookAuthor { get; set; }
        public string bookDescription { get; set; }
        public DateTime bookReleaseDate { get; set; }

        public ICollection<Reviews> Reviews { get; set; }
    }
}
