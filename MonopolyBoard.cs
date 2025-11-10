using System;

namespace MonopolyBoard
{
    public class Board
    {
        private readonly string[,] spaces;
        private const int Size = 7;
        private const int CellWidth = 17;

        public Board()
        {
            spaces = new string[Size, Size]
            {
                { "Prison", "Green3", "Violet1", "Train2", "Red3", "White1", "BackToStart" },
                { "Blue3", "Community", "Red2", "Violet2", "Water Works", "Chance", "White2" },
                { "Blue2", "Red1", "Chance", "Brown2", "Community", "Black1", "Lux Tax" },
                { "Train1", "Green2", "Teal1", "Start", "Teal2", "Black2", "Train3" },
                { "Blue1", "Green1", "Community", "Brown1", "Chance", "Black3", "White3" },
                { "Pink1", "Chance", "Orange1", "Orange2", "Orange3", "Community", "Yellow3" },
                { "FreePark", "Pink2", "Electric Company", "Train4", "Yellow1", "Yellow2", "Police" }
            };
        }

        // ... (método Display() e outros ficam iguais) ...
        public void Display()
        {
            Console.WriteLine("\t\t\t\t\t    === Tabuleiro de Monopólio 7x7 ===\n");
            DrawTopBorder();

            for (int i = 0; i < Size; i++)
            {
                DrawRow(i);
                if (i < Size - 1)
                    DrawMiddleBorder();
            }

            DrawBottomBorder();
        }

        // *** MÉTODO NOVO ADICIONADO AQUI ***
        // Este método permite que outros ficheiros peçam o nome da casa
        // Nota: 'row' é Y, 'col' é X
        public string GetSpaceName(int row, int col)
        {
            // Verificação de segurança para não dar erro se sair do tabuleiro
            if (row >= 0 && row < Size && col >= 0 && col < Size)
            {
                return spaces[row, col];
            }
            return "Fora do Tabuleiro";
        }


        // ... (métodos DrawTopBorder, DrawMiddleBorder, etc. ficam iguais) ...
        private void DrawTopBorder()
        {
            Console.Write("┌");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(new string('─', CellWidth));
                if (i < Size - 1) Console.Write("┬");
            }
            Console.WriteLine("┐");
        }

        private void DrawMiddleBorder()
        {
            Console.Write("├");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(new string('─', CellWidth));
                if (i < Size - 1) Console.Write("┼");
            }
            Console.WriteLine("┤");
        }

        private void DrawBottomBorder()
        {
            Console.Write("└");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(new string('─', CellWidth));
                if (i < Size - 1) Console.Write("┴");
            }
            Console.WriteLine("┘");
        }

        private void DrawRow(int row)
        {
            Console.Write("│");
            for (int j = 0; j < Size; j++)
            {
                string text = CenterText(spaces[row, j], CellWidth);
                Console.Write($"{text}│");
            }
            Console.WriteLine();
        }

        private string CenterText(string text, int width)
        {
            text = text.Trim();
            int padding = width - text.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }
    }
}