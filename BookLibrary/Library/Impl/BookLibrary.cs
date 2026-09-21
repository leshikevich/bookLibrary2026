using Library.BookLibrary.Interfaces;
using Library.BookLibrary.Models;

namespace Library.BookLibrary.Impl;

public sealed class BookLibrary : IBookLibrary
{
    private readonly IBookSerializer serializer;
    private List<Book> books = new();

    public BookLibrary(IBookSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        this.serializer = serializer;
    }

    public IEnumerable<Book> Books => books.ToArray();

    public void AddBook(Book book) => AddTo(books, book);

    public IEnumerable<Book> SortBooks() => books
        .OrderBy(b => b.Author, StringComparer.OrdinalIgnoreCase)
        .ThenBy(b => b.Title, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public IEnumerable<Book> SearchByTitle(string keyword)
    {
        ArgumentNullException.ThrowIfNull(keyword);

        return books.Where(b => b.Title
                    .Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
    }

    public void Load(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var loaded = new List<Book>();
        foreach (var book in serializer.Read(reader))
            AddTo(loaded, book);
        books = loaded;
    }

    public void Save(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        serializer.Write(writer, Books);
    }

    private static void AddTo(List<Book> target, Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        if (target.Any(b => string.Equals(b.Title, book.Title, StringComparison.OrdinalIgnoreCase)
                         && string.Equals(b.Author, book.Author, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("A book with this title and author already exists.", nameof(book));
        target.Add(book);
    }
}
