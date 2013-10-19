
namespace Br.Ticket.Mensagens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Define os tipos de entrega de mensagens.
    /// </summary>
    public enum TipoEntrega
    {
        Indefinido =0,
        Persistente = 1,
        EmMemoria = 2
    }
}
