
using System;
using System.Collections.Generic;
using System.Linq;
using MonopolyGameLogic; 
using MonopolyBoard; 

namespace Resisto_dos_jogadores
{
    public class SistemaJogo
    {
 
        public class Jogador
        {
            

            public string Nome { get; set; }
            public int Jogos { get; set; }
            public int Vitorias { get; set; }
            public int Empates { get; set; }
            public int Derrotas { get; set; }
            public int PosicaoX { get; set; }
            public int PosicaoY { get; set; }
            public int Dinheiro { get; set; }
            public bool JaLancouDadosTurno { get; set; }
            
            public Jogador(string nome)
            {
                Nome = nome;
                Jogos = 0;
                Vitorias = 0;
                Empates = 0;
                Derrotas = 0;
                PosicaoX = 3; 
                PosicaoY = 3; // Posição "Start" [3, 3]
                Dinheiro = 1500; // Dinheiro inicial
                JaLancouDadosTurno = false; 
            }

            public void RealizarJogada(DiceRoller diceRoller, Board board, Dictionary<string, EspacoComercial> espacos, SistemaJogo sistema)
            {
                // 1. Lançar dados e mover
                DiceResult resultado = diceRoller.Roll();
                int novaPosX = this.PosicaoX + resultado.HorizontalMove;
                int novaPosY = this.PosicaoY + resultado.VerticalMove;
                
                this.PosicaoX = Math.Clamp(novaPosX, 0, 6);
                this.PosicaoY = Math.Clamp(novaPosY, 0, 6);
                
                string nomeCasa = board.GetSpaceName(this.PosicaoY, this.PosicaoX);
                this.JaLancouDadosTurno = true; 

                // 2. Mostrar resultado do movimento
                Console.WriteLine($"{this.Nome} (${this.Dinheiro}) - Posição: ({this.PosicaoX}, {this.PosicaoY}) [{nomeCasa}]");
                Console.WriteLine($"  (Dados lançados: X={resultado.HorizontalMove}, Y={resultado.VerticalMove})");

                // 3. Verificar a ação do espaço
                if (EspacoComercial.EspacoEComercial(nomeCasa))
                {
                    var espaco = espacos[nomeCasa];
                    espaco.AterrarNoEspaco(this, sistema); 
                }
                else if (nomeCasa == "Lux Tax")
                {
                    int imposto = 80;
                    Console.WriteLine($"  Você aterrou em [Lux Tax]! Pague ${imposto}.");
                    this.Dinheiro -= imposto;
                    sistema.AdicionarDinheiroFreePark(imposto); 
                    Console.WriteLine($"  $80 movidos para o FreePark. O seu novo saldo é ${this.Dinheiro}.");
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                }
                else if (nomeCasa == "FreePark")
                {
                    int premio = sistema.DinheiroFreePark;
                    if (premio > 0)
                    {
                        Console.WriteLine($"  Você aterrou em [FreePark] e recolheu ${premio}!");
                        this.Dinheiro += premio;
                        sistema.ZerarDinheiroFreePark();
                        Console.WriteLine($"  O seu novo saldo é ${this.Dinheiro}.");
                    }
                    else
                    {
                        Console.WriteLine("  Você aterrou em [FreePark]. (Não há dinheiro acumulado.)");
                    }
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                }
            }
        }



        // --- Propriedades do SistemaJogo ---
        private readonly List<Jogador> jogadores = new();
        private readonly DiceRoller diceRoller = new(); 
        private readonly Board board; 
        private readonly Dictionary<string, EspacoComercial> espacosComerciais = new(); 
        private bool jogoIniciado = false;
        private int dinheiroFreePark = 0;
        
        // <-- MUDANÇA 1: Adicionei um índice para controlar o turno -->
        private int indiceJogadorAtual = 0;
        
        // Propriedades públicas
        public bool JogoIniciado => jogoIniciado;
        public int ContagemJogadores => jogadores.Count;
        public int DinheiroFreePark => dinheiroFreePark;

        // <-- MUDANÇA 2: Propriedade pública para saber quem é o jogador atual -->
        public Jogador JogadorAtual => (jogoIniciado && jogadores.Any()) ? jogadores[indiceJogadorAtual] : null;

        
        // --- Construtor do SistemaJogo ---
        public SistemaJogo(Board board)
        {
            this.board = board;
            InicializarEspacos(); 
        }

        // --- Métodos de gestão do jogo ---
        private void InicializarEspacos()
        {
            var precosBase = EspacoComercial.ObterPrecosBase();
            foreach (var par in precosBase)
            {
                string nome = par.Key;
                int preco = par.Value;
                espacosComerciais.Add(nome, new EspacoComercial(nome, preco));
            }
        }
        public void AdicionarDinheiroFreePark(int valor) { dinheiroFreePark += valor; }
        public void ZerarDinheiroFreePark() { dinheiroFreePark = 0; }
        public List<Jogador> ObterOutrosJogadores(Jogador jogadorAtual)
        {
            return jogadores.Where(j => j != jogadorAtual).ToList();
        }
        public IEnumerable<Jogador> ObterJogadoresOrdenados()
        {
            return jogadores
                .OrderByDescending(j => j.Vitorias)
                .ThenBy(j => j.Nome);
        }

        // --- Método Principal de Comandos ---
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
                    if (jogoIniciado)
                    {
                        Console.WriteLine("Erro: Não pode registar jogadores depois do jogo começar.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
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
                
                // <-- MUDANÇA 3: Comando 'LD' simplificado -->
                case "LD": // Lançar Dados
                    if (!jogoIniciado)
                    {
                        Console.WriteLine("Erro: O jogo ainda não começou. Use o comando 'IJ'.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    
                    // Agora só aceita "LD", sem nome de jogador
                    if (partes.Length != 1) 
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        // Chama o novo método LancarDados (sem parâmetros)
                        LancarDados(); 
                    }
                    break;

                default:
                    MostrarInstrucaoInvalida();
                    return false;
            }
            
            return true;
        }

        // --- Métodos de Lógica do Jogo ---

        private void IniciarJogo()
        {
            if (jogoIniciado)
            {
                Console.WriteLine("O jogo já foi iniciado.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            if (jogadores.Count < 2)
            {
                Console.WriteLine($"Erro: São necessários pelo menos 2 jogadores para iniciar. (Atuais: {jogadores.Count})");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            jogoIniciado = true;
            // <-- MUDANÇA 4: Define o turno inicial -->
            indiceJogadorAtual = 0; // Começa com o primeiro jogador registado
            
            Console.WriteLine("--- O JOGO COMEÇOU ---");
            Console.WriteLine($"A jogar com {jogadores.Count} jogadores.");
            Console.WriteLine($"É a vez de: {JogadorAtual.Nome}"); // Mostra quem começa
            Console.WriteLine("Registo de jogadores bloqueado.");
            Console.WriteLine("Lançamento de dados (LD) disponível.");
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
        
        // <-- MUDANÇA 5: Método "LancarDados" agora controla o turno -->
        private void LancarDados()
        {
            // 1. Obtém o jogador cujo turno é agora
            var jogador = JogadorAtual; 
            
            if (jogador == null) // Apenas uma salvaguarda
            {
                Console.WriteLine("Erro: Não foi possível encontrar o jogador atual.");
                return; 
            }
            
            Console.WriteLine($"\n--- Turno de {jogador.Nome} ---");

            // 2. Executa a jogada (como antes)
            jogador.RealizarJogada(diceRoller, board, espacosComerciais, this);

            // 3. Avança o índice para o próximo jogador
            indiceJogadorAtual++;
            
            // 4. Se o índice passar do último jogador, volta ao primeiro (índice 0)
            if (indiceJogadorAtual >= jogadores.Count)
            {
                indiceJogadorAtual = 0;
            }
            
            // 5. Anuncia (no ecrã principal) de quem é o próximo turno
            // (O Program.cs fará isto ao redesenhar)
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