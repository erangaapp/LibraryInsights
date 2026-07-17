using System.Text;

namespace WarmUpTasks;

public class Exercises
{
    // <summary>.Net core 14.0 vrsion has a built-in method
    public bool IsBookIdAPowerOfTwo_v1(int bookId)
       => int.IsPow2(bookId);

    // <summary> c# algorithm to check if a number is a power of two
    public bool IsBookIdAPowerOfTwo_v2(int bookId)
    {
        if(bookId <= 0)
            return false;

        while (bookId > 1)
        {
            if (bookId % 2 != 0)
                return false;
            bookId /= 2;
        }
        return true;
    }

    public string ReverseABookTitle(string bookTitle)
    {
        if (string.IsNullOrEmpty(bookTitle))
            return bookTitle;

        var charArray = bookTitle.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public string GenerateBookTitleReplicas(int replicas, string title)
    {
        if (replicas <= 0 || string.IsNullOrEmpty(title))
            return string.Empty;

        return string.Concat(Enumerable.
            Repeat(title, replicas));
    }

    public int[] OddNumbersList(int max = 100)
    {
        List<int> result = [];
        
        for (int i = 1; i < max; i += 2)
        {
            result.Add(i);
            Console.WriteLine($"Generated ID: {i}");
        }

        return [.. result];
    }
}
