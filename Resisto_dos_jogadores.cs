// Nome do Ficheiro: Resisto_dos_jogadores.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MonopolyGameLogic; 
using MonopolyBoard; 

namespace Resisto_dos_jogadores
{
    public class SistemaJogo
    {
        // ==================================================================
        // === CLASSE INTERNA JOGADOR (Fica igual) ==========================
        // ==================================================================
        public class Jogador
        {
            // --- Propriedades do Jogador ---
            public string Nome { get; set; }
            public int Jogos { get; set; }
            public int Vitorias { get; set; }
            public int Empates { get; set; }
            public int Derrotas { get; set; }
            public int PosicaoX { get; set; }
            public int PosicaoY { get; set; }
            public int Dinheiro { get; set; }
            public bool JaLancouDadosTurno { get; set; }
            
            // --- Construtor do Jogador ---
            public Jogador(string nome)
            {
                Nome = nome; Jogos = 0; Vitorias = 0; Empates = 0; Derrotas = 0;
                PosicaoX = 3; PosicaoY = 3; Dinheiro = 1500; JaLancouDadosTurno = false; 
            }

            // --- Método de Ação do Jogador ---
            public void RealizarJogada(DiceRoller diceRoller, Board board, Dictionary<string, EspacoComercial> espacos, SistemaJogo sistema)
            {
                // 1. Lançar dados e mover
                DiceResult resultado = diceRoller.Roll();
                int novaPosX = this.PosicaoX + resultado.HorizontalMove;
                int novaPosY = this.PosicaoY + resultado.VerticalMove;
                this.PosicaoX = Math.Clamp(novaPosX, 0, 6);
                this.PosicaoY = Math.Clamp(novaPosY, 0, 6);
                string nomeCasa = board.GetSpaceName(this.PosicaoY, this.PosicaoX);

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
        // ==================================================================
        // === FIM DA CLASSE JOGADOR ========================================
        // ==================================================================


        // --- Propriedades do SistemaJogo ---
        private readonly List<Jogador> jogadores = new();
        private readonly DiceRoller diceRoller = new(); 
        private readonly Board board; 
        private readonly Dictionary<string, EspacoComercial> espacosComerciais = new(); 
        private bool jogoIniciado = false;
        private int dinheiroFreePark = 0;
        private int indiceJogadorAtual = 0;
        private bool jogadorJaLancouDadosEsteTurno = false;
        
        public bool JogoIniciado => jogoIniciado;
        public int ContagemJogadores => jogadores.Count;
        public int DinheiroFreePark => dinheiroFreePark;
        public Jogador JogadorAtual => (jogoIniciado && jogadores.Any()) ? jogadores[indiceJogadorAtual] : null;
        public IReadOnlyDictionary<string, EspacoComercial> Espacos => espacosComerciais;

        
        // --- Construtor e Métodos de Gestão ---
        public SistemaJogo(Board board)
        {
            this.board = board;
            InicializarEspacos(); 
        }

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
        
        public IEnumerable<EspacoComercial> ObterPropriedadesDoJogador(Jogador jogador)
        {
            return espacosComerciais.Values
                .Where(espaco => espaco.Dono == jogador)
                .OrderBy(espaco => espaco.Cor) 
                .ThenBy(espaco => espaco.Nome);
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
            if (linhaLimpa.Equals("q", StringComparison.OrdinalIgnoreCase)) { Environment.Exit(0); return true; }
            string[] partes = linhaLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) { MostrarInstrucaoInvalida(); return false; }
            string instrucao = partes[0].ToUpper(); 

            if (!jogoIniciado && instrucao != "IJ" && instrucao != "RJ" && instrucao != "Q" && instrucao != "LS")
            {
                // (O comando 'CC' e 'PROPS' agora são permitidos antes do IJ)
                if (instrucao != "CC" && instrucao != "PROPS")
                {
                    Console.WriteLine("Erro: O jogo ainda não começou. Use 'RJ', 'IJ' ou 'LS'.");
                    Console.Write("Pressione Enter para continuar...");
                    Console.ReadLine();
                    return false;
                }
            }

            switch (instrucao)
            {
                case "IJ": 
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    IniciarJogo();
                    break;
                case "RJ": 
                    if (jogoIniciado) { /*...*/ return false; }
                    if (jogadores.Count >= 5) { /*...*/ return false; }
                    if (partes.Length != 2) { MostrarInstrucaoInvalida(); return false; }
                    RegistarJogador(partes[1]);
                    break;
                case "LD": 
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    LancarDados(); 
                    break;
                
                case "CC": // Comprar Casa
                    if (partes.Length != 1) // Já não aceita "CC NomeProp"
                    {
                        Console.WriteLine("Erro: O comando é apenas 'CC' para abrir o menu de compra.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    // Chama o novo método de sub-menu
                    MenuComprarCasas();
                    break;

                case "ET": 
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    EncerrarTurno();
                    break;
                case "PROPS": 
                    // (Removido o 'if (!jogoIniciado)')
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    VerPropriedades();
                    break;
                case "LS": 
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    ListarEstatisticas();
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
            if (jogoIniciado) { /*...*/ return; }
            if (jogadores.Count < 2) { /*...*/ return; }
            jogoIniciado = true;
            indiceJogadorAtual = 0; 
            jogadorJaLancouDadosEsteTurno = false; 
            Console.WriteLine("--- O JOGO COMEÇOU ---");
            Console.WriteLine($"A jogar com {jogadores.Count} jogadores.");
            Console.WriteLine($"É a vez de: {JogadorAtual.Nome}");
        }
        private void RegistarJogador(string nome) 
        {
            if (jogadores.Any(j => j.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase))) { /*...*/ return; }
            jogadores.Add(new Jogador(nome));
        }
        private void LancarDados()
        {
            if (jogadorJaLancouDadosEsteTurno)
            {
                Console.WriteLine($"Erro: {JogadorAtual.Nome} já lançou os dados neste turno.");
                Console.WriteLine("Pode Comprar Casas (CC), ver Propriedades (PROPS), ver Estatísticas (LS) ou Encerrar o Turno (ET).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            var jogador = JogadorAtual; 
            Console.WriteLine($"\n--- Turno de {jogador.Nome} (Lançamento) ---");
            jogador.RealizarJogada(diceRoller, board, espacosComerciais, this);
            jogadorJaLancouDadosEsteTurno = true;
        }
        private void EncerrarTurno()
        {
            if (!jogadorJaLancouDadosEsteTurno)
            {
                Console.WriteLine("Erro: Deve lançar os dados (LD) antes de encerrar o turno.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            Console.WriteLine($"\n--- Fim do turno de {JogadorAtual.Nome} ---");
            indiceJogadorAtual++;
            if (indiceJogadorAtual >= jogadores.Count) { indiceJogadorAtual = 0; }
            jogadorJaLancouDadosEsteTurno = false;
            Console.WriteLine($"Próximo a jogar: {JogadorAtual.Nome}");
            Console.Write("Pressione Enter para continuar...");
            Console.ReadLine();
        }

        // Este é o método de compra que é chamado PELO sub-menu
        private void ComprarCasa(string nomePropriedade)
        {
            // 1. A verificação 'jogadorJaLancouDadosEsteTurno' foi REMOVIDA
            
            var jogador = JogadorAtual;
            
            // 1.1 Verifica se o jogo começou (não pode comprar se não houver jogador atual)
            if (!jogoIniciado)
            {
                Console.WriteLine("Erro: O jogo ainda não começou. Registe jogadores (RJ) e inicie (IJ).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            // 2. Encontra a propriedade
            if (!espacosComerciais.TryGetValue(nomePropriedade, out var espaco))
            {
                Console.WriteLine($"Erro: Propriedade '{nomePropriedade}' não encontrada.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            // 3. Verifica se o jogador é o dono
            if (espaco.Dono != jogador)
            {
                Console.WriteLine($"Erro: Você não é o dono de [{espaco.Nome}].");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            // 4. Verifica se tem o monopólio
            if (!VerificarMonopolio(jogador, espaco.Cor))
            {
                Console.WriteLine($"Erro: Deve possuir todas as propriedades do grupo '{espaco.Cor}' para comprar casas.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            
            // 5. Verifica CONSTRUÇÃO UNIFORME
            string corDoGrupo = espaco.Cor;
            int nivelAtualAlvo = espaco.NivelCasa; 
            var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(corDoGrupo);
            foreach (string nomePropIrma in nomesPropsDoGrupo)
            {
                if (nomePropIrma == espaco.Nome) continue; 
                var espacoIrmao = espacosComerciais[nomePropIrma];
                
                if (espacoIrmao.NivelCasa < nivelAtualAlvo)
                {
                    Console.WriteLine($"Erro: Deve construir uniformemente.");
                    Console.WriteLine($"  [{espaco.Nome}] está no nível {nivelAtualAlvo}, mas [{espacoIrmao.Nome}] está no nível {espacoIrmao.NivelCasa}.");
                    Console.WriteLine($"  Deve primeiro construir em [{espacoIrmao.Nome}].");
                    Console.Write("Pressione Enter para continuar...");
                    Console.ReadLine();
                    return; 
                }
            }

            // 6. Verifica o preço
            int precoCasa = espaco.PrecoCasa;
            if (jogador.Dinheiro < precoCasa)
            {
                Console.WriteLine($"Erro: Sem dinheiro. Uma casa em [{espaco.Nome}] custa ${precoCasa}.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            // 7. Verifica o limite de casas
            if (espaco.NivelCasa >= 5)
            {
                Console.WriteLine($"Erro: [{espaco.Nome}] já atingiu o nível máximo (Hotel).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            // 8. Tudo certo! Executa a compra
            jogador.Dinheiro -= precoCasa;
            espaco.NivelCasa++;
            
            Console.WriteLine($"Casa comprada para [{espaco.Nome}]!");
            Console.WriteLine($"Novo Nível: {espaco.NivelCasa} (Custo: ${precoCasa})");
            Console.WriteLine($"Saldo restante: ${jogador.Dinheiro}");
            Console.Write("Pressione Enter para continuar...");
            Console.ReadLine();
        }

        private bool VerificarMonopolio(Jogador jogador, string cor)
        {
            if (string.IsNullOrEmpty(cor)) { return false; }
            var propriedadesDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(cor);
            foreach (string nomeProp in propriedadesDoGrupo)
            {
                if (espacosComerciais[nomeProp].Dono != jogador)
                {
                    return false; 
                }
            }
            return true; 
        }

        // Este é o novo método que controla o SUB-MENU
        private void MenuComprarCasas()
        {
            // 0. Verifica se o jogo começou
            if (!jogoIniciado)
            {
                Console.WriteLine("Erro: O jogo ainda não começou. Registe jogadores (RJ) e inicie (IJ).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            
            var jogador = JogadorAtual;

            // Este é o loop do "sub-menu"
            while (true)
            {
                Console.Clear(); // Limpa o ecrã para mostrar o menu
                
                // 1. Obter a lista de propriedades elegíveis
                var elegiveis = new List<EspacoComercial>();
                var minhasPropriedades = espacosComerciais.Values.Where(p => p.Dono == jogador);

                foreach (var prop in minhasPropriedades)
                {
                    if (string.IsNullOrEmpty(prop.Cor)) continue;
                    if (!VerificarMonopolio(jogador, prop.Cor)) continue;
                    if (prop.NivelCasa >= 5) continue;
                    if (jogador.Dinheiro < prop.PrecoCasa) continue;

                    // Regra de Construção Uniforme
                    string corDoGrupo = prop.Cor;
                    int nivelAtualAlvo = prop.NivelCasa;
                    var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(corDoGrupo);
                    
                    bool construcaoUniforme = true;
                    foreach (string nomePropIrma in nomesPropsDoGrupo)
                    {
                        if (nomePropIrma == prop.Nome) continue; 
                        var espacoIrmao = espacosComerciais[nomePropIrma];
                        if (espacoIrmao.NivelCasa < nivelAtualAlvo)
                        {
                            construcaoUniforme = false; 
                            break; 
                        }
                    }
                    if (construcaoUniforme)
                    {
                        elegiveis.Add(prop);
                    }
                }

                // 2. Mostrar o menu
                Console.WriteLine($"--- Menu de Compra de Casas ({jogador.Nome} | Saldo: ${jogador.Dinheiro}) ---");
                if (elegiveis.Count == 0)
                {
                    Console.WriteLine("\n  Nenhuma propriedade elegível.");
                    Console.WriteLine("  (Lembre-se: precisa do grupo completo, dinheiro,");
                    Console.WriteLine("  espaço para construir, e construir uniformemente.)");
                }
                else
                {
                    Console.WriteLine("\n  Propriedades elegíveis para comprar casas:");
                    foreach (var prop in elegiveis.OrderBy(p => p.NivelCasa).ThenBy(p => p.Cor))
                    {
                        Console.WriteLine($"  - [{prop.Nome}] ({prop.Cor}) | Custo: ${prop.PrecoCasa} | Nível Atual: {prop.NivelCasa}");
                    }
                }
                
                // 3. Pedir o comando (dentro do sub-menu)
                Console.WriteLine("\n  Digite o nome da propriedade para comprar (ex: Brown1)");
                Console.WriteLine("  Ou digite 'SAIR' para voltar ao jogo.");
                Console.Write("  > ");
                string input = (Console.ReadLine() ?? "").Trim();

                // 4. Processar o comando
                if (input.Equals("SAIR", StringComparison.OrdinalIgnoreCase))
                {
                    break; // Sai do loop 'while(true)' e volta ao menu principal
                }
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue; // Repete o loop se o utilizador só premir Enter
                }

                // Se não for 'SAIR' ou vazio, tenta comprar
                // Chama o método ComprarCasa INTERNAMENTE
                ComprarCasa(input);
                
                // Após a tentativa de compra (ComprarCasa() já fez a pausa "Pressione Enter"),
                // o loop 'while(true)' repete-se, limpando o ecrã e mostrando a lista atualizada.
            }
        }
        
        private void VerPropriedades()
        {
            if (!jogoIniciado) 
            {
                if (jogadores.Count == 0)
                {
                    Console.WriteLine("Erro: Sem jogadores registados.");
                    Console.Write("Pressione Enter para continuar...");
                    Console.ReadLine();
                    return;
                }
            }
            
            var jogador = JogadorAtual ?? jogadores.FirstOrDefault();
            if (jogador == null) { return; } 

            Console.WriteLine($"\n--- Propriedades de {jogador.Nome} (${jogador.Dinheiro}) ---");
            var props = ObterPropriedadesDoJogador(jogador);
            if (!props.Any())
            {
                Console.WriteLine("  (Nenhuma propriedade.)");
            }
            else
            {
                var grupos = props.GroupBy(p => p.Cor ?? "Outros").OrderBy(g => g.Key); 
                foreach (var grupo in grupos)
                {
                    Console.WriteLine($"\n  Grupo: {grupo.Key}");
                    foreach (var p in grupo)
                    {
                        string nivel = (p.NivelCasa > 0) ? $"(Nível {p.NivelCasa})" : "(sem casas)";
                        string precoCasaStr = (p.Cor != null) ? $" (Casa: ${p.PrecoCasa})" : ""; 
                        Console.WriteLine($"    - [{p.Nome}] {nivel}{precoCasaStr}");
                    }
                }
            }
            Console.Write("\n  Pressione Enter para continuar...");
            Console.ReadLine();
        }

        private void ListarEstatisticas()
        {
            Console.WriteLine($"\n--- Estatísticas dos Jogadores ---");
            var jogadoresOrdenados = ObterJogadoresOrdenados();
            if (!jogadoresOrdenados.Any())
            {
                Console.WriteLine("  (Sem jogadores registados)");
            }
            else
            {
                Console.WriteLine($"  {"Nome", -15} | {"Jogos", -7} | {"Vitórias", -9} | {"Derrotas", -8}");
                Console.WriteLine("  ---------------------------------------------------");
                foreach (var j in jogadoresOrdenados)
                {
                    Console.WriteLine($"  {j.Nome, -15} | {j.Jogos, -7} | {j.Vitorias, -9} | {j.Derrotas, -8}");
                }
            }
            Console.Write("\n  Pressione Enter para continuar...");
            Console.ReadLine();
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