using System;
using System.Net.Mime;
using System.Threading;
using Br.Ticket.Mensagens.Contratos;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Text;


namespace Br.Ticket.Mensagens.Consumidores.OracleAQ
{

    /// <summary>
    /// Consome mensagens disponibilizadas no Oracle AQ.
    /// Por baixo dos panos, temos o ODP.NET.
    /// <remarks>Uma instância por fila</remarks>
    /// </summary>
    public sealed class ConsumidorOracleAQ : Observable,IConsumidor
    {
        private string _idConsumidor;
        protected bool _consumir;
        private TipoFila _tipoFila;
        private OracleConnectionStringBuilder _connectionString;
        private string _fila = String.Empty;
        private OracleAQMessageType _tipoPayload;
        private bool _configurado;
        private static string _nomeThreadConsumir = "ConsumidorOracleAQ_Consumir";
        private static  ConsumidorFilaSection _configFila;
      
        


      
        /// <summary>
        /// Configura os dados necessários para a conexão com o AQ.
        /// 
        /// </summary>
        /// <param name="dadosConexao">Uma Conection String, em formato específico do componente</param>
        /// <param name="fila">Nome da Fila</param>
        public void Configurar(string connectionString, string fila, string idAplicacaoConsumidora)
        {
            Helper.VerificarInstalacaoODP();

            _connectionString = Helper.ValidarStringConexao(connectionString);

            _tipoFila = Helper.ObtemTipoFilaDoConfig();

            _tipoPayload = Helper.TipoPayloadMensagem();


            if (String.IsNullOrEmpty(idAplicacaoConsumidora))
                throw new ArgumentException("Consumidor Oracle AQ: Argumento Id Aplicação Consumidora não pode ser Null/Vazio");

            _idConsumidor = idAplicacaoConsumidora;

            _fila = fila;

            

            _configurado = true;

        }

        private void Queue_MessageAvailable(object src, OracleAQMessageAvailableEventArgs arg)
        {


            Console.WriteLine("Queue_MessageAvailable {0}", Thread.CurrentThread.ManagedThreadId);
            _consumir = true;

            byte[] idMensagemDisponivel = arg.MessageId[0];

            try
            {
                using (OracleConnection conexao = new OracleConnection(_connectionString.ConnectionString))
                {

                    conexao.Open();
                    using (OracleAQQueue queueListen = new OracleAQQueue(_fila, conexao))
                    {
                        ConfigurarQueueAQ(queueListen);

                        queueListen.DequeueOptions.ConsumerName = _idConsumidor;
                        var aqMsg = queueListen.Dequeue();

                        MensagemRecebida msg = Traduzir(aqMsg);
                        Notificar(msg);
                    }

                    Console.WriteLine("Consumir: Saiu loop");


                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Queue_MessageAvailable {0}", e.Message);
                throw e;
            }


        }

        

        public void ConsumirMono()
        {

            Console.WriteLine("Consumir: Thread {0}", Thread.CurrentThread.ManagedThreadId);
            _consumir = true;

           
                using (OracleConnection conexao = new OracleConnection(_connectionString.ConnectionString))
                {

                    conexao.Open();
                    using (OracleAQQueue queueListen = new OracleAQQueue(_fila, conexao))
                    {
                        var tx = conexao.BeginTransaction();
                        ConfigurarQueueAQ(queueListen);
                        queueListen.NotificationConsumers = new string[1]{ _idConsumidor};
                        queueListen.MessageAvailable += new OracleAQMessageAvailableEventHandler(Queue_MessageAvailable);
                        tx.Commit();
                        
                     
                    }

                    
                }
                
        }

           
        


        public void Consumir()
        {

            Console.WriteLine("Consumir: Thread {0}", Thread.CurrentThread.ManagedThreadId);
            _consumir = true;

            Thread t = new Thread(() =>
                    {
                        Console.WriteLine("Consumir - new thread: Thread {0}", Thread.CurrentThread.ManagedThreadId);

                        //while (_evento.GetInvocationList().Length == 0)
                        //{
                        //    //TODO: controlar melhor este fluxo

                        //    Thread.Sleep(500);
                        //}

                        using (OracleConnection conexao = new OracleConnection(_connectionString.ConnectionString))
                        {
                            conexao.Open();

                            using (var listenQueue = new OracleAQQueue(_fila, conexao))
                            {
                                ConfigurarQueueAQ(listenQueue);


                                var x = 1;
                                while (_consumir)
                                {
                                    Console.WriteLine("Consumir LOOP: Thread {0}", Thread.CurrentThread.Name, x);
                                    Thread.Sleep(500);

                                    var aqMsg = Dequeue(conexao, listenQueue);

                                    MensagemRecebida msg = Traduzir(aqMsg);
                                    x++;

                                    
                                    Notificar(msg);


                                }

                                Console.WriteLine("Consumir: Saiu loop");


                            }
                        }

                    });

            
            t.Name = _nomeThreadConsumir;
            t.Start();
            t.Join(1000);

            Console.WriteLine("Consumir: Saiu Thread {0}", Thread.CurrentThread.Name);
            Console.WriteLine("Consumir: Entrou Thread main {0}", Thread.CurrentThread.Name);
        }

        public void EncerrarConsumo()
        {
            // TODO: Log
            _consumir = false;
            
            Console.WriteLine("Parando Publisher");
            
            
        }
        
        internal void ConfigurarQueueAQ(OracleAQQueue fila)
        {
            //TODO: deveriam estar em config
            fila.MessageType = Helper.TipoPayloadMensagem();
            fila.DequeueOptions.Visibility = OracleAQVisibilityMode.Immediate;
            fila.DequeueOptions.Wait = 10;
            fila.DequeueOptions.ProviderSpecificType = true;
            fila.DequeueOptions.NavigationMode = OracleAQNavigationMode.FirstMessage;
            fila.DequeueOptions.DeliveryMode = OracleAQMessageDeliveryMode.PersistentOrBuffered;


        }

        internal OracleAQMessage Dequeue(OracleConnection conexao, OracleAQQueue filaAQ)
        {

            OracleTransaction txn = null;
            OracleAQMessage deqMsg;

            Console.WriteLine("Dequeue: Thread {0}", Thread.CurrentThread.Name);

            try
            {
                // TODO: Log
                // TODO: receber Delegate no lugar do método abaixo para facilitar teste unitário

                //bloqueia thread e só retorna quando tiver mensagem
                EscutarFilaAQDeAcordoComTipoSeuTipo(filaAQ);

                txn = conexao.BeginTransaction();
                Console.WriteLine("Retirando mensagem na fila");
                deqMsg = filaAQ.Dequeue();

                Console.WriteLine("Mensagem {0} retirada",Encoding.UTF8.GetString(deqMsg.MessageId));
                txn.Commit();
                

            }
            catch (Exception e)
            {
                txn.Rollback();
                
                //TODO: Log
                Console.WriteLine("Erro no Consumidor AQ: {0}", e.Message);

                throw new Exception(String.Format("Erro no Consumidor AQ: {0}", e.Message), e);

            }
            finally
            {

                
                txn.Dispose();
                txn = null;
            }

            return deqMsg;


        }
    

        internal MensagemRecebida Traduzir(OracleAQMessage mensagemAQ)
        {
            var encoding = Helper.OracleEncoding();

            // TODO: tratar tipos diferentes de payload: Raw. XML, UTD
            var conteudo = encoding.GetBytes((mensagemAQ.Payload as OracleXmlType).Value);

            // TODO: tornar configuravel
            var tipoConteudo = new ContentType("text/xml;charset=iso-8859-1");
            
            var msg = new MensagemRecebida(conteudo, tipoConteudo);
            msg.Correlacao = mensagemAQ.Correlation;
            msg.Criador = mensagemAQ.SenderId.Name;

            msg.Expiracao = mensagemAQ.Expiration;
            msg.FilaOrigem = _fila;

            //TODO: corrigir problemas com GUID
            msg.Id = encoding.GetString(mensagemAQ.MessageId);
            msg.Prioridade = mensagemAQ.Priority;
            msg.TimeStampPublicacao = mensagemAQ.EnqueueTime.Ticks;

            msg.TipoEntrega = ObterTipoEntrega(mensagemAQ.DeliveryMode);

            return msg;


        }


        internal TipoEntrega ObterTipoEntrega(OracleAQMessageDeliveryMode modo)
        {
            if (modo.Equals(OracleAQMessageDeliveryMode.Buffered))
                return TipoEntrega.EmMemoria;

            if (modo.Equals(OracleAQMessageDeliveryMode.Persistent))
                return TipoEntrega.Persistente;

                return TipoEntrega.Indefinido;

        }


        /// <summary>
        /// funcionando
        /// </summary>
        /// <param name="mensagens"></param>
        /// 
        
        //internal void Listen(object mensagens)
        //{

        //    OracleConnection conexao = new OracleConnection(_connectionString.ConnectionString);

        //    OracleAQQueue queueListen = new OracleAQQueue(_fila, conexao);
        //    try
        //    {
        //        conexao.Open();

        //        queueListen.MessageType = OracleAQMessageType.Xml;

        //        while (_consumir)
        //        {


        //            Console.WriteLine("[Listen Thread] Listen returned...");


        //            // Prepare to Dequeue
        //            queueListen.DequeueOptions.Visibility = OracleAQVisibilityMode.OnCommit;
        //            queueListen.DequeueOptions.Wait = 10;
        //            queueListen.DequeueOptions.ProviderSpecificType = true;
        //            queueListen.DequeueOptions.NavigationMode = OracleAQNavigationMode.FirstMessage;
        //            queueListen.DequeueOptions.DeliveryMode = OracleAQMessageDeliveryMode.Persistent;

        //            string consumidorListener = string.Empty;

        //            if (!String.IsNullOrEmpty(_idConsumidor))
        //            {
        //                queueListen.DequeueOptions.ConsumerName = _idConsumidor;

        //                Console.WriteLine("[Listen Thread] LISTEM");

        //                queueListen.Listen(new string[1] { _idConsumidor }, -1);


        //                Console.WriteLine("[Listen Thread] Consumidor: {0}", _idConsumidor);
        //            }
        //            else
        //            {
        //                queueListen.Listen(null);
        //            }

        //            OracleTransaction txn = conexao.BeginTransaction();

        //            OracleAQMessage deqMsg = queueListen.Dequeue();

        //            OracleXmlType oraXml = deqMsg.Payload as OracleXmlType;

        //            Console.WriteLine("[Listen Thread] Dequeued");



        //            //var msg = new Mensagem() { Origem = _fila, Conteudo = oraXml.Value };

        //            //(mensagens as ObservableCollection<Mensagem>).Add(msg);


        //            txn.Commit();
        //        }


        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Erro no Consumidor AQ: {0}", e.Message);

        //        throw new Exception(String.Format("Erro no Consumidor AQ: {0}", e.Message), e);

        //    }
        //    finally
        //    {
        //        // Close/Dispose objects
        //        queueListen.Dispose();
        //        conexao.Close();
        //        conexao.Dispose();
        //    }
        //}



        internal void EscutarFilaAQDeAcordoComTipoSeuTipo(OracleAQQueue fila)
        {

            int timeout = -1;
            Console.WriteLine("Escudanto a fila {0}", fila.Name);

            switch (_tipoFila)
            {
                case TipoFila.PubSub:

                    var consumidores = new String[1] { _idConsumidor };


                    fila.DequeueOptions.ConsumerName = _idConsumidor;

                    fila.Listen(consumidores, timeout);
                    

                    break;

                case TipoFila.P2P:

                    //exigência do ODP para filas deste tipo. Nenhum consumidor deve ser informado.
                    fila.Listen(null, timeout);


                    break;

                default:

                    throw new NotSupportedException(String.Format("Consumidor Oracle AQ: O tipo de fila informado não é suportado: {0}", _tipoFila));
                //TODO: Log
            }

            Console.WriteLine("Mensagem disponível na fila");

        }



      
    }
}
