using Library.BookLibrary.Models;

namespace Library.BookLibrary.Interfaces;

public interface IBookSerializer
{
    IEnumerable<Book> Read(TextReader reader);
    void Write(TextWriter writer, IEnumerable<Book> books);
}
