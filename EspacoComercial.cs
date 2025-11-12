// Nome do Ficheiro: EspacoComercial.cs
using System.Collections.Generic;
using Resisto_dos_jogadores; 
using System;
using System.Linq; // <-- Adicionar 'using System.Linq'

namespace MonopolyGameLogic
{
    public enum ResultadoCompra
    {
        Sucesso,
        JaTemDono,
        SemDinheiroSuficiente
    }

    public class EspacoComercial
    {
        // ... (O Dicionário Estático PrecosBase fica igual) ...
        private static readonly Dictionary<string, int> PrecosBase = new()
        {
            // Browns
            { "Brown1", 100 }, { "Brown2", 120 },
            // Teals
            { "Teal1", 90 }, { "Teal2", 130 },
            // Oranges
            { "Orange1", 120 }, { "Orange2", 120 }, { "Orange3", 140 },
            // Blacks
            { "Black1", 110 }, { "Black2", 120 }, { "Black3", 130 },
            // Reds
            { "Red1", 130 }, { "Red2", 130 }, { "Red3", 160 },
            // Greens
            { "Green1", 120 }, { "Green2", 140 }, { "Green3", 160 },
            // Blues
            { "Blue1", 140 }, { "Blue2", 140 }, { "Blue3", 170 },
            // Pinks
            { "Pink1", 160 }, { "Pink2", 180 },
            // Whites
            { "White1", 160 }, { "White2", 180 }, { "White3", 190 },
            // Yellows
            { "Yellow1", 140 }, { "Yellow2", 140 }, { "Yellow3", 170 },
            // Violets
            { "Violet1", 150 }, { "Violet2", 130 },
            // Trains (Estações)
            { "Train1", 150 }, { "Train2", 150 }, { "Train3", 150 }, { "Train4", 150 },
            // Utilities (Companhias)
            { "Electric Company", 120 }, 
            { "Water Works", 120 }
        };

        // ... (Propriedades, Construtor, Métodos Estáticos ficam iguais) ...
        public string Nome { get; }
        public int Preco { get; } 
        public SistemaJogo.Jogador Dono { get; set; }

        public EspacoComercial(string nome, int preco)
        {
            Nome = nome;
            Preco = preco; 
            Dono = null; 
        }
        
        public static bool EspacoEComercial(string nome)
        {
            return PrecosBase.ContainsKey(nome);
        }

        public static IReadOnlyDictionary<string, int> ObterPrecosBase()
        {
            return PrecosBase;
        }

        public ResultadoCompra TentarComprar(SistemaJogo.Jogador comprador)
        {
            // ... (Nenhuma mudança nesta função) ...
            if (this.Dono != null)
            {
                return ResultadoCompra.JaTemDono;
            }
            if (comprador.Dinheiro < this.Preco)
            {
                return ResultadoCompra.SemDinheiroSuficiente;
            }
            
            comprador.Dinheiro -= this.Preco;
            this.Dono = comprador;
            return ResultadoCompra.Sucesso;
        }
        
        // <-- MUDANÇA 1: A assinatura do método mudou -->
        public void AterrarNoEspaco(SistemaJogo.Jogador jogador, SistemaJogo sistema)
        {
            // 1. Verificar se tem dono
            if (this.Dono == null)
            {
                // 2. Verificar se tem dinheiro
                if (jogador.Dinheiro >= this.Preco)
                {
                    // 3. Perguntar ao jogador
                    Console.WriteLine($"  Gostaria de comprar [{this.Nome}] por ${this.Preco}? (S/N)");
                    Console.Write("  > ");
                    string resposta = Console.ReadLine().Trim().ToUpper();

                    if (resposta == "S")
                    {
                        // 4. Tentar comprar
                        ResultadoCompra resultadoCompra = TentarComprar(jogador);
                        if (resultadoCompra == ResultadoCompra.Sucesso)
                        {
                            Console.WriteLine($"  Espaço comprado! O seu novo saldo é ${jogador.Dinheiro}.");
                        }
                        // (Pressionar Enter é tratado pela lógica do leilão/oferta)
                    }
                    else // <-- MUDANÇA 2: Lógica de recusa -->
                    {
                        Console.WriteLine($"  {jogador.Nome} decidiu não comprar o espaço.");
                        // Iniciar lógica de recusa (leilão ou oferta)
                        ExecutarLogicaDeRecusa(jogador, sistema);
                    }
                }
                else // <-- MUDANÇA 2: Lógica de não ter dinheiro -->
                {
                    Console.WriteLine($"  {jogador.Nome} aterrou em [{this.Nome}] (${this.Preco}), mas não tem dinheiro suficiente.");
                    // Iniciar lógica de recusa (leilão ou oferta)
                    ExecutarLogicaDeRecusa(jogador, sistema);
                }
            }
            else // Espaço já tem dono
            {
                if (this.Dono == jogador)
                {
                    Console.WriteLine($"  Você aterrou em [{this.Nome}], que é a sua propriedade.");
                }
                else
                {
                    Console.WriteLine($"  Você aterrou em [{this.Nome}], que pertence a {this.Dono.Nome}.");
                    // (Aqui entraria a lógica de pagar renda no futuro)
                }
            }
            
            // Pausa no final da jogada (movida para fora do IF)
            Console.Write("  Pressione Enter para continuar...");
            Console.ReadLine();
        }


        // <-- MUDANÇA 3: NOVO MÉTODO para gerir a recusa -->
        private void ExecutarLogicaDeRecusa(SistemaJogo.Jogador jogadorQueRecusou, SistemaJogo sistema)
        {
            var outrosJogadores = sistema.ObterOutrosJogadores(jogadorQueRecusou);
            int totalJogadoresNoJogo = sistema.ContagemJogadores;

            // Cenário 1: Jogo a 2 jogadores
            if (totalJogadoresNoJogo == 2)
            {
                var outroJogador = outrosJogadores[0]; // Só há um
                
                // Verifica se esse outro jogador tem dinheiro PARA O PREÇO DE LISTA
                if (outroJogador.Dinheiro >= this.Preco)
                {
                    OferecerPropriedade(outroJogador); // Oferece pelo preço de lista
                }
                else
                {
                    Console.WriteLine($"  {outroJogador.Nome} não tem dinheiro (${this.Preco}) para comprar. A propriedade continua sem dono.");
                }
            }
            // Cenário 2: Jogo com > 2 jogadores (Leilão)
            else if (totalJogadoresNoJogo > 2)
            {
                // Filtra apenas quem tem *algum* dinheiro (mínimo $1) para licitar
                var jogadoresElegiveis = outrosJogadores.Where(j => j.Dinheiro > 0).ToList();
                
                if (jogadoresElegiveis.Count > 0)
                {
                    IniciarLeilao(jogadoresElegiveis);
                }
                else
                {
                    Console.WriteLine("  Nenhum outro jogador tem dinheiro para o leilão. A propriedade continua sem dono.");
                }
            }
        }

        // <-- MUDANÇA 4: NOVO MÉTODO para o cenário de 2 jogadores -->
        private void OferecerPropriedade(SistemaJogo.Jogador compradorPotencial)
        {
            Console.WriteLine($"\n  A oferta passa para {compradorPotencial.Nome}.");
            Console.WriteLine($"  {compradorPotencial.Nome}, gostaria de comprar [{this.Nome}] por ${this.Preco}? (S/N)");
            Console.Write("  > ");
            string resposta = Console.ReadLine().Trim().ToUpper();

            if (resposta == "S")
            {
                // (Usar TentarComprar aqui é seguro porque o preço é o de lista)
                ResultadoCompra resultado = TentarComprar(compradorPotencial);
                if (resultado == ResultadoCompra.Sucesso)
                {
                    Console.WriteLine($"  Propriedade comprada por {compradorPotencial.Nome}! Novo saldo: ${compradorPotencial.Dinheiro}.");
                }
                // (Não deve falhar, pois já verificámos o dinheiro, mas é boa prática)
            }
            else
            {
                Console.WriteLine($"  {compradorPotencial.Nome} recusou. A propriedade continua sem dono.");
            }
        }

        // <-- MUDANÇA 5: NOVO MÉTODO para a lógica de LEILÃO -->
        private void IniciarLeilao(List<SistemaJogo.Jogador> licitantes)
        {
            Console.WriteLine($"\n--- LEILÃO INICIADO PARA: {this.Nome} ---");
            Console.WriteLine("Licitantes: " + string.Join(", ", licitantes.Select(j => j.Nome)));
            Console.WriteLine("Digite um valor para licitar (maior que o atual) ou 'P' para passar/desistir.");
            Console.WriteLine("A licitação mínima é $1.");

            int licitacaoAtual = 0;
            SistemaJogo.Jogador maiorLicitante = null;
            
            // Copia a lista para podermos remover jogadores que desistem
            var licitantesAtivos = new List<SistemaJogo.Jogador>(licitantes);
            int indiceAtual = 0;

            // O leilão continua enquanto houver mais de 1 pessoa disposta a licitar
            while (licitantesAtivos.Count > 1)
            {
                var jogadorAtual = licitantesAtivos[indiceAtual];
                
                Console.WriteLine($"\nLicitação atual: ${licitacaoAtual} (de {maiorLicitante?.Nome ?? "Ninguém"})");
                Console.WriteLine($"Turno de {jogadorAtual.Nome} (${jogadorAtual.Dinheiro}).");
                Console.Write("  > ");
                string input = Console.ReadLine().Trim();

                if (input.Equals("P", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  {jogadorAtual.Nome} desistiu.");
                    licitantesAtivos.RemoveAt(indiceAtual);
                    // Não incrementa o índice, pois a lista "encolheu"
                }
                else if (int.TryParse(input, out int novaLicitacao))
                {
                    // Tem que licitar mais, tem que ter o dinheiro, e (se for o primeiro) licitar pelo menos 1
                    if (novaLicitacao > licitacaoAtual && novaLicitacao <= jogadorAtual.Dinheiro && novaLicitacao > 0)
                    {
                        licitacaoAtual = novaLicitacao;
                        maiorLicitante = jogadorAtual;
                        Console.WriteLine($"  {jogadorAtual.Nome} licitou ${licitacaoAtual}!");
                        indiceAtual++; // Próximo jogador
                    }
                    else if (novaLicitacao <= licitacaoAtual)
                    {
                        Console.WriteLine($"  Valor inválido. Deve ser MAIOR que ${licitacaoAtual}.");
                    }
                    else if (novaLicitacao <= 0)
                    {
                         Console.WriteLine("  Valor inválido. Deve ser $1 ou mais.");
                    }
                    else // (novaLicitacao > jogadorAtual.Dinheiro)
                    {
                        Console.WriteLine($"  Valor inválido. Você só tem ${jogadorAtual.Dinheiro}.");
                    }
                }
                else
                {
                    Console.WriteLine("  Entrada inválida. Digite um número ou 'P'.");
                }

                // Gere a "volta" ao círculo
                if (indiceAtual >= licitantesAtivos.Count)
                {
                    indiceAtual = 0;
                }
            }

            // --- Fim do Leilão ---
            
            // Caso 1: Sobrou 1 jogador
            if (licitantesAtivos.Count == 1)
            {
                var vencedor = licitantesAtivos[0];
                
                // Caso 1a: Ninguém licitou nada (licitacaoAtual == 0)
                if (maiorLicitante == null)
                {
                    Console.WriteLine($"  Todos desistiram. {vencedor.Nome} é o único interessado.");
                    // O preço mínimo para comprar é $1
                    licitacaoAtual = 1;
                    Console.WriteLine($"  {vencedor.Nome} pode comprar por $1.");
                }
                
                // Caso 1b: Houve licitações, e o vencedor é o último que sobrou
                // (O 'vencedor' é o 'maiorLicitante' ou o último que não desistiu)
                Console.WriteLine($"--- LEILÃO ENCERRADO ---");
                Console.WriteLine($"  Vendido a {vencedor.Nome} por ${licitacaoAtual}!");
                
                // Transação manual (NÃO usar TentarComprar, pois o preço é o do leilão)
                vencedor.Dinheiro -= licitacaoAtual;
                this.Dono = vencedor;
                Console.WriteLine($"  O novo saldo de {vencedor.Nome} é ${vencedor.Dinheiro}.");
            }
            else // Caso 2: (licitantesAtivos.Count == 0) Todos desistiram
            {
                Console.WriteLine($"--- LEILÃO ENCERRADO ---");
                Console.WriteLine("  Todos desistiram (ou ninguém era elegível). A propriedade continua sem dono.");
            }
        }
    }
}