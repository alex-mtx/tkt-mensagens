using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
//using Br.Ticket.Mensageria;

namespace Br.Ticket.Mensagens.Contratos
{
    public interface IConsumidor 
    {
        /// <summary>
        /// Configura os dados necessários para a conexão com o servidor e Fila.
        /// </summary>
        /// <param name="connectionString">Uma Conection String, em formato específico do Consumidor</param>
        /// <param name="fila">Nome da Fila</param>
        /// <param name="idAplicacaoConsumidora">Identificação da aplicação consumidora</param>
        void Configurar(string connectionString,string fila,string idAplicacaoConsumidora);

        /// <summary>
        /// Inicia o consumo da fila informada.
        /// <para>Cada mensagem recebida da Fila será enviada pelo CallBack do Evento MensagemRecebidaEvento</para>
         /// </summary>
        void Consumir();

        /// <summary>
        /// Encerra o consumo, liberando os recursos utilizados.
        /// </summary>
        void EncerrarConsumo();

        /// <summary>
        /// Evento que entrega a instância da MensagemRecebida.
        /// <para>Exemplo:<example > IConsumidor.MensagemRecebidaEvento += new EventHandler(Observer_MensagemRecebida) </example></para>
        /// <para>Faça o TypeCast de EventArgs para MensagemEventArgs para acessar a MensagemRecebida no evento.</para>
        /// </summary>
        event EventHandler MensagemRecebidaEvento;
    }
}
