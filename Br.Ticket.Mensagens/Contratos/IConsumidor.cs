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
        /// <para>Útil para quando se mantém os parâmetros</para>
        /// Cada Adaptador possui requisitos próprios.
        /// </summary>
        /// <param name="connectionString">Uma Conection String, em formato específico do Consumidor</param>
        /// <param name="fila">Nome da Fila</param>
        /// <param name="idAplicacaoConsumidora">Identificação da aplicação consumidora</param>
        void Configurar(string connectionString,string fila,string idAplicacaoConsumidora);

        /// <summary>
        /// Inicia o consumo da fila informada.
        /// <para>Esta coleção é ThreadSafe.</para>
        /// </summary>
        void Consumir();

        /// <summary>
        /// Encerra o consumo, liberando os recursos utilizados.
        /// </summary>
        void EncerrarConsumo();

        event EventHandler MensagemRecebidaEvento;
    }
}
