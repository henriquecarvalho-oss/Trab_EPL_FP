using System;
using System.Collections.Generic;
using System.Linq;
using MonopolyGameLogic; 
using MonopolyBoard; 

namespace Resisto_dos_jogadores
{
    public class SistemaJogo
    {
        // ... (classe Jogador fica igual) ...
        public class Jogador
        {
            public string Nome { get; set; }
            public int Jogos { get; set; }
            public int Vitorias { get; set; }
            public int Empates { get; set; }
            public int Derrotas { get; set; }
            public int PosicaoX { get; set; }
            public int PosicaoY { get; set; }
            
            public Jogador(string nome)
            {
                Nome = nome;
                Jogos = 0;
                Vitorias = 0;
                Empates = 0;
                Derrotas = 0;
                PosicaoX = 3; 
                PosicaoY = 3; // Posição "Start" [3, 3]
            }
        }

        private readonly List<Jogador> jogadores = new();
        private readonly DiceRoller diceRoller = new();
        private readonly Board board;
        
        private bool jogoIniciado = false;

        public bool JogoIniciado
        {
            get { return jogoIniciado; }
        }
        
        // <-- MUDANÇA 1: Propriedade para o Program.cs saber a contagem
        public int ContagemJogadores
        {
            get { return jogadores.Count; }
        }

        public SistemaJogo(Board board)
        {
            this.board = board;
        }

        // ... (ObterJogadoresOrdenados fica igual) ...
        public IEnumerable<Jogador> ObterJogadoresOrdenados()
        {
            return jogadores
                .OrderByDescending(j => j.Vitorias)
                .ThenBy(j => j.Nome);
        }

        // *** MÉTODO 'ExecutarComando' ATUALIZADO ***
        public bool ExecutarComando(string linha)
        {
            // ... (Q, split, etc. fica igual) ...
            string linhaLimpa = linha.Trim();

            if (linhaLimpa.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
                return true; 
            }

            string[] partes = linhaLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 0)
            {
                MostrarInstrucaoInvalida();
                return false; 
            }

            string instrucao = partes[0].ToUpper(); 

            switch (instrucao)
            {
                case "IJ":
                    if (partes.Length != 1)
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        IniciarJogo();
                    }
                    break;

                case "RJ":
                    // Proteção 1: Jogo já começou?
                    if (jogoIniciado)
                    {
                        Console.WriteLine("Erro: Não pode registar jogadores depois do jogo começar.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    
                    // <-- MUDANÇA 2: Proteção 2 (Limite Máximo)
                    if (jogadores.Count >= 5)
                    {
                        Console.WriteLine("Erro: O limite máximo de 5 jogadores foi atingido.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false; 
                    }
                    
                    if (partes.Length != 2)
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        RegistarJogador(partes[1]);
                    }
                    break;

                case "LD":
                    if (!jogoIniciado)
                    {
                        Console.WriteLine("Erro: O jogo ainda não começou. Use o comando 'IJ'.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    
                    if (partes.Length != 2)
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        LancarDados(partes[1]);
                    }
                    break;

                default:
                    MostrarInstrucaoInvalida();
                    return false;
            }
            
            return true;
        }

        // *** MÉTODO 'IniciarJogo' ATUALIZADO ***
        private void IniciarJogo()
        {
            if (jogoIniciado)
            {
                Console.WriteLine("O jogo já foi iniciado.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            // <-- MUDANÇA 3: Alterar a verificação de 0 para 2 (Limite Mínimo)
            if (jogadores.Count < 2)
            {
                Console.WriteLine($"Erro: São necessários pelo menos 2 jogadores para iniciar. (Atuais: {jogadores.Count})");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            jogoIniciado = true;
            Console.WriteLine("--- O JOGO COMEÇOU ---");
            Console.WriteLine($"A jogar com {jogadores.Count} jogadores.");
            Console.WriteLine("Registo de jogadores bloqueado.");
            Console.WriteLine("Lançamento de dados (LD) disponível.");
        }


        // ... (RegistarJogador, LancarDados, MostrarInstrucaoInvalida ficam iguais) ...
        private void RegistarJogador(string nome)
        {
            if (jogadores.Any(j => j.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Jogador existente.");
                Console.WriteLine("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            jogadores.Add(new Jogador(nome));
        }
        
        private void LancarDados(string nomeJogador)
        {
            var jogador = jogadores.FirstOrDefault(j => j.Nome.Equals(nomeJogador, StringComparison.OrdinalIgnoreCase));

            if (jogador == null)
            {
                Console.WriteLine($"Jogador '{nomeJogador}' não encontrado.");
                return; 
            }

            DiceResult resultado = diceRoller.Roll();
            
            int novaPosX = jogador.PosicaoX + resultado.HorizontalMove;
            int novaPosY = jogador.PosicaoY + resultado.VerticalMove;
            
            jogador.PosicaoX = Math.Clamp(novaPosX, 0, 6);
            jogador.PosicaoY = Math.Clamp(novaPosY, 0, 6);
            
            string nomeCasa = board.GetSpaceName(jogador.PosicaoY, jogador.PosicaoX);

            Console.WriteLine($"{jogador.Nome} (Jogos:{jogador.Jogos} V:{jogador.Vitorias} E:{jogador.Empates} D:{jogador.Derrotas}) - Posição: ({jogador.PosicaoX}, {jogador.PosicaoY}) [{nomeCasa}]");
            Console.WriteLine($"  (Dados lançados: X={resultado.HorizontalMove}, Y={resultado.VerticalMove})");
        }
        
        private void MostrarInstrucaoInvalida()
        {
            Console.WriteLine("2025-2026");
            Console.WriteLine("Instrução inválida.");
            Console.Write("Pressione Enter para reiniciar...");
            Console.ReadLine();
        }
    }
}