 using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;


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
    }
}
