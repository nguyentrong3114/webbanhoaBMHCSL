using System;
using System.Numerics;
using System.Text;


namespace BMHCSDL.Data
{
    public class SimpleRSA
    {
        public BigInteger p; 
        public BigInteger q; 
        public BigInteger N; 
        public BigInteger phi;
        public BigInteger e; 
        public BigInteger d; 
        private static Random random = new Random();

        public SimpleRSA(int keySize)
        {
            GenerateKeys(keySize);
        }

        // Sinh cặp khóa RSA
        public void GenerateKeys(int bitSize)
        {
            List<BigInteger> primes = GeneratePrimes(bitSize);
            p = primes[0];
            q = primes[1];
            N = p * q;
            phi = (p - 1) * (q - 1);
            e = FindE(phi);
            d = CalculateD(e, phi);
        }

        public BigInteger[] Encrypt(string plaintext)
        {
            char[] chars = plaintext.ToCharArray();

            BigInteger[] encryptedValues = new BigInteger[chars.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(chars[i].ToString());
                BigInteger encryptedValue = BigInteger.ModPow(new BigInteger(asciiBytes), e, N);
                encryptedValues[i] = encryptedValue;
            }

            return encryptedValues;
        }

        public string Decrypt(BigInteger[] encryptedValues)
        {
            byte[] bytes = new byte[encryptedValues.Length];

            for (int i = 0; i < encryptedValues.Length; i++)
            {
                BigInteger decryptedValue = BigInteger.ModPow(encryptedValues[i], d, N);

                bytes[i] = decryptedValue.ToByteArray()[0];
            }

            string message = Encoding.ASCII.GetString(bytes); // Chuyển đổi mảng byte thành chuỗi

            return message;
        }

        // Sinh các số nguyên tố cần thiết cho việc tạo khóa RSA
        public List<BigInteger> GeneratePrimes(int bitSize)
        {
            List<BigInteger> primes = new List<BigInteger>();

            while (primes.Count < 2)
            {
                // Tạo một số nguyên tố với kích thước bit là bitSize
                BigInteger prime = GeneratePrime(bitSize);

                // Kiểm tra tính duy nhất của số nguyên tố
                if (!primes.Contains(prime))
                {
                    primes.Add(prime); // Thêm số nguyên tố vào danh sách
                }
            }

            return primes;
        }

        private BigInteger GeneratePrime(int bitSize)
        {
            BigInteger prime;
            do
            {
                // Tạo một mảng byte với kích thước dựa trên bitSize
                byte[] buffer = new byte[bitSize / 8];
                random.NextBytes(buffer); // Sinh ngẫu nhiên các giá trị byte
                prime = new BigInteger(buffer); // Tạo số nguyên từ dãy byte
                prime = BigInteger.Abs(prime); // Đảm bảo số nguyên là dương
            } while (!IsPrime(prime)); // Kiểm tra xem số vừa tạo có phải là số nguyên tố không
            return prime;
        }

        // Kiểm tra xem một số có phải là số nguyên tố hay không
        private bool IsPrime(BigInteger number)
        {
            if (number <= BigInteger.One)
                return false; // Trả về false nếu số đưa vào nhỏ hơn hoặc bằng 1
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0 || number % 3 == 0)
                return false;
            BigInteger sqrt = Sqrt(number);
            for (BigInteger i = 5; i <= sqrt; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }
            return true;
        }

        // Tính căn bậc hai của một số nguyên dương
        private BigInteger Sqrt(BigInteger number)
        {
            if (number == BigInteger.Zero)
                return BigInteger.Zero;
            BigInteger sqrt = number;
            BigInteger prev = BigInteger.Zero;
            while (sqrt != prev)
            {
                prev = sqrt;
                sqrt = (sqrt + number / prev) / 2; // Áp dụng phép tính căn bậc hai Babylonian
            }
            return prev;
        }
        private BigInteger FindE(BigInteger phi)
        {
            BigInteger e = 5;
            while (e < phi)
            {
                if (IsCoprime(e, phi))
                    return e;
                e++;
            }
            // If no suitable e value is found, return 0
            return BigInteger.Zero;
        }

        // Check if two numbers are coprime
        private bool IsCoprime(BigInteger a, BigInteger b)
        {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(a, b);
            return gcd == BigInteger.One;
        }

        // Calculate the value of d using the extended Euclidean algorithm
        private BigInteger CalculateD(BigInteger e, BigInteger phi)
        {
            BigInteger d = ExtendedEuclideanAlgorithm(e, phi);
            d = (d % phi + phi) % phi; // Ensure d is positive
            return d;
        }

        // Extended Euclidean algorithm
        private BigInteger ExtendedEuclideanAlgorithm(BigInteger a, BigInteger b)
        {
            BigInteger oldR = a;
            BigInteger r = b;
            BigInteger oldS = BigInteger.One;
            BigInteger s = BigInteger.Zero;
            BigInteger oldT = BigInteger.Zero;
            BigInteger t = BigInteger.One;

            while (r != BigInteger.Zero)
            {
                BigInteger quotient = oldR / r;

                BigInteger temp = oldR;
                oldR = r;
                r = temp - quotient * r;

                temp = oldS;
                oldS = s;
                s = temp - quotient * s;

                temp = oldT;
                oldT = t;
                t = temp - quotient * t;
            }

            return oldS;
        }
        
    }
}