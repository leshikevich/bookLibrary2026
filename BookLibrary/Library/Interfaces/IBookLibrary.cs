using Library.BookLibrary.Models;

namespace Library.BookLibrary.Interfaces;

public interface IBookLibrary
{
    IEnumerable<Book> Books { get; }
    void AddBook(Book book);
    IEnumerable<Book> SortBooks();
    IEnumerable<Book> SearchByTitle(string keyword);
    void Load(TextReader reader);
    void Save(TextWriter writer);
}
