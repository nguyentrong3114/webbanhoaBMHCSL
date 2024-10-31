using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BMHCSDL.Data;
    class CallCaesarFromDatabase
    {
        private readonly ILogger<CallCaesarFromDatabase> _logger;
        private readonly IConfiguration _configuration;
        public CallCaesarFromDatabase(IConfiguration configuration, ILogger<CallCaesarFromDatabase> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public static string Encrypt(string connectString, string text, int shift)
        {
            string encryptedText = string.Empty;

            using (var connection = new OracleConnection(connectString))
            {
                connection.Open();

                using (var command = new OracleCommand("P_CaesarEncrypt", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new OracleParameter("p_text", text));
                    command.Parameters.Add(new OracleParameter("p_shift", shift));


                    OracleParameter outputParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000);
                    outputParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParam);

                    command.ExecuteNonQuery();


                    encryptedText = outputParam.Value.ToString();
                }
            }

            return encryptedText;
        }


        public static string Decrypt(string connectString, string text, int shift)
        {
            string decryptedText = string.Empty;

            using (var connection = new OracleConnection(connectString))
            {
                connection.Open();

                using (var command = new OracleCommand("P_CaesarDecrypt", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new OracleParameter("p_text", text));
                    command.Parameters.Add(new OracleParameter("p_shift", shift));


                    OracleParameter outputParam = new OracleParameter("p_result", OracleDbType.Varchar2, 4000);
                    outputParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParam);

                    command.ExecuteNonQuery();


                    decryptedText = outputParam.Value.ToString();
                }
            }
            return decryptedText;
        }
    }
