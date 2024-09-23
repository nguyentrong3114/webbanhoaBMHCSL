namespace BMHCSDL.Data;
class Caesar
{
    public static string CaesarEncrypt(string input, int shift)
    {
        char[] buffer = input.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char letter = buffer[i];
            if (char.IsLetter(letter))
            {
                char letterOffset = char.IsUpper(letter) ? 'A' : 'a';
                letter = (char)((((letter + shift) - letterOffset) % 26) + letterOffset);
            }
            buffer[i] = letter;
        }
        return new string(buffer);
    }

    public static string CaesarDecrypt(string input, int shift)
    {
        return CaesarEncrypt(input, 26 - shift); 
    }
}