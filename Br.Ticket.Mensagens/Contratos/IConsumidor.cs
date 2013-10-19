using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
//using Br.Ticket.Mensageria;

namespace Br.Ticket.Mensagens.Contratos
{
    public interface IConsumidor 
    {
        /// <summary>
        /// Configura os dados necessários para a conexão com o servidor e Fila.
        /// Cada Adaptador possui requisitos próprios.
        /// </summary>
        /// <param name="dadosConexao">Uma Conection String, em formato específico do Consumidor</param>
        /// <param name="fila">Nome da Fila</param>
        void Configurar(string dadosConexao,string fila);

        /// <summary>
        /// Inicia o consumo da fila informada.
        /// <para>Cada mensagem recebida é adicionada à coleção de <paramref name="mensagens"/></para>
        /// <para>Esta coleção lança diversos eventos, os quais podem ser observados pelo código cliente.</para>
        /// </summary>
        /// <param name="mensagens">A lista Observale aonde cada mensagem recebida será adicionada</param>
        void IniciarConsumo(ObservableCollection<Mensagem> mensagens);

        /// <summary>
        /// Encerra o consumo, liberando os recursos utilizados.
        /// </summary>
        void EncerrarConsumo();
    }
}
