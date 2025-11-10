using System;
using System.Linq; 
using MonopolyBoard;
using Resisto_dos_jogadores;

class Program
{
    static Board board = new Board();
    
    // <-- MUDANÇA 1: Passar o 'board' para o construtor do 'sistema'
    static SistemaJogo sistema = new SistemaJogo(board); 

    static void Main()
    {
        RedesenharUI();
        // ... (o resto do Main() fica igual) ...
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
            else if (comando == "RJ") 
            {
                RedesenharUI(); 
            }
        }
    }

    // MÉTODO 'RedesenharUI' ATUALIZADO
    static void RedesenharUI()
    {
        Console.Clear(); 
        board.Display(); 

        Console.WriteLine("\n--- Jogadores Registados ---");
        var jogadores = sistema.ObterJogadoresOrdenados();
        
        if (!jogadores.Any())
        {
            Console.WriteLine("(Sem jogadores registados)");
        }
        else
        {
            foreach (var j in jogadores)
            {
                // <-- MUDANÇA 2: Obter o nome da casa aqui também
                string nomeCasa = board.GetSpaceName(j.PosicaoY, j.PosicaoX);
                
                // <-- MUDANÇA 3: Atualizar a lista permanente
                Console.WriteLine($"- {j.Nome} (Jogos:{j.Jogos} V:{j.Vitorias} E:{j.Empates} D:{j.Derrotas}) - Posição: ({j.PosicaoX}, {j.PosicaoY}) [{nomeCasa}]");
            }
        }

        // ... (o resto do menu fica igual) ...
        Console.WriteLine("\n=== Sistema de Registo de Jogadores ===");
        Console.WriteLine("Comandos disponíveis:");
        Console.WriteLine("  RJ NomeJogador  → Regista novo jogador");
        Console.WriteLine("  LD NomeJogador  → Lança os dados para um jogador");
        Console.WriteLine("  Q               → Termina o programa");
        Console.WriteLine("----------------------------------------");
    }
}