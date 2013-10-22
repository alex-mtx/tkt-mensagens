using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

using Br.Ticket.Mensagens.Contratos;
using Br.Ticket.Mensagens;

namespace ConsoleApp
{
    class Program
    {
        private static string _cn = "user id=gp;password=gp;data source=//localhost:1521/xe";
        private static string _fila = "gp.EVENTOSCONTRATO_Q";
        static void Main(string[] args)
        {
           

            Thread.CurrentThread.Name = "Main";

            IConsumidor queuePubSubGP = Facilitador.InstanciarConsumidor(@"H:\tkt\Br.Ticket.Mensagens.All\Br.Ticket.Mensagens.Consumidores.OracleAQ\bin\Debug\Br.Ticket.Mensagens.Consumidores.OracleAQ.dll"
                , "Br.Ticket.Mensagens.Consumidores.OracleAQ.ConsumidorOracleAQ");
            queuePubSubGP.Configurar(_cn, _fila, "GP_CONSUMER");

            queuePubSubGP.MensagemRecebidaEvento += new EventHandler(Observer_MensagemRecebida);


            queuePubSubGP.Consumir();
            
            Publica(_fila, 10);

            Console.ReadKey();

            queuePubSubGP.EncerrarConsumo();
            Console.ReadKey();

        }

       
        static private string Payload()
        {

            return String.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                                "<statusClienteAltrado xmlns=\"http://Ticket/Pedidos\">",
                                  String.Format("<Id>{0}</Id>", new Random().Next()),
                                  "<Status>Status1</Status>",
                                  String.Format("<DataHoraAtualizacao>{0}</DataHoraAtualizacao>", DateTime.Now),
                                "</statusClienteAltrado>");

        }
        private static void Publica(string fila,int mensagens)
        {
            var range = new Random();

            Console.WriteLine("Publicando");
            ThreadStart ts = new ThreadStart(() =>
            {
                //    // Create connection
                string constr = "user id=gp;password=gp;data source=//localhost:1521/xe";
                using (OracleConnection con = new OracleConnection(constr))
                {

                    // Create queue
                    OracleAQQueue queue = new OracleAQQueue(fila, con);
                    // Open connection
                    con.Open();

                    // Set message type for the queue
                    queue.MessageType = OracleAQMessageType.Xml;

                    queue.EnqueueOptions.DeliveryMode = OracleAQMessageDeliveryMode.Persistent;
                    OracleTransaction txn = null; ;

                    int x = 1;
                    while (x <= mensagens)
                    {
                        try
                        {
                            // Begin transaction for enqueue
                            txn = con.BeginTransaction();

                            // Prepare message and xml payload
                            OracleAQMessage enqMsg = new OracleAQMessage();

                            //enqMsg.Recipients = new OracleAQAgent[] { new OracleAQAgent("GP_A2") };


                            queue.MessageType = OracleAQMessageType.Xml;

                            var payload = Payload();
                            OracleXmlType oraXml = new OracleXmlType(con, payload);

                            enqMsg.Payload = oraXml;


                            // Prepare to Enqueue
                            queue.EnqueueOptions.Visibility = OracleAQVisibilityMode.OnCommit;

                            enqMsg.SenderId = new OracleAQAgent("GP_CONSUMER");

                            queue.Enqueue(enqMsg);
                            Console.WriteLine();
                            Console.WriteLine("***** Mensagem {0} enfileirada: {1}", x, enqMsg.EnqueueTime);
                            Console.WriteLine();

                            // Enqueue transaction commit
                            txn.Commit();

                           System.Threading.Thread.Sleep(500);
                            x++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Problema {0}", e.Message);

                            txn.Rollback();
                        }
                    }
                }
            });

            Thread t = new Thread(ts);
            t.Start();
        }


        public static void Observer_MensagemRecebida(object sender, EventArgs e)
        {
            Console.WriteLine("Mensagem recebida {0} ", (e as MensagemEventArgs).Mensagem.ToString());
        }
    }
}
