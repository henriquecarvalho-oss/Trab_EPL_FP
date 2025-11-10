using System;

// Criei um novo namespace para organizar a lógica do jogo
namespace MonopolyGameLogic
{
    // Esta 'struct' (estrutura) serve para guardar o resultado
    // dos dois dados de forma organizada.
    public readonly struct DiceResult
    {
        public int HorizontalMove { get; }
        public int VerticalMove { get; }

        public DiceResult(int horizontal, int vertical)
        {
            HorizontalMove = horizontal;
            VerticalMove = vertical;
        }
    }

    public class DiceRoller
    {
        // O gerador de números aleatórios. 
        // É importante criar apenas UMA instância e reutilizá-la.
        private readonly Random random;

        // A lista de valores possíveis para um dado, 
        // excluindo o zero, como pediu.
        private readonly int[] possibleValues = { -3, -2, -1, 1, 2, 3 };

        public DiceRoller()
        {
            random = new Random();
        }

        // Método privado que lança UM dado
        private int RollSingleDie()
        {
            // Escolhe um índice aleatório do array 'possibleValues'
            // O 'possibleValues.Length' é 6, por isso 'Next(0, 6)' 
            // vai dar um número de 0 a 5.
            int index = random.Next(0, possibleValues.Length);
            return possibleValues[index];
        }

        // Método público que o seu SistemaJogo vai chamar
        // Lança os dois dados e devolve o resultado
        public DiceResult Roll()
        {
            int horizontal = RollSingleDie();
            int vertical = RollSingleDie();
            
            // Devolve a struct com os dois valores
            return new DiceResult(horizontal, vertical);
        }
    }
}