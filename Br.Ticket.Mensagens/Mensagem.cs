using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mime;

namespace Br.Ticket.Mensagens
{



    /// <summary>
    /// Uma mensagem a ser publicada.
    /// </summary>
    public class Mensagem
    {


        
        private int? _prioridade = null;

        /// <summary>
        /// Utilizado para correlacionar esta mensagem com um processo.
        /// </summary>
        public string Correlacao { get; set; }
        /// <summary>
        /// Identificação da aplicação criadora da mensagem.
        /// </summary>
        public string Criador { get; set; }

        public IDictionary<string, string> Cabecalhos { get; set; }

        /// <summary>
        /// Informa a fila de origem
        /// </summary>
        public string FilaOrigem { get; set; }


        
        /// <summary>
        /// Tempo em segundos para a expiração da mensagem depois de publicada.
        /// <para>
        /// Null para não expirar.
        /// </para>
        /// </summary>
        public int Expiracao {get;set;}

        /// <summary>
        /// 0 (zero) é mais importante.
        /// <para>
        /// Null para não priorizar.
        /// </para>
        /// </summary>
        /// <value></value>
        public int? Prioridade
        {
            get { return _prioridade; }
            set { _prioridade = value; }
        }
        /// <summary>
        /// O conteúdo está sempre ligado a um charset que pode ser diferente do atual.
        /// <para>
        /// Utilize o TipoConteudo para saber/informar qual charset correto.
        /// </para>
        /// </summary>
        public byte[] Conteudo { get; private set; }

        public TipoEntrega TipoEntrega { get; set; }

        /// <summary>
        /// Segue os padrões IANA.
        /// <para>
        /// Para os media type consulte: 
        /// <seealso cref="https://www.ietf.org/assignments/media-types/"/>
        /// </para>
        /// <see cref=""/>
        /// <para><example>text/plain</example>
        /// <example>text/xml</example>
        /// <example>application/json</example></para>
        /// 
        /// <para>
        /// Para os char sets consulte: 
        /// <seealso cref="http://www.iana.org/assignments/character-sets/character-sets.xhtml"/>
        /// </para>
        /// exemplos
        /// <para>
        /// <example>utf-8</example>
        /// <example>ascii</example>
        /// </para>
        /// </summary>
        public ContentType TipoConteudo { get; private set; }

        /// <summary>
        /// Uma mensagem obrigatoriamente deve ter conteúdo e tipo de conteúdo.
        /// </summary>
        /// <param name="conteudo"></param>
        /// <param name="mime">Informa o consumidor da mensagem qual o tipo do conteúdo e qual encoding</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Mensagem(byte[] conteudo, ContentType mime)
        {

            if (conteudo == null)
                throw new ArgumentNullException("Conteúdo não pode ser nulo");

            if (mime == null)
                throw new ArgumentNullException("O ContentType não pode ser nulo");


            Conteudo = conteudo;
            

            TipoConteudo = mime;
        }

        public override string ToString()
        {
            var inicio = String.Format("[Correlação: {0}] [Criador: {1}] [Expiração: {2}] [Origem: {3}] [Prioridade: {4}] [Tipo Conteúdo: {5}] [Tipo Entrega: {6}] "
                               , Correlacao, Criador, Expiracao, FilaOrigem, Prioridade, TipoConteudo.ToString(), TipoEntrega);



            var fim = Environment.NewLine +
                               "[Conteúdo:"
                               + Environment.NewLine +
                               Facilitador.ConverteEmTextoNoEncodingEsperado(Conteudo, TipoConteudo)
                               + Environment.NewLine +
                               " ]";

            return String.Concat(inicio, fim);
        }


    }
}
