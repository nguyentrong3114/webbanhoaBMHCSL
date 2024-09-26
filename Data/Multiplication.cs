namespace BMHCSDL.Data
{
    public class Multiplication
    {
        public string Encrypt(string plaintext, int numRows)
        {
            if (numRows <= 1)
                return plaintext;

            char[] encrypted = new char[plaintext.Length];
            int index = 0;

            for (int i = 0; i < numRows; i++)
            {
                for (int j = i; j < plaintext.Length; j += numRows)
                {
                    encrypted[index++] = plaintext[j];
                }
            }

            return new string(encrypted);
        }

        public string Decrypt(string ciphertext, int numRows)
        {
            if (numRows <= 1)
                return ciphertext;

            char[] decrypted = new char[ciphertext.Length];
            int index = 0;

            for (int i = 0; i < numRows; i++)
            {
                for (int j = i; j < ciphertext.Length; j += numRows)
                {
                    decrypted[j] = ciphertext[index++];
                }
            }

            return new string(decrypted);
        }
    }
}