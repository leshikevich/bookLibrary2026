using Library.BookLibrary.Impl;
using Library.BookLibrary.Interfaces;
using Library.BookLibrary.Models;

namespace BookLibraryTests;

public class BookLibraryTests
{
    private static BookLibrary Create() => new(new XmlBookSerializer());
    private static readonly Book First = new("The Shining", "Stephen King", 447);

    [Fact]
    public void Add_ExposesBookDirectly()
    {
        var library = Create();
        library.AddBook(First);
        Assert.Equal(First, Assert.Single(library.Books));
    }

    [Fact]
    public void Add_RejectsNullAndDuplicateWithoutChangingBooks()
    {
        var library = Create();
        library.AddBook(First);
        Assert.Throws<ArgumentNullException>(() => library.AddBook(null!));
        Assert.Throws<ArgumentException>(() =>
            library.AddBook(new Book(" THE SHINING ", "stephen king", 500)));
        Assert.Equal(First, Assert.Single(library.Books));
    }

    [Fact]
    public void Add_AllowsSameTitleWithDifferentAuthor()
    {
        var library = Create();
        library.AddBook(First);
        library.AddBook(new Book(First.Title, "Another author", 10));
        Assert.Equal(2, library.Books.Count());
    }

    [Theory]
    [InlineData(null, "Author", 1)]
    [InlineData("", "Author", 1)]
    [InlineData("  ", "Author", 1)]
    [InlineData("Title", null, 1)]
    [InlineData("Title", "", 1)]
    [InlineData("Title", "  ", 1)]
    [InlineData("Title", "Author", 0)]
    [InlineData("Title", "Author", -1)]
    public void Book_RejectsInvalidValues(string? title, string? author, int pages)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Book(title!, author!, pages));
    }

    [Fact]
    public void Sort_OrdersByAuthorThenTitleWithoutChangingLibrary()
    {
        var library = Create();
        var input = new[] { new Book("B", "King", 1), new Book("a", "king", 2), new Book("Z", "Andersen", 3) };
        foreach (var book in input) library.AddBook(book);
        Assert.Equal(new[] { input[2], input[1], input[0] }, library.SortBooks());
        Assert.Equal(input, library.Books);
    }

    [Theory]
    [InlineData("SHIN", 1)]
    [InlineData("missing", 0)]
    [InlineData("", 1)]
    public void Search_MatchesSubstringIgnoringCase(string query, int count)
    {
        var library = Create();
        library.AddBook(First);
        Assert.Equal(count, library.SearchByTitle(query).Count());
    }

    [Fact]
    public void EmptyLibrary_SupportsViewingSearchingAndSorting()
    {
        var library = Create();
        Assert.Empty(library.Books);
        Assert.Empty(library.SearchByTitle("anything"));
        Assert.Empty(library.SortBooks());
    }

    [Fact]
    public void Results_AreIndependentSnapshots()
    {
        var library = Create();
        library.AddBook(First);
        var results = new[] { library.Books, library.SearchByTitle(""), library.SortBooks() };
        library.AddBook(new Book("Carrie", "King", 199));
        foreach (var result in results)
        {
            Assert.Single(result);
            if (result is IList<Book> mutable) mutable[0] = new Book("Changed", "Someone", 1);
        }
        Assert.Equal(First, library.Books.First());
        Assert.Equal(2, library.Books.Count());
    }

    [Fact]
    public void PublicMethods_RejectNullDependenciesAndArguments()
    {
        var library = Create();
        Assert.Throws<ArgumentNullException>(() => new BookLibrary(null!));
        Assert.Throws<ArgumentNullException>(() => library.SearchByTitle(null!));
        Assert.Throws<ArgumentNullException>(() => library.Load(null!));
        Assert.Throws<ArgumentNullException>(() => library.Save(null!));
    }

    [Fact]
    public void Library_UsesInjectedSerializer()
    {
        var serializer = new MemorySerializer();
        var library = new BookLibrary(serializer);
        library.AddBook(First);
        using var writer = new StringWriter();
        library.Save(writer);
        var restored = new BookLibrary(serializer);
        using var reader = new StringReader("");
        restored.Load(reader);
        Assert.Equal(First, Assert.Single(restored.Books));
    }

    private sealed class MemorySerializer : IBookSerializer
    {
        private Book[] data = Array.Empty<Book>();
        public IEnumerable<Book> Read(TextReader reader) => data;
        public void Write(TextWriter writer, IEnumerable<Book> books) => data = books.ToArray();
    }
}
