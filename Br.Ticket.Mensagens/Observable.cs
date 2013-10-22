using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Br.Ticket.Mensagens.Contratos;
using System.Threading;

namespace Br.Ticket.Mensagens
{
   

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Observable
    {
        protected EventHandler _evento;
        
        public event EventHandler MensagemRecebidaEvento
        {
            add { _evento += value; }
            remove { _evento -= value; }
        }

        protected virtual void Notificar(MensagemRecebida msg)
        {
            EventHandler eventHandler = null;

            lock (this)
            {
                eventHandler = _evento;
            }

            if (eventHandler != null)
            {
                foreach (EventHandler handler in eventHandler.GetInvocationList())
                {
                    try
                    {
                        handler(this, new MensagemEventArgs() { Mensagem = msg });
                        Console.WriteLine("Notificar: Thread {0}", Thread.CurrentThread.ManagedThreadId);
                    }
                    //Não pode subir nenhuma excessão, caso contrário os próximos handlers não recebem
                    // a notificação
                    catch (Exception e)
                    {
                        // TODO: Log
                        Console.WriteLine("Erro no handler {0}: {1}",
                        handler.Method.Name, e.Message);
                    }
                }
            }
        }

      

    }
}
