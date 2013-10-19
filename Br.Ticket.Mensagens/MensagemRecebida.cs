using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mime;

namespace Br.Ticket.Mensagens
{
    /// <summary>
    /// Representa uma Mensagem Recebida pela rede.
    /// <para>
    /// Geralmente será criada pelo Adaptador responsável pelo recebimento.
    /// </para>
    /// A classe Mensagem deve ser utilizada para publicação.
    /// </summary>
    public class MensagemRecebida : Mensagem
    {
        /// <summary>
        /// Identificador da mensagem gerado pelo middleware.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Momento da publicação em milisegundos.
        /// <para>
        /// Utilize o método Publicacao para obter valor em DateTime.
        /// </para>
        /// </summary>
        public long TimeStampPublicacao { get; set; }

        public MensagemRecebida(byte[] conteudo, ContentType mime):base(conteudo,mime){}

 
        /// <summary>
        /// Conveniência para exibir TimeStampPublicacao em formato amigável.
        /// </summary>
        public DateTime Publicacao()
        {
            return new DateTime(TimeStampPublicacao);


        }

        public override string ToString()
        {
            var classeBase= base.ToString();

            var estaClasse = String.Format("[Id: {0}] [TimeStamp Publicação: {1}] [Data/Hora Publicação: {2}] ", Id, TimeStampPublicacao,Publicacao());

            return String.Concat(estaClasse, classeBase);
        }

    }
}
