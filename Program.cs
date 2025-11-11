using System;
using System.Linq; 
using MonopolyBoard;
using Resisto_dos_jogadores;

class Program
{
    static Board board = new Board();
    static SistemaJogo sistema = new SistemaJogo(board); 

    static void Main()
    {
        RedesenharUI();
        
        while (true)
        {
            Console.Write("> ");
            string linha = Console.ReadLine();
            
            string[] partes = linha.Trim().Split(' ');
            string comando = (partes.Length > 0) ? partes[0].ToUpper() : "";

            bool sucesso = sistema.ExecutarComando(linha);

            if (!sucesso)
            {
                RedesenharUI();
            }
            else if (comando == "RJ" || comando == "IJ") 
            {
                RedesenharUI(); 
            }
        }
    }

    // *** MÉTODO 'RedesenharUI' ATUALIZADO ***
    static void RedesenharUI()
    {
        Console.Clear(); 
        board.Display(); 

        Console.WriteLine("\n--- Jogadores Registados ---");
        
        // <-- MUDANÇA: Adicionar a contagem de jogadores
        // (Só mostra esta contagem antes de o jogo começar)
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine($"({sistema.ContagemJogadores}/5 jogadores registados)");
        }
        
        var jogadores = sistema.ObterJogadoresOrdenados();
        
        if (!jogadores.Any())
        {
            Console.WriteLine("(Sem jogadores registados)");
        }
        else
        {
            foreach (var j in jogadores)
            {
                string nomeCasa = board.GetSpaceName(j.PosicaoY, j.PosicaoX);
                Console.WriteLine($"- {j.Nome} (Jogos:{j.Jogos} V:{j.Vitorias} E:{j.Empates} D:{j.Derrotas}) - Posição: ({j.PosicaoX}, {j.PosicaoY}) [{nomeCasa}]");
            }
        }

        Console.WriteLine("\n=== Sistema de Registo de Jogadores ===");
        Console.WriteLine("Comandos disponíveis:");
        
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine("  RJ NomeJogador  → Regista novo jogador (Máx 5)");
            Console.WriteLine("  IJ              → Inicia o Jogo (Mín 2)");
        }
        else
        {
            Console.WriteLine("  LD NomeJogador  → Lança os dados para um jogador");
        }
        
        Console.WriteLine("  Q               → Termina o programa");
        Console.WriteLine("----------------------------------------");
    }
}