
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

        [ConfigurationProperty("assemblyPluginConsumidor", IsRequired = true)]
        public string AssemblyPluginConsumidor
        {
            get
            {

                return (string)this["assemblyPluginConsumidor"];
            }
            set
            {

                this["assemblyPluginConsumidor"] = value;
            }
        }


        [ConfigurationProperty("classePluginConsumidor", IsRequired = true)]
        public string ClassePluginConsumidor
        {
            get
            {

                return (string)this["classePluginConsumidor"];
            }
            set
            {

                this["classePluginConsumidor"] = value;
            }
        }

        [ConfigurationProperty("idAplicacaoConsumidora", IsRequired = true)]
        public string IdAplicacaoConsumidora
        {
            get
            {

                return (string)this["idAplicacaoConsumidora"];
            }
            set
            {

                this["idAplicacaoConsumidora"] = value;
            }
        }


    }
}
