// Nome do Ficheiro: MonopolyBoard.cs
using System;

namespace MonopolyBoard
{
    public class Board
    {
        private readonly string[,] spaces;
        private const int Size = 7;
        
        private const int CellWidth = 13; 

        public Board()
        {
            spaces = new string[Size, Size]
            {
                // Col 0                 Col 1                Col 2                  Col 3                Col 4                Col 5                Col 6
                // ====================================================================================================================================================
                // <-- MUDANÇA AQUI: "Start" (0,0) revertido para "Prison" (como no original) -->
                { "Prison",            "Green3",            "Violet1",             "Train2",            "Red3",              "White1",            "BackToStart" },  // Row 0
                // ====================================================================================================================================================
                { "Blue3",             "Community",         "Red2",                "Violet2",           "Water Works",       "Chance",            "White2" },       // Row 1
                { "Blue2",             "Red1",              "Chance",              "Brown2",            "Community",         "Black1",            "Lux Tax" },      // Row 2
                // ====================================================================================================================================================
                // <-- MUDANÇA AQUI: "Prison" (3,3) revertido para "Start" (como no original) -->
                { "Train1",            "Green2",            "Teal1",               "Start",             "Teal2",             "Black2",            "Train3" },       // Row 3
                // ====================================================================================================================================================
                { "Blue1",             "Green1",            "Community",           "Brown1",            "Chance",            "Black3",            "White3" },       // Row 4
                { "Pink1",             "Chance",            "Orange1",             "Orange2",           "Orange3",           "Community",         "Yellow3" },      // Row 5
                { "FreePark",          "Pink2",             "Electric Company",    "Train4",            "Yellow1",           "Yellow2",           "Police" }        // Row 6
            };
        }

        public void Display()
        {
            Console.WriteLine("\t\t\t\t    === Tabuleiro de Monopólio 7x7 ===\n"); 
            DrawTopBorder();

            for (int i = 0; i < Size; i++)
            {
                DrawRow(i);
                if (i < Size - 1)
                    DrawMiddleBorder();
            }

            DrawBottomBorder();
        }
        
        /// <summary>
        /// Obtém o nome da casa usando as coordenadas da MATRIZ (0-6).
        /// </summary>
        public string GetSpaceName(int arrayRow, int arrayCol)
        {
            if (arrayRow >= 0 && arrayRow < Size && arrayCol >= 0 && arrayCol < Size)
            {
                return spaces[arrayRow, arrayCol];
            }
            return "Fora do Tabuleiro";
        }

        /// <summary>
        /// Encontra as coordenadas da MATRIZ (Linha, Coluna) de um espaço pelo seu nome.
        /// </summary>
        /// <returns>Um Tuple<int, int> (Item1 = Linha 0-6, Item2 = Coluna 0-6).</returns>
        public Tuple<int, int> GetSpaceCoords(string name)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (spaces[r, c].Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return new Tuple<int, int>(r, c); // Encontrado!
                    }
                }
            }
            
            // Se não encontrar, devolve "Start" por segurança
            Console.WriteLine($"AVISO: Casa '{name}' não encontrada no tabuleiro!");
            // ==================================================================================
            // <-- MUDANÇA AQUI: O "Start" de segurança agora é (3, 3) -->
            return new Tuple<int, int>(3, 3); // Posição (3, 3) = Start
            // ==================================================================================
        }


        // --- Métodos de Desenho (Ficam iguais) ---
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
            if (text.Length > width)
            {
                return text.Substring(0, width - 1) + "."; 
            }
            if (text.Length == width)
            {
                return text;
            }
            int padding = width - text.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }
    }
}