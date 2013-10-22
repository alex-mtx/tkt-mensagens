
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Br.Ticket.Mensagens
{

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConsumidorFilaSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionString",IsRequired=true)]
        public string ConnectionString
        {
            get
            {

                return (string)this["connectionString"];
            }
            set
            {

                this["connectionString"]=value;
            }
        }

        [ConfigurationProperty("fila", IsRequired = true)]
        public string Fila
        {
            get
            {

                return (string)this["fila"];
            }
            set
            {

                this["fila"] = value;
            }
        }

        [ConfigurationProperty("nomeAssembly", IsRequired = true)]
        public string NomeAssembly
        {
            get
            {

                return (string)this["nomeAssembly"];
            }
            set
            {

                this["nomeAssembly"] = value;
            }
        }


        [ConfigurationProperty("nomeClasse", IsRequired = true)]
        public string NomeClasse
        {
            get
            {

                return (string)this["nomeClasse"];
            }
            set
            {

                this["nomeClasse"] = value;
            }
        }



    }
}
