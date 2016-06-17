using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace client.Classes
{
    class Servidor
    {
        const int TAMANHO_BUFFER = 10000;
        int requisicoes;
        //mensagem que o cliente manda para o servidor
        string mensagemCliente;
        //mensagem que o servidor manda ao cliente
        string respostaServidor;
        //Socket do servidor
        TcpListener servidor;
        //Socket do cliente
       TcpClient cliente;
        public Servidor(int porta)
        {
            this.servidor = new TcpListener(IPAddress.Any, porta);
            this.cliente = default(TcpClient);
            this.servidor.Start();
            this.cliente = servidor.AcceptTcpClient();
            this.requisicoes = 0;
            this.respostaServidor = "";
        }
        public void Run()
        {
            this.requisicoes++;
            NetworkStream netStream = cliente.GetStream();
            byte[] recebido = new byte[TAMANHO_BUFFER];
            //recebe a mensagem do cliente
            netStream.Read(recebido, 0, (int)cliente.ReceiveBufferSize);
            //converte bytes em string
            this.mensagemCliente = Encoding.ASCII.GetString(recebido);
            /* reduz a string deixando de fora os caracteres
             * adicionados durante o processo de conversão bytes->string */
            this.mensagemCliente = this.mensagemCliente.Substring(0, this.mensagemCliente.IndexOf("$"));

            /* define a resposta do servidor
             * manda para o cliente a mensagem recebida
             * convertida em letras maiusculas */
            this.respostaServidor = "Resposta do Servidor " + Convert.ToString(requisicoes) + ": " +
            this.mensagemCliente.ToUpperInvariant();

            Byte[] enviado = Encoding.ASCII.GetBytes(this.respostaServidor);
            //envia a resposta em bytes ao cliente
            netStream.Write(enviado, 0, enviado.Length);
            netStream.Flush();
        }
    }
}
