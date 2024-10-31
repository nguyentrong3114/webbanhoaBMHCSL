using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using Oracle.ManagedDataAccess.Types;

namespace BMHCSDL.Data
{
    public class Des
    {
        private readonly ILogger<Des> _logger;
        private readonly IConfiguration _configuration;

        public Des(IConfiguration configuration, ILogger<Des> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public static byte[] EncryptDES(string connectionString, string plainText, string priKey)
        {
            using (var conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string function = "DES.encrypt";

                    using (OracleCommand cmd = new OracleCommand(function, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        OracleParameter resultParam = new OracleParameter
                        {
                            ParameterName = "Result",
                            OracleDbType = OracleDbType.Raw,
                            Size = 500,
                            Direction = ParameterDirection.ReturnValue
                        };
                        cmd.Parameters.Add(resultParam);

                        cmd.Parameters.Add(new OracleParameter
                        {
                            ParameterName = "p_plainText",
                            OracleDbType = OracleDbType.Varchar2,
                            Value = plainText,
                            Direction = ParameterDirection.Input
                        });

                        cmd.Parameters.Add(new OracleParameter
                        {
                            ParameterName = "priKey",
                            OracleDbType = OracleDbType.Varchar2,
                            Value = priKey,
                            Direction = ParameterDirection.Input
                        });

                        cmd.ExecuteNonQuery();

                        return resultParam.Value is OracleBinary ret ? (byte[])ret.Value : null;
                    }
                }
                catch (Exception ex)
                {
                    throw new (ex.Message);
                }
            }
        }

        public static string DecryptDES(string connectionString, byte[] encrypted, string priKey)
        {
            using (var conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string function = "DES.decrypt";

                    using (OracleCommand cmd = new OracleCommand(function, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        OracleParameter resultParam = new OracleParameter
                        {
                            ParameterName = "Result",
                            OracleDbType = OracleDbType.Varchar2,
                            Size = 100,
                            Direction = ParameterDirection.ReturnValue
                        };
                        cmd.Parameters.Add(resultParam);

                        cmd.Parameters.Add(new OracleParameter
                        {
                            ParameterName = "p_encryptedText",
                            OracleDbType = OracleDbType.Raw,
                            Value = encrypted,
                            Direction = ParameterDirection.Input
                        });

                        cmd.Parameters.Add(new OracleParameter
                        {
                            ParameterName = "priKey",
                            OracleDbType = OracleDbType.Varchar2,
                            Value = priKey,
                            Direction = ParameterDirection.Input
                        });

                        cmd.ExecuteNonQuery();

                        return resultParam.Value?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw new (ex.Message);
                }
            }
        }
    }
}
