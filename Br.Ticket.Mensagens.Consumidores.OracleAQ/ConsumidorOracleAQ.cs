using System;
using System.Collections.ObjectModel;
using System.Threading;
using Br.Ticket.Mensagens;
using Br.Ticket.Mensagens.Contratos;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;


using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.PlatformServices;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Data;
using System.Net.Mime;
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
        private bool _consumir;
        private TipoFila _tipoFila;
        private OracleConnectionStringBuilder _connectionString;
        private string _fila = String.Empty;
        private OracleAQMessageType _tipoPayload;
        private bool _configurado;


        internal delegate object DelegadoDequeue(OracleConnection conn, OracleAQQueue fila);


        public void EncerrarConsumo()
        {
            _consumir = false;
        }

        public IObservable<MensagemRecebida> Consumir()
        {
            if (!_configurado)
            {
                throw new ConstraintException("Consumidor Oracle AQ: A instância não foi configurada. Execute Configurar");
                // TODO: Log
            }

            DelegadoDequeue handler = Dequeue;


            return IniciarConsumo(handler,
        }

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

            _tipoPayload = Helper.ObtemTipoPayload();


            if (String.IsNullOrEmpty(idAplicacaoConsumidora))
                throw new ArgumentException("Consumidor Oracle AQ: Argumento Id Aplicação Consumidora não pode ser Null/Vazio");

            _idConsumidor = idAplicacaoConsumidora;

            _fila = fila;

            _configurado = true;

        }




        
        internal void ConfigurarQueueAQ(OracleAQQueue fila)
        {
            //TODO: deveriam estar em config
            fila.MessageType = Helper.ObtemTipoPayload();
            fila.DequeueOptions.Visibility = OracleAQVisibilityMode.Immediate;
            fila.DequeueOptions.Wait = 10;
            fila.DequeueOptions.ProviderSpecificType = true;
            fila.DequeueOptions.NavigationMode = OracleAQNavigationMode.FirstMessage;
            fila.DequeueOptions.DeliveryMode = OracleAQMessageDeliveryMode.PersistentOrBuffered;


        }

        internal MensagemRecebida Dequeue(OracleConnection conexao, OracleAQQueue filaAQ)
        {

            OracleTransaction txn = new OracleTransaction();
            try
            {

                //bloqueia thread e só retorna quando tiver mensagem
                EscutarFilaAQDeAcordoComTipoFila(filaAQ);

                txn = conexao.BeginTransaction();

                OracleAQMessage deqMsg = filaAQ.Dequeue();

                OracleXmlType oraXml = deqMsg.Payload as OracleXmlType;

                Console.WriteLine("[Listen Thread] Dequeued");



                //var msg = new Mensagem() { Origem = _fila, Conteudo = oraXml.Value };

                //(mensagens as ObservableCollection<Mensagem>).Add(msg);


                txn.Commit();



            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Consumidor AQ: {0}", e.Message);

                throw new Exception(String.Format("Erro no Consumidor AQ: {0}", e.Message), e);

            }
            finally
            {

                txn.Rollback();
            }


        }
        // o delegate é para poder testar sem o oracle
        internal IObservable<object> IniciarConsumo(DelegadoDequeue delegado, OracleConnection conn, OracleAQQueue fila)
        {

            return Observable.Create<object>(
           o =>
           {
               Console.WriteLine("GetProducts on threadId:{0}", Thread.CurrentThread.ManagedThreadId);
               int x = 0;
               while (_consumir)
               {

                   Thread.Sleep(500);
                   o.OnNext(Dequeue(conn, fila));
                   x++;
               }
               o.OnCompleted();
               //Console.WriteLine("______GetProducts on completed_____");

               return Disposable.Create(Dispose);

           });
        }

        internal void Dispose()
        {
            Console.WriteLine("liberar recursos");
            // TODO: liberar recursos
        }

        internal MensagemRecebida Traduz(OracleAQMessage mensagemAQ)
        {
            var encoding = Helper.OracleEncoding();

            // TODO: tratar tipos diferentes de payload
            var conteudo = encoding.GetBytes(mensagemAQ.Payload.ToString());

            // TODO: tirar hardcoded content type
            var tipoConteudo = new ContentType("text/xml;iso-8859-1");
            //TODO: traduzir

            var msg = new MensagemRecebida(conteudo, tipoConteudo);
            msg.Correlacao = mensagemAQ.Correlation;
            msg.Criador = mensagemAQ.SenderId.Name;

            //milisegundos | segundos
            msg.Expiracao = mensagemAQ.Expiration * 1000;
            msg.FilaOrigem = _fila;
            msg.Id = encoding.GetString(mensagemAQ.MessageId);
            msg.Prioridade = mensagemAQ.Priority;
            msg.TimeStampPublicacao = mensagemAQ.EnqueueTime.Ticks;

            if (mensagemAQ.DeliveryMode.Equals(OracleAQMessageDeliveryMode.Buffered))
                msg.TipoEntrega = TipoEntrega.EmMemoria;

            if (mensagemAQ.DeliveryMode.Equals(OracleAQMessageDeliveryMode.Persistent))
                msg.TipoEntrega = TipoEntrega.Persistente;

            if (mensagemAQ.DeliveryMode.Equals(OracleAQMessageDeliveryMode.PersistentOrBuffered))
                msg.TipoEntrega = TipoEntrega.Indefinido;

            return msg;


        }


        /// <summary>
        /// funcionando
        /// </summary>
        /// <param name="mensagens"></param>
        /// 
        
        internal void Listen(object mensagens)
        {

            OracleConnection conexao = new OracleConnection(_connectionString.ConnectionString);

            OracleAQQueue queueListen = new OracleAQQueue(_fila, conexao);
            try
            {
                conexao.Open();

                queueListen.MessageType = OracleAQMessageType.Xml;

                while (_consumir)
                {


                    Console.WriteLine("[Listen Thread] Listen returned...");


                    // Prepare to Dequeue
                    queueListen.DequeueOptions.Visibility = OracleAQVisibilityMode.Immediate;
                    queueListen.DequeueOptions.Wait = 10;
                    queueListen.DequeueOptions.ProviderSpecificType = true;
                    queueListen.DequeueOptions.NavigationMode = OracleAQNavigationMode.FirstMessage;
                    queueListen.DequeueOptions.DeliveryMode = OracleAQMessageDeliveryMode.PersistentOrBuffered;

                    string consumidorListener = string.Empty;

                    if (!String.IsNullOrEmpty(_idConsumidor))
                    {
                        queueListen.DequeueOptions.ConsumerName = _idConsumidor;

                        Console.WriteLine("[Listen Thread] LISTEM");

                        queueListen.Listen(new string[1] { _idConsumidor }, -1);


                        Console.WriteLine("[Listen Thread] Consumidor: {0}", _idConsumidor);
                    }
                    else
                    {
                        queueListen.Listen(null);
                    }

                    OracleTransaction txn = conexao.BeginTransaction();

                    OracleAQMessage deqMsg = queueListen.Dequeue();

                    OracleXmlType oraXml = deqMsg.Payload as OracleXmlType;

                    Console.WriteLine("[Listen Thread] Dequeued");



                    //var msg = new Mensagem() { Origem = _fila, Conteudo = oraXml.Value };

                    //(mensagens as ObservableCollection<Mensagem>).Add(msg);


                    txn.Commit();
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Erro no Consumidor AQ: {0}", e.Message);

                throw new Exception(String.Format("Erro no Consumidor AQ: {0}", e.Message), e);

            }
            finally
            {
                // Close/Dispose objects
                queueListen.Dispose();
                conexao.Close();
                conexao.Dispose();
            }
        }

        /// <summary>
        /// Encerra o consumo de mensagens.
        /// </summary>
        
        internal void EscutarFilaAQDeAcordoComTipoFila(OracleAQQueue fila)
        {

            int escutarAteQueSurjaMensagem = -1;


            switch (_tipoFila)
            {
                case TipoFila.PubSub:

                    var consumidores = new String[1] { _idConsumidor };


                    fila.DequeueOptions.ConsumerName = _idConsumidor;

                    fila.Listen(consumidores, escutarAteQueSurjaMensagem);

                    break;

                case TipoFila.P2P:

                    //exigência do ODP para filas deste tipo. Nenhum consumidor deve ser informado.
                    fila.Listen(null, escutarAteQueSurjaMensagem);


                    break;

                default:

                    throw new NotSupportedException(String.Format("Consumidor Oracle AQ: O tipo de fila informado não é suportado: {0}", _tipoFila));
                //TODO: Log
            }

        }



      
    }
}
