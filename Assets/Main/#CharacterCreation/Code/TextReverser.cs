
public static class TextReverser 
{
   public static string Reverse(string origin)
   {
        int length = origin.Length;
        char[] reveresedCharacters = new char[length];
        //string reveresedString;
        for (int i = 0; i < length; i++)
        {
            char letter = origin[(length -1) - i];
            reveresedCharacters[i] = letter;
        }
        return new string(reveresedCharacters);
   }
}
