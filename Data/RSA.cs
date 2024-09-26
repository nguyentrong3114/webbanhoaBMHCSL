using System;
using System.Numerics;
using System.Text;

class SimpleRSA
{
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private static BigInteger ModInverse(BigInteger e, BigInteger phi)
    {
        BigInteger m0 = phi, t, q;
        BigInteger x0 = 0, x1 = 1;

        if (phi == 1)
            return 0;

        while (e > 1)
        {
            q = e / phi;
            t = phi;
            phi = e % phi;
            e = t;
            t = x0;
            x0 = x1 - q * x0;
            x1 = t;
        }

        if (x1 < 0)
            x1 += m0;

        return x1;
    }
    public static BigInteger Encrypt(string message)
    {
        int p = 61;
        int q = 53; 

        BigInteger n = p * q;

        BigInteger phi = (p - 1) * (q - 1);
        int e = 17; 
        if (GCD(e, (int)phi) != 1)
        {
            throw new Exception("e không coprime với phi(n).");
        }
        BigInteger messageAsBigInt = StringToBigInteger(message);


        return BigInteger.ModPow(messageAsBigInt, e, n);
    }

    public static string Decrypt(BigInteger cipherText)
    {

        int p = 61;
        int q = 53;

        BigInteger n = p * q;

        BigInteger phi = (p - 1) * (q - 1);

        int e = 17;

        BigInteger d = ModInverse(e, phi);

        BigInteger decryptedMessageAsBigInt = BigInteger.ModPow(cipherText, d, n);
        return BigIntegerToString(decryptedMessageAsBigInt);
    }

    private static BigInteger StringToBigInteger(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        return new BigInteger(bytes);
    }

    private static string BigIntegerToString(BigInteger bigInt)
    {
        byte[] bytes = bigInt.ToByteArray();
        return Encoding.UTF8.GetString(bytes);
    }
}
