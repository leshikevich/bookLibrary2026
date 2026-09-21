using Library.BookLibrary.Impl;
using Library.BookLibrary.Models;

namespace BookLibraryTests;

public sealed class XmlBookSerializerTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "BookLibraryTests-" + Guid.NewGuid());
    private string FilePath => Path.Combine(directory, "books.xml");
    private static BookLibrary Create() => new(new XmlBookSerializer());

    public XmlBookSerializerTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, recursive: true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void File_RoundTripSupportsBooksAndEmptyCollection(bool empty)
    {
        var library = Create();
        if (!empty) library.AddBook(new Book("Книга <C#> & XML", "Автор", 200));
        using (var writer = File.CreateText(FilePath)) library.Save(writer);
        var restored = Create();
        using (var reader = File.OpenText(FilePath)) restored.Load(reader);
        Assert.Equal(library.Books, restored.Books);
    }

    [Fact]
    public void String_RoundTripDoesNotCloseCallerOwnedReaderOrWriter()
    {
        var library = Create();
        library.AddBook(new Book("Title", "Author", 10));
        using var writer = new StringWriter();
        library.Save(writer);
        var restored = Create();
        using var reader = new StringReader(writer.ToString());
        restored.Load(reader);
        Assert.Equal(library.Books, restored.Books);
        Assert.Equal(-1, reader.Peek());
        writer.Write("still open");
    }

    [Fact]
    public void Load_ReadsOriginalXmlFormatAndReplacesExistingBooks()
    {
        var library = Create();
        library.AddBook(new Book("Old", "Author", 1));
        using var reader = new StringReader("<Books><Book><Title>New</Title><Author>Writer</Author><Pages>42</Pages></Book></Books>");
        library.Load(reader);
        Assert.Equal(new Book("New", "Writer", 42), Assert.Single(library.Books));
    }

    [Fact]
    public void Load_EmptyXmlCollectionClearsLibrary()
    {
        var library = Create();
        library.AddBook(new Book("Old", "Author", 1));
        using var reader = new StringReader("<Books />");
        library.Load(reader);
        Assert.Empty(library.Books);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not XML")]
    [InlineData("<Books>")]
    [InlineData("<WrongRoot />")]
    [InlineData("<Books><Book><Title>T</Title><Author>A</Author><Pages>abc</Pages></Book></Books>")]
    public void Load_RejectsInvalidXmlAndPreservesState(string xml)
    {
        var library = Create();
        var original = new Book("Original", "Author", 10);
        library.AddBook(original);
        using var reader = new StringReader(xml);
        Assert.Throws<InvalidOperationException>(() => library.Load(reader));
        Assert.Equal(original, Assert.Single(library.Books));
    }

    [Theory]
    [InlineData("<Book><Author>A</Author><Pages>1</Pages></Book>")]
    [InlineData("<Book><Title>T</Title><Pages>1</Pages></Book>")]
    [InlineData("<Book><Title>T</Title><Author>A</Author></Book>")]
    [InlineData("<Book><Title>T</Title><Author>A</Author><Pages>-1</Pages></Book>")]
    [InlineData("<Book><Title>Good</Title><Author>A</Author><Pages>1</Pages></Book><Book><Title>good</Title><Author>a</Author><Pages>2</Pages></Book>")]
    public void Load_RejectsInvalidBooksOrDuplicatesWithoutPartialReplacement(string books)
    {
        var library = Create();
        var original = new Book("Original", "Author", 10);
        library.AddBook(original);
        using var reader = new StringReader("<Books>" + books + "</Books>");
        Assert.ThrowsAny<ArgumentException>(() => library.Load(reader));
        Assert.Equal(original, Assert.Single(library.Books));
    }

    [Fact]
    public void Load_MissingFileThrowsAndPreservesState()
    {
        var library = Create();
        var original = new Book("Original", "Author", 1);
        library.AddBook(original);
        Assert.Throws<FileNotFoundException>(() =>
        {
            using var reader = File.OpenText(FilePath);
            library.Load(reader);
        });
        Assert.Equal(original, Assert.Single(library.Books));
    }

    [Fact]
    public void Save_FileLockedForExclusiveAccessThrows()
    {
        var library = Create();
        using var locked = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        Assert.Throws<IOException>(() =>
        {
            using var writer = File.CreateText(FilePath);
            library.Save(writer);
        });
    }

    [Fact]
    public void Save_WriteFailureIsNotSwallowed()
    {
        var library = Create();
        library.AddBook(new Book("T", "A", 1));
        using var writer = new FailingWriter();
        Assert.Throws<IOException>(() => library.Save(writer));
        Assert.Single(library.Books);
    }

    private sealed class FailingWriter : StringWriter
    {
        public override void Write(char value) => throw new IOException("Write failed.");
        public override void Write(string? value) => throw new IOException("Write failed.");
        public override void Write(char[] buffer, int index, int count) => throw new IOException("Write failed.");
    }
}
