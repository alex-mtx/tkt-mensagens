using System;
using System.Collections.ObjectModel;
using System.Threading;
using Br.Ticket.Mensagens.Contratos;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;


namespace Br.Ticket.Mensagens.Consumidores.OracleAQ
{
    
    /// <summary>
    /// Consome mensagens disponibilizadas no Oracle AQ.
    /// Por baixo dos panos, temos o ODP.NET.
    /// </summary>
    public sealed class ConsumidorOracleAQ : IConsumidor
    {
        private string _nomeConsumidor;
        private bool _consumir;
        private string _dadosConexao = String.Empty;
        private string _fila = String.Empty;


        public void IniciarConsumo(ObservableCollection<MensagemRecebida> mensagens)
        {
            _consumir = true;
            ParameterizedThreadStart ts = new ParameterizedThreadStart(Listen);
            Thread t = new Thread(ts);
            t.Start(mensagens);


        }



        internal MensagemRecebida Traduz(OracleAQMessage mensagemAQ)
        {



            return new MensagemRecebida(null, null);

        }

        internal void Listen(object mensagens)
        {
           
            OracleConnection conexao = new OracleConnection(_dadosConexao);
           
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

                    if (!String.IsNullOrEmpty(_nomeConsumidor))
                    {
                        queueListen.DequeueOptions.ConsumerName = _nomeConsumidor;

                        Console.WriteLine("[Listen Thread] LISTEM");

                        queueListen.Listen(new string[1] { _nomeConsumidor }, -1);

               
                        Console.WriteLine("[Listen Thread] Consumidor: {0}",_nomeConsumidor);
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
        public void EncerrarConsumo()
        {
            _consumir = false;
        }


        /// <summary>
        /// Configura os dados necessários para a conexão com o AQ.
        /// 
        /// </summary>
        /// <param name="dadosConexao">Uma Conection String, em formato específico do componente</param>
        /// <param name="fila">Nome da Fila</param>
        public void Configurar(string dadosConexao, string fila)
        {
            ValidarDadosConexao(dadosConexao);

            Helper.VerificarInstalacaoODP();

            _fila = fila;

            _dadosConexao = ExtrairNomeConsumidorDadosConexao(dadosConexao); 
            
        }

        /// <summary>
        /// TODO: tratar corretamente o formato
        /// </summary>
        /// <param name="dadosConexao"></param>
        internal void ValidarDadosConexao(string dadosConexao)
        {
            if (String.IsNullOrEmpty(dadosConexao))
                throw new FormatException(String.Format("Os dados de conexão informados estão em formato incorreto. Conteúdo: {0}", dadosConexao));
            
        }

        internal string ExtrairNomeConsumidorDadosConexao(string dadosConexao)
        {
            // TODO: tratar string conexão
            int posicaoInicial = dadosConexao.IndexOf("NomeConsumidor=");
            if (posicaoInicial.Equals(-1))
                return dadosConexao;

            _nomeConsumidor = dadosConexao.Substring(posicaoInicial + "NomeConsumidor=".Length);

            return dadosConexao.Remove(posicaoInicial);

        }

    }
}
