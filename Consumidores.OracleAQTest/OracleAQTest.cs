using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Net.Mime;
using Oracle.DataAccess.Client;

namespace Consumidores.OracleAQTest
{
    [TestFixture]
    public class OracleAQTest
    {

        [Test]
        public void Teste_Listen()
        {

            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder("user id=gp;password=gp;data source=//localhost:1521/xe;NomeConsumidor=GP_CONSUMER;fila=eventos");
            
            

        }


    }
}
