using System;
using System.Collections.Generic;
using System.Linq;
using MonopolyGameLogic; 
using MonopolyBoard; // <-- MUDANÇA 1: Adicionar using

namespace Resisto_dos_jogadores
{
    public class SistemaJogo
    {
        // ... (classe Jogador fica igual, com PosicaoX e PosicaoY) ...
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
        
        // <-- MUDANÇA 2: Guardar uma referência ao tabuleiro
        private readonly Board board;

        // <-- MUDANÇA 3: Adicionar um construtor que "recebe" o tabuleiro
        public SistemaJogo(Board board)
        {
            this.board = board;
        }

        // ... (ObterJogadoresOrdenados e ExecutarComando ficam iguais) ...
        public IEnumerable<Jogador> ObterJogadoresOrdenados()
        {
            return jogadores
                .OrderByDescending(j => j.Vitorias)
                .ThenBy(j => j.Nome);
        }
        public bool ExecutarComando(string linha)
        {
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
                case "RJ":
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
        
        // *** MÉTODO 'LancarDados' ATUALIZADO ***
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

            // <-- MUDANÇA 4: Usar o 'board' para obter o nome
            // (Y é a linha, X é a coluna)
            string nomeCasa = board.GetSpaceName(jogador.PosicaoY, jogador.PosicaoX);

            // <-- MUDANÇA 5: Atualizar a mensagem de saída
            Console.WriteLine($"{jogador.Nome} (Jogos:{jogador.Jogos} V:{jogador.Vitorias} E:{jogador.Empates} D:{jogador.Derrotas}) - Posição: ({jogador.PosicaoX}, {jogador.PosicaoY}) [{nomeCasa}]");
            Console.WriteLine($"  (Dados lançados: X={resultado.HorizontalMove}, Y={resultado.VerticalMove})");
        }

        // ... (MostrarInstrucaoInvalida fica igual) ...
        private void MostrarInstrucaoInvalida()
        {
            Console.WriteLine("2025-2026");
            Console.WriteLine("Instrução inválida.");
            Console.Write("Pressione Enter para reiniciar...");
            Console.ReadLine();
        }
    }
}