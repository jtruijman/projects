using System.ComponentModel.Design;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using static System.Net.Mime.MediaTypeNames;

namespace Sudoku
{
    public class Vakje //class to save info about each individual square of the sudoku
    {
        public int Waarde { get; set; } //value in the square
        public bool Gefixeerd { get; set; } //is the square fixated or not

        public Vakje(int n)
        {
            Waarde = n;
            if (n == 0)
            {
                Gefixeerd = false;
            }
            else
            {
                Gefixeerd = true;
            }
        }
    }


    public class Sudoku
    {
        public int[] Input { get; set; }
        public Vakje[,] Field { get; set; }  //the actual sudoku 

        public int val_score;
        public List<int> rij_scores;
        public List<int> kolom_scores;

        public Sudoku(int[] input)
        {
            rij_scores = new List<int>();
            kolom_scores = new List<int>();
            Input = input;
            val_score = 0;
            Vakje[,] field = new Vakje[9, 9];
            Field = field;
            int n = 0;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    field[i, j] = new Vakje(input[n]);  //squares are filled with their respective numbers from the input
                    n++;
                }
            }
        }

        public void vullen()
        {
            for (int i = 0; i < 9; i++)  // per 3x3 box
            {
                List<int> opties = [1, 2, 3, 4, 5, 6, 7, 8, 9,];
                for (int j = 0; j < 9; j++)
                {
                    if (Field[rij(i, j), kolom(i, j)].Gefixeerd == true)
                    {
                        opties.Remove(Field[rij(i, j), kolom(i, j)].Waarde);   //first we loop over the box to see which numbers are already there.
                    }
                }
                for (int j = 0; j < 9; j++)
                {
                    if (Field[rij(i, j), kolom(i, j)].Gefixeerd == false)
                    {
                        Random rnd = new Random();
                        int index = rnd.Next(opties.Count);

                        Field[rij(i, j), kolom(i, j)].Waarde = opties[index];   //now we loop over every empty sqaure and choose a
                        opties.Remove(opties[index]);                           //random number from the remaining ones
                    }
                }
            }
            this.begin_eval();  //we initialize the score for the randomly filled sudoku
        }
        public int rij(int box, int pos)
        {
            return box / 3 * 3 + pos / 3;  //the formula for knowing the row index given the box and position
        }

        public int kolom(int box, int pos)
        {
            return (box % 3) * 3 + (pos % 3);  //the formula for knowing the column index given the box and position
        }

        public void print()  //function that prints the sudoku
        {
            Console.WriteLine();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write("{0} ", Field[i, j].Waarde);
                }
                Console.WriteLine();
            }
        }

        public int swap_eval((int rij, int kolom) v_1, (int rij, int kolom) v_2, bool real_swap = false)
        {

            int prev_score_1 = (rij_scores[v_1.rij] + kolom_scores[v_1.kolom]);  // here we get the current score of the current correspodning row and column
            int prev_score_2 = (rij_scores[v_2.rij] + kolom_scores[v_2.kolom]);  // of both squares
            int prev_score = prev_score_1 + prev_score_2;

            swap((v_1.rij, v_1.kolom), (v_2.rij, v_2.kolom));  // we swap the two squares

            (int, int) new_score_1 = partial_eval(v_1.rij, v_1.kolom); //we calculate the new row and column score of the two swapped squares
            (int, int) new_score_2 = partial_eval(v_2.rij, v_2.kolom);
            int new_score = new_score_1.Item1 + new_score_1.Item2 + new_score_2.Item1 + new_score_2.Item2;
            int result = prev_score - new_score; //the difference between the row and column score


            if (real_swap == false)
            {
                swap((v_1.rij, v_1.kolom), (v_2.rij, v_2.kolom));  //if true swap is set to false we want to set the squares back on their right place
            }
            else
            {
                rij_scores[v_1.rij] = new_score_1.Item1;  //if real_swap is set to true we don't want to set the squares back but update the row and column score
                rij_scores[v_2.rij] = new_score_2.Item1;
                kolom_scores[v_1.kolom] = new_score_1.Item2;
                kolom_scores[v_2.kolom] = new_score_2.Item2;
                val_score -= result;
            }

            return result;

        }

        public (int, int) partial_eval(int rij, int kolom)
        {
            HashSet<int> set = new HashSet<int>();   //this function loops over the row and column of a specified square and calculates the val_score
            for (int i = 0; i < 9; i++)
            {
                set.Add(Field[rij, i].Waarde);
            }
            int count_rij = (9 - set.Count());
            set.Clear();

            for (int i = 0; i < 9; i++)
            {
                set.Add(Field[i, kolom].Waarde);
            }
            int count_kolom = (9 - set.Count());

            return (count_rij, count_kolom);
        }

        public void begin_eval() //calculates the amount of duplicate numbers in every row and column at initialisation
        {
            rij_scores.Clear(); kolom_scores.Clear();
            HashSet<int> set = new HashSet<int>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    set.Add(Field[i, j].Waarde);
                }
                rij_scores.Add(9 - set.Count());
                set.Clear();
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    set.Add(Field[j, i].Waarde);
                }
                kolom_scores.Add(9 - set.Count());
                set.Clear();
            }
            val_score = rij_scores.Sum() + kolom_scores.Sum();
        }

        public void swap((int rij, int kolom) vakje1, (int rij, int kolom) vakje2) //swaps two squares
        {
            var vakje1_temp = Field[vakje1.rij, vakje1.kolom];
            Field[vakje1.rij, vakje1.kolom] = Field[vakje2.rij, vakje2.kolom];
            Field[vakje2.rij, vakje2.kolom] = vakje1_temp;
        }


        public void random_walk(int S)  //this function randomly picks a 3x3 box and two posistions and swaps them
        {
            Random rnd = new Random();
            Random rnd1 = new Random();
            Random rnd2 = new Random();

            for (int i = 0; i < S; i++)
            {
                int box = rnd.Next(9);
                int blok_pos1 = rnd1.Next(9);
                int blok_pos2 = rnd2.Next(9);
                if (Field[rij(box, blok_pos1), kolom(box, blok_pos1)].Gefixeerd == false && Field[rij(box, blok_pos2), kolom(box, blok_pos2)].Gefixeerd == false)
                {
                    swap_eval((rij(box, blok_pos1), kolom(box, blok_pos1)), (rij(box, blok_pos2), kolom(box, blok_pos2)), true);
                }
                else
                {
                    i--;
                }
            }
        }

        public void los_op()
        {
            this.vullen();
            int S = 0;

            while (val_score > 0)
            {
                Random rnd = new Random();  //we pick a random box
                int box = rnd.Next(9);
                int min = 0;
                (int, int) vakjes = (-1, -1);
                for (int i = 8; i >= 0; i--)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (Field[rij(box, i), kolom(box, i)].Gefixeerd == false && Field[rij(box, j), kolom(box, j)].Gefixeerd == false)
                        {
                            int check = swap_eval((rij(box, i), kolom(box, i)), (rij(box, j), kolom(box, j)));
                            if (check >= min)                     //we try every possible swap in a box and calculate what this would do to the current score
                            {                                     //we save the lowest score
                                min = check;
                                vakjes = (i, j);
                            }
                        }
                    }
                }
                if (vakjes == (-1, -1)) //check whether there was a swap that caused an improvement
                {
                    if (S == 6)  //perform a random walk after not finding a better swap after S times
                    {
                        random_walk(S);
                        S = 0;
                    }
                    S++;
                }
                else  //if we do find a swap that improves the scores we reset the counter and swap the boxes
                {
                    swap_eval((rij(box, vakjes.Item1), kolom(box, vakjes.Item1)), (rij(box, vakjes.Item2), kolom(box, vakjes.Item2)), true);
                    S = 0;
                }
            }
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string[] string_input = Console.ReadLine().Split(' ');
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            int[] input = Array.ConvertAll(string_input, int.Parse);
            Sudoku test = new Sudoku(input);
            test.print();
            test.los_op();
            test.print();
        }
    }
}