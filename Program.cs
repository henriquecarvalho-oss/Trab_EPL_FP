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
            // O RJ ou IJ redesenham o ecrã
            else if (comando == "RJ" || comando == "IJ") 
            {
                RedesenharUI(); 
            }
            
            // O LD (Lançar Dados) também redesenha o ecrã
            // para atualizar posições, dinheiro, e o indicador de turno.
            if (comando == "LD" && sucesso)
            {
                RedesenharUI();
            }
        }
    }

    static void RedesenharUI()
    {
        Console.Clear(); 
        board.Display(); 

        Console.WriteLine("\n--- Jogadores Registados ---");
        
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine($"({sistema.ContagemJogadores}/5 jogadores registados)");
        }
        
        var jogadores = sistema.ObterJogadoresOrdenados();
        // <-- MUDANÇA 1: Obter o jogador atual do sistema -->
        var jogadorAtual = sistema.JogadorAtual; 
        
        if (!jogadores.Any())
        {
            Console.WriteLine("(Sem jogadores registados)");
        }
        else
        {
            foreach (var j in jogadores)
            {
                // <-- MUDANÇA 2: Adicionar um prefixo "→" para o jogador atual -->
                string prefixo = (j == jogadorAtual) ? "→ " : "- ";
                
                string nomeCasa = board.GetSpaceName(j.PosicaoY, j.PosicaoX);
                
                // Mostra o prefixo na linha
                Console.WriteLine($"{prefixo}{j.Nome} (${j.Dinheiro}) (Jogos:{j.Jogos} V:{j.Vitorias} E:{j.Empates} D:{j.Derrotas}) - Posição: ({j.PosicaoX}, {j.PosicaoY}) [{nomeCasa}]");
            }
        }

        // <-- MUDANÇA 3: Mostrar o pote E de quem é o turno -->
        if (sistema.JogoIniciado)
        {
            Console.WriteLine($"\n💰 Pote do FreePark: ${sistema.DinheiroFreePark}");
            if (jogadorAtual != null)
            {
                // Mostra de quem é o próximo turno
                Console.WriteLine($"➡️ É a vez de: {jogadorAtual.Nome}"); 
            }
        }
        // <-- Fim da Mudança -->

        Console.WriteLine("\n=== Sistema de Registo de Jogadores ===");
        Console.WriteLine("Comandos disponíveis:");
        
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine("  RJ NomeJogador  → Regista novo jogador (Máx 5)");
            Console.WriteLine("  IJ              → Inicia o Jogo (Mín 2)");
        }
        else
        {
            // <-- MUDANÇA 4: Simplificar o comando LD no menu -->
            Console.WriteLine("  LD              → Lança os dados (jogador atual)");
        }
        
        Console.WriteLine("  Q               → Termina o programa");
        Console.WriteLine("----------------------------------------");
    }
}