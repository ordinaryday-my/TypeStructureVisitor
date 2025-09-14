namespace TypeStructureVisitor;

public static class StringExtension
{
    public static string EncloseInQuotes(this string self, string quote = "\" \"")
    {
        var quoteArr =  quote.Split(" ");
        if (quoteArr.Length != 2)
        {
            throw new FormatException("Invalid quote format");
        }

        return $"{quoteArr[0]}{self}{quoteArr[1]}";
    }
}