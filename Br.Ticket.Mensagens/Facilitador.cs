namespace Br.Ticket.Mensagens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net.Mime;
    using Br.Ticket.Mensagens.Contratos;
    using System.Reflection;
    using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Facilitador
    {
        /// <summary>
        /// Transforma o conteúdo em uma string, no mesmo charset informado.
        /// <para>
        /// Em caso de problemas com os parâmetros, uma string vazia é retornada.
        /// </para>
        /// </summary>
        /// <param name="conteudo">um conjunto de bytes que representa uma mensagem recebida de outro sistema</param>
        /// <param name="encodingAtual">o Enconding no qual o conteúdo foi originalmente gerado</param>
        /// <remarks>em caso de erro </remarks>
        /// <returns>mensagem decodificada no charset informado</returns>
        public static string ConverteEmTextoNoEncodingEsperado(byte[] conteudo, ContentType encodingAtual)
        {
            string mensagem = string.Empty;
            string erro;

            try
            {
                var encodedOriginal = Encoding.GetEncoding(encodingAtual.CharSet);

                mensagem = encodedOriginal.GetString(conteudo);

            }
            catch (NullReferenceException e)
            {
                erro = String.Concat("Erro: Referência nula. Detalhes: ", e.Message);
                // TODO: Log
            }
            catch (ArgumentNullException e)
            {
                erro = String.Concat("Erro: Argumento nulo. Detalhes: ", e.Message);
                // TODO: Log
            }
            catch (ArgumentException e)
            {
                erro = String.Concat("Erro: Argumento inválido. Detalhes: ", e.Message);
                // TODO: Log
            }
            finally
            {
                if (String.IsNullOrEmpty(mensagem))
                    mensagem = String.Empty;
            }

            return mensagem;
        }


        /// <summary>
        /// Recupera uma instância da classe informada, que implementa IConsumidor.
        /// </summary>
        /// <param name="caminhoCompletoAssembly">Caminho absoluto, incluindo nome do Assembly</param>
        /// <param name="nomeCompletoClasse">Nome completo da Classe, incluindo namespace.</param>
        /// <returns>Instância da classe solicitada</returns>
        public static IConsumidor InstanciarConsumidor(string caminhoCompletoAssembly, string nomeCompletoClasse)
        {
            IConsumidor consumidor = null;
            try
            {

                Assembly assembly = Assembly.LoadFrom(caminhoCompletoAssembly);

                Type tipo = assembly.GetType(nomeCompletoClasse);

                consumidor = Activator.CreateInstance(tipo) as IConsumidor;

            }
            catch (Exception e)
            {
                var mensagem = String.Format("Erro Fatal. Confirme os parâmetros informados e a InnerException. {0} CaminhoCompletoAssembly: {1}{0} nomeCompletoClasse: {2}.{0} InnerException: {3}",
                    Environment.NewLine, caminhoCompletoAssembly, nomeCompletoClasse, e.ToString());

                //Console.WriteLine(mensagem);
                throw new Exception(mensagem, e);

                // TODO: Log
            }

            return consumidor;
        }

        /// <summary>
        /// Método de conveniência para obter a instância da seção de configuração do App/Web.config.
        /// </summary>
        /// <returns></returns>
        public static ConsumidorFilaSection ObterConfiguracao(string nomeSecao)
        {
            var config = new ConsumidorFilaSection();

            try
            {


                var secao = ConfigurationManager.GetSection(nomeSecao);
                
                if (secao == null)
                    throw new ConfigurationErrorsException(String.Format("Sessão '{0}' não foi encontrada no arquivo de configuração",nomeSecao));
            
                config = secao as ConsumidorFilaSection;

            }
            catch (ConfigurationErrorsException e)
            {
                // TODO: log
                throw e;

            }

            return config;
        }
    }
}
