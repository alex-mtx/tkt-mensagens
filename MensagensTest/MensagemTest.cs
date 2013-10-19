using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Br.Ticket.Mensagens;

using NUnit.Framework;
using System.Net.Mime;

namespace MensagensTest
{
    [TestFixture]
    public class MensagemTest
    {

        [Test]
        public void Construtor_Nao_Aceita_Parametros_Nulos()
        {
            byte[] conteudo = new byte[1] { 255 };
            ContentType tipo = new ContentType();

            Assert.Throws<ArgumentNullException>(() =>
                {
                    var x = new Mensagem(null, tipo);

                });

            Assert.Throws<ArgumentNullException>(() =>
            {
                var x = new Mensagem(conteudo,null);

            });
        }

        [Test]
        public void To_String()
        {
            string esperado = "[Correlação: 123] [Criador: dev] [Expiração: 1] "
                             +"[Origem: origem] [Prioridade: 9] [Tipo Conteúdo: text/plain; charset=utf-8] "
                             +"[Tipo Entrega: EmMemoria] " + Environment.NewLine + "[Conteúdo:" + Environment.NewLine
                             +"abcdefghijklmnopqrstuvxz!@#$%&*()_-<>,.;:{}[]=+1234567890?/\\|" + Environment.NewLine + " ]";


            var texto = @"abcdefghijklmnopqrstuvxz!@#$%&*()_-<>,.;:{}[]=+1234567890?/\|";

            var bytes = Encoding.UTF8.GetBytes(texto);

            var tipoConteudo = new ContentType("text/plain;charset=utf-8");

            var msg = new Mensagem(bytes,tipoConteudo);
            msg.Correlacao = "123";
            msg.Criador = "dev";
            msg.Expiracao = 1;
            msg.Origem = "origem";
            msg.Prioridade = 9;
            msg.TipoEntrega = TipoEntrega.EmMemoria;
            Console.WriteLine(msg.ToString());

            Assert.AreEqual(esperado,msg.ToString(),"Os textos são diferentes");



        }
    }
}
