namespace Library.BookLibrary.Models;

public sealed record Book
{
    public string Title { get; }
    public string Author { get; }
    public int Pages { get; }

    public Book(string title, string author, int pages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pages);
        Title = title.Trim();
        Author = author.Trim();
        Pages = pages;
    }
}
