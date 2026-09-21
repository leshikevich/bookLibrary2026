using System.Xml.Serialization;
using Library.BookLibrary.Interfaces;
using Library.BookLibrary.Models;

namespace Library.BookLibrary.Impl;

public sealed class XmlBookSerializer : IBookSerializer
{
    private static readonly XmlSerializer Serializer = new(typeof(BookData[]), new XmlRootAttribute("Books"));

    public IEnumerable<Book> Read(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var data = (BookData[]?)Serializer.Deserialize(reader)
            ?? throw new InvalidOperationException("Expected a Books collection.");
        return data.Select(b => new Book(b.Title, b.Author, b.Pages)).ToArray();
    }

    public void Write(TextWriter writer, IEnumerable<Book> books)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(books);
        var data = books.Select(b => new BookData { Title = b.Title, Author = b.Author, Pages = b.Pages }).ToArray();
        Serializer.Serialize(writer, data);
    }

    [XmlType("Book")]
    public sealed class BookData
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int Pages { get; set; }
    }
}
