using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using Br.Ticket.Mensagens;
using NUnit.Framework;
using System.Configuration;

namespace MensagensTest
{
    [TestFixture]
    public class FacilitadorTest
    {

        [Test]
        public void Retorna_String_Vazia_Quando_Parametros_Sao_Invalidos_Ao_Converter_ByteArr_em_String()
        {

            Assert.DoesNotThrow(() =>
                {

                    var texto = Facilitador.ConverteEmTextoNoEncodingEsperado(null, null);
                   
                },"Mesmo recebendo parâmetros inválidos, o método não deve lançar excessões");

            var vazio = Facilitador.ConverteEmTextoNoEncodingEsperado(null, null);
            Assert.IsEmpty(vazio, "A mensagem convertida deveria estar vazia");

        }

        [Test]
        public void Lanca_Excessao_Quando_Parametros_Sao_Invalidos_Para_Instanciar_Consumidor()
        {


            Assert.Throws<Exception>(() =>
                {
                    var x = Facilitador.InstanciarConsumidor("", "");
                });

           
        }

        [Test]
        [Ignore("Verificar qual o problema")]
        public void Instancia_ConfiguracaoConsumidorFila()
        {
            Assert.DoesNotThrow(()=>
                {
                    ConsumidorFilaSection config = Facilitador.ObterConfiguracao("consumidorFilaSection"); 
            });

        }

        [Test]
        public void Instancia_IConsumidor_OracleAQ()
        {
            
            //Obs: este assembly não deve ser referenciado diretamente pela app cliente
            //já que nosso objetivo é substituí-lo sem recompilar
            var assembly = "Plugins\\Br.Ticket.Mensagens.Consumidores.OracleAQ.dll";


            var caminho = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assembly);

            var classe = "Br.Ticket.Mensagens.Consumidores.OracleAQ.ConsumidorOracleAQ";

            Assert.DoesNotThrow(() =>
            {
                var cons = Facilitador.InstanciarConsumidor(caminho, classe);
                
            });

            var consumidor = Facilitador.InstanciarConsumidor(caminho, classe);
            Assert.IsNotNull(consumidor);
           


        }

        [Test]
        public void Converte_BytesASCII_Em_String_ASCII()
        {
            
            var texto = @"abcdefghijklmnopqrstuvxz!@#$%&*()_-<>,.;:{}[]=+1234567890?/\|";

            var asciiBytes = Encoding.ASCII.GetBytes(texto);

            var tipoConteudoAscii = new ContentType("text/plain;charset=ascii");

            var asciiStringConvertido = Facilitador.ConverteEmTextoNoEncodingEsperado(asciiBytes, tipoConteudoAscii);

            var asciiConvertido = Encoding.ASCII.GetString(asciiBytes);

            Assert.AreEqual(texto, asciiStringConvertido,"O texto original não é igual ao texto esperado");

            }

        [Test]
        public void Converte_BytesUTF8_Em_String_UTF8()
        {
            
            var utf = Encoding.UTF8;

            var texto = "^~´`àéõãáç";

            var bytes = Encoding.UTF8.GetBytes(texto);

            var tipoConteudo = new ContentType("text/plain;charset=utf-8");

            var stringConvertido = Facilitador.ConverteEmTextoNoEncodingEsperado(bytes, tipoConteudo);

            var stringEsperado = Encoding.UTF8.GetString(bytes);

            Assert.AreEqual(texto, stringConvertido, "O texto gerado não é igual ao texto esperado");

        }

    }
}
