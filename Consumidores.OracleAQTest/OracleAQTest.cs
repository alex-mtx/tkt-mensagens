using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Net.Mime;

namespace Consumidores.OracleAQTest
{
    [TestFixture]
    public class OracleAQTest
    {

        [Test]
        public void Teste_Listen()
        {

            var x = new ContentType("text/7777xml;charset=9iso-8859-1");
            x.CharSet = "xISO-8859-1";

            x.MediaType = "text/xml";
            

        }


    }
}
