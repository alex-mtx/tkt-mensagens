 using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
using Oracle.DataAccess.Client;


namespace Br.Ticket.Mensagens.Consumidores.OracleAQ
{
   

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal static class Helper
    {

        internal static void VerificarInstalacaoODP()
        {
            string url = "http://www.oracle.com/technetwork/topics/dotnet/";

            DataTable _table = System.Data.Common.DbProviderFactories.GetFactoryClasses();
            
            var f = _table.Select("InvariantName ='Oracle.DataAccess.Client'");

            if (f.Length.Equals(0))
                throw new Exception("ConsumidorAQ: O Driver ODP.NET (Oracle.DataAccess.Client) não está instalado. Consulte o site da Oracle: " + url);

        }

        internal static TipoFila ObtemTipoFilaDoConfig()
        {

            //TODO: criar corpo

            return TipoFila.PubSub;


        }

        internal static OracleAQMessageType TipoPayloadMensagem()
        {

            //TODO: criar corpo

            return OracleAQMessageType.Xml;


        }


        internal static Encoding OracleEncoding()
        {

            //TODO: criar corpo

            //return Encoding.GetEncoding("iso-8859-1")
                return Encoding.GetEncoding("utf-8");


        }

        internal static OracleConnectionStringBuilder ValidarStringConexao(string connectionString)
        {
            OracleConnectionStringBuilder cnb = new OracleConnectionStringBuilder();

            try
            {
                cnb = new OracleConnectionStringBuilder(connectionString);

            }
            catch (Exception e)
            {
                throw new FormatException(String.Format("Os dados da connecionString estão em formato incorreto.: {0}", e.Message), e);
            }

            return cnb;
        }
    }
}
