using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Formats.Asn1.AsnWriter;
class Reversi : Form
{
    internal class Stone  //hanidge informatie voor objecten
    {
        public StoneColor Color { get; set; }
        public bool Show;
        public Stone(StoneColor color, bool show = true)
        {
            this.Color = color;     // de kleur
            this.Show = show;   //of je de steen ziet of niet
        }
    }
    internal enum StoneColor { Black, White, Possible } // de mogelijke statussen van de stenen in de stenen classe

    public int size = 6; // begint op bord groote 6x6
    public int new_size;
    public Stone[,] board; // het bord als array

    public bool help = false; 
    public bool no_moves = false;
    public bool end_game = false;

    public bool robot = false;
    public StoneColor robot_turn;
    public (int x, int y) next_robot_move;

    public StoneColor turn;
    public Point hier = new Point(0, 0);
    public Reversi() // het initialiseren van het scherm
    {
        this.Text = "Reversi";
        this.ClientSize = new Size(400, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;

        Button but1 = new Button();
        Button but2 = new Button();
        Button but3 = new Button();

        but1.Text = "Nieuw spel";
        but1.Location = new Point(10, 10);

        but2.Text = "Help";
        but2.Location = new Point(10, 40);

        but3.Text = "VS Robot";
        but3.Location = new Point(10, 70);

        Label label1 = new Label();
        label1.BorderStyle = BorderStyle.Fixed3D;
        label1.Size = new Size(390, 390);
        label1.Location = new Point(5, 105);
        label1.BackColor = Color.Green;

        Label label2 = new Label();
        label2.BorderStyle = BorderStyle.Fixed3D;
        label2.Size = new Size(190, 90);
        label2.Location = new Point(100, 5);
        label1.BackColor = Color.Green;

        CheckBox check1 = new CheckBox();
        check1.Text = "4 x 4";
        check1.Location = new Point(300, 10);
        CheckBox check2 = new CheckBox();
        check2.Checked = true;
        check2.Text = "6 x 6";
        check2.Location = new Point(300, 32);
        CheckBox check3 = new CheckBox();
        check3.Text = "8 x 8";
        check3.Location = new Point(300, 54);
        CheckBox check4 = new CheckBox();
        check4.Text = "10 x 10";
        check4.Location = new Point(300, 76);

        this.Controls.Add(label1);
        this.Controls.Add(label2);
        this.Controls.Add(but1);
        this.Controls.Add(but2);
        this.Controls.Add(but3);
        this.Controls.Add(check1);
        this.Controls.Add(check2);
        this.Controls.Add(check3);
        this.Controls.Add(check4);

        but1.Click += button_1;
        but2.Click += button_2;
        but3.Click += button_3;
        check1.Click += check_1;
        check2.Click += check_2;
        check3.Click += check_3;
        check4.Click += check_4;
        label1.MouseClick += click;
        label1.Paint += draw_board;
        label2.Paint += draw_score;

        start_of_game();
    }
    public void draw_board(object sender, PaintEventArgs e)
    {
        Graphics gr = e.Graphics;
        float s = size_part();
        Pen gray = new Pen(Color.Gray, 3);

        for (int i = 0; i <= size; i++)
        {
            gr.FillRectangle(Brushes.Black, 15, 20 + i * s + (i - 1) * 5, size * s + (size + 1) * 5, 5);  // horizontale lijnen
            gr.FillRectangle(Brushes.Black, 20 + i * s + (i - 1) * 5, 20, 5, size * s + (size) * 5);  // verticale lijnen
        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] != null)
                {
                    float x = 20 + j * (s + 5) + s / 8;
                    float y = 20 + i * (s + 5) + s / 8;
                    if (board[i, j].Color == StoneColor.Black)  // alle zwarten stenen
                    {
                        gr.FillEllipse(Brushes.Black, x, y, s * 3 / 4, s * 3 / 4);
                    }
                    else if (board[i, j].Color == StoneColor.White)  // alle witte stenen
                    {
                        gr.FillEllipse(Brushes.White, x, y, s * 3 / 4, s * 3 / 4);
                        gr.DrawEllipse(Pens.Black, x, y, s * 3 / 4, s * 3 / 4);
                    }
                    else if (board[i, j].Color == StoneColor.Possible && board[i, j].Show == true)  // alle mogelijke stenen wanneer je dit wilt
                    {
                        gr.DrawEllipse(gray, x, y, s * 3 / 4, s * 3 / 4);
                    }
                }
            }
        }
        if (no_moves == true && end_game == false)  // bij geen mogelijke moves laat deze tekst zien, maar niet als het spel eindigt
        {
            Font font = new Font("Atalic", 20);
            gr.DrawString("geen zetten mogelijk!", font, Brushes.Red, 80, 125);
        }

    }
    public void draw_score(object sende, PaintEventArgs e)
    {
        Graphics gr = e.Graphics;

        (int, int) scores = score();

        Font font = new Font("Atalic", 20);
        gr.DrawString($": {scores.Item1}", font, Brushes.Black, 40, 7);   // de tekst en score op het score bord
        gr.DrawString($": {scores.Item2}", font, Brushes.Black, 130, 7);
        gr.FillEllipse(Brushes.White, 10, 10, 25, 25);
        gr.DrawEllipse(Pens.Black, 10, 10, 25, 25);
        gr.FillEllipse(Brushes.Black, 100, 10, 25, 25);

        if (end_game == false)                 //hier printen we uit wie aan zet is en wie gewonnen heeft
        {
            gr.DrawString($"Its {turn}s turn", font, Brushes.Black, 5, 45);
        }
        else
        {
            if (scores.Item1 > scores.Item2)
            {
                gr.DrawString($"White wins!", font, Brushes.Black, 5, 45);
            }
            else if (scores.Item1 < scores.Item2)
            {
                gr.DrawString($"Black wins!", font, Brushes.Black, 5, 45);
            }
            else
            {
                gr.DrawString($"Remise", font, Brushes.Black, 5, 45);
            }
        }
    }
    public void button_1(object sender, EventArgs e)
    {
        start_of_game();   // button die het spel herstart
    }
    public void button_2(object sender, EventArgs e)  // button die hulp aanzet
    {
        Button actual = sender as Button;
        if (actual.Text == "Help")
        {
            actual.Text = "Help niet";  
            for (int i = 0; i < size; i++)
            {   
                for (int j = 0; j < size; j++)      // we loopen over alle mogelijke stenen en maken ze zichtbaar
                {
                    if (board[i, j] != null && board[i, j].Color == StoneColor.Possible)
                    {
                        board[i, j].Show = true;
                    }
                }
            }
            help = true;
        }
        else
        {
            actual.Text = "Help";
            for (int i = 0; i < size; i++)    // we loopen over alle mogelijke stenen en maken ze onzichtbaar
            {
                for (int j = 0; j < size; j++)
                {
                    if (board[i, j] != null && board[i, j].Color == StoneColor.Possible)
                    {
                        board[i, j].Show = false;
                    }
                }
            }
            help = false;
        }
        Reload();
    }
    public void button_3(object sender, EventArgs e) // knop 3 zet de robot aan of uit
    {
        Button actual = sender as Button;
        if (actual.Text == "VS Robot")
        {
            actual.Text = "VS Mens";
            robot_turn = other_color(turn);
            robot = true;
        }
        else
        {
            actual.Text = "VS Robot";
            robot = false;
        }
    }
    public void click(object sender, MouseEventArgs e)  // wat er gebeurt als je klikt
    {
        hier = e.Location;
        method_click(hier);
    }
    public async void method_click(Point loc)   // de functie die aan klikken vast zit
    {
        float s = size_part();
        int n = 0;
        bool right = false;

        for (int i = 0; i < size; i++) // we loopen over alle mogelijke stenen en meten de afstand tussen de klik en het vakje
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] != null && board[i, j].Color == StoneColor.Possible) 
                {
                    float x = 20 + j * (s + 5) + s / 2;
                    float y = 20 + i * (s + 5) + s / 2;
                    double dist = Math.Sqrt(Math.Pow(loc.X - x, 2) + Math.Pow(loc.Y - y, 2));

                    if (dist < (s / 2))
                    {
                        board[i, j] = new Stone(turn);      // zet de steen neer
                        placed(i, j);                       // het veranderen van andere stenen
                        turn = other_color(turn);
                        int m = possible();                 // wat zijn de mogelijke stenen voor de volgende beurt
                        n += m;
                        check_end();                        // checken of we aan het einde zijn
                        right = true;
                    }
                }
            }
        }
        if (n == 0 && right)                                // als er geen moves zijn voor de volgende beurt
        {
            no_moves = true;
            if (!end_game)
            {
                Reload();
                Thread.Sleep(3000);
                no_moves = false;
                turn = other_color(turn);
                if (possible() == 0)
                {
                    end_game = true;
                }
            }
        }
        Reload();
        if (robot && robot_turn == turn && !end_game)       // als de robot speelt
        {
            Random rnd = new Random();
            int random = rnd.Next(500, 2000);
            await Task.Delay(random);
            robot_click();
        }
    }
    public async void robot_click()  // hoe de robot klikt, eerst deed ik dat ook met methode klik maar dat had veel problemen.
    {                                  // de robot heeft nog wat problemen met het spel wanneer er geen zetten mogelijk zijn, maar hij is wel leuk
        possible();
        float s = size_part();
        board[next_robot_move.x, next_robot_move.y] = new Stone(turn);
        placed(next_robot_move.x, next_robot_move.y);
        turn = other_color(turn);
        int n = possible();
        check_end();

        if (n == 0)
        {
            no_moves = true;
            if (!end_game)
            {
                Reload();
                Thread.Sleep(3000);
                no_moves = false;
                turn = other_color(turn);
                if (possible() == 0)
                {
                    end_game = true;
                }
            }
            Random rnd = new Random();
            int random = rnd.Next(500, 2000);
            await Task.Delay(random);
            robot_click();
        }
        Reload();
    }
    public void check_1(object sender, EventArgs e) // check zet groote op 4
    {
        CheckBox now = sender as CheckBox;
        Uncheck(now);
        this.new_size = 4;
    }
    public void check_2(object sender, EventArgs e) // check zet groote op 6
    {
        CheckBox now = sender as CheckBox;
        Uncheck(now);
        this.new_size = 6;
    }
    public void check_3(object sender, EventArgs e) // check zet groote op 8
    {
        CheckBox now = sender as CheckBox;
        Uncheck(now);
        this.new_size = 8;
    }
    public void check_4(object sender, EventArgs e) // check zet groote op 10
    {
        CheckBox now = sender as CheckBox;
        Uncheck(now);
        this.new_size = 10;
    }
    private void Uncheck(CheckBox selected) // bij het checken van een box ontchecken we de andere boxen
    {
        foreach (Control control in this.Controls)
        {
            if (control is CheckBox checkbox && checkbox != selected)
            {
                checkbox.Checked = false;
            }
        }
    }
    public void Reload()  // invalidate voor functies buiten de control
    {
        foreach (Control control in this.Controls)
        {
            if (control is Label)
            {
                control.Invalidate();
            }
        }
    }
    public int possible()  // alle oude mogelijke stenen weghalen en uitrekenen welke nu mogelijk zijn
    {

        int n = 0;
        int best = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] != null && board[i, j].Color == StoneColor.Possible)
                {
                    board[i, j] = null;
                }
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] != null && board[i, j].Color == turn)
                {
                    int m = Check_Possible(i, j);
                    n += m;
                    if (m > best)
                    {
                        best = m;
                    }
                }
            }
        }

        return n;
    }

    public void placed(int i, int j)  // bij het neerzetten van een steen alle stenen die moeten veranderen veranderen
    {
        int[,] d = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } };
        for (int k = 0; k < 8; k++)
        {
            StoneColor dif = other_color(turn);

            int x = i + d[k, 0];
            int y = j + d[k, 1];
            List<(int, int)> maybe = new List<(int, int)>();

            while (x >= 0 && y >= 0 && x < size && y < size)
            {
                if (board[x, y] == null)
                {
                    break;
                }
                else if (board[x, y].Color == dif)
                {
                    maybe.Add((x, y));
                }
                else if (board[x, y].Color == turn)
                {
                    foreach ((int fx, int fy) in maybe)
                    {
                        board[fx, fy].Color = turn;
                    }
                    break;
                }
                else
                {
                    break;
                }
                x += d[k, 0];
                y += d[k, 1];
            }
        }
        Reload();
    }

    public int Check_Possible(int i, int j)  // kijken of deze steen een valide mogelijkheid is
    {
        int n = 0;
        int[,] d = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } };
        if (board[i, j] != null && board[i, j].Color == turn)
        {
            for (int k = 0; k < 8; k++)
            {
                bool maybe = false;
                StoneColor dif = other_color(turn);

                int x = i + d[k, 0];
                int y = j + d[k, 1];

                while (x >= 0 && y >= 0 && x < size && y < size)
                {
                    if (board[x, y] != null && board[x, y].Color == dif)
                    {
                        maybe = true;
                        x += d[k, 0];
                        y += d[k, 1];
                    }
                    else if (board[x, y] == null && maybe == true)
                    {
                        board[x, y] = new Stone(StoneColor.Possible, help);
                        next_robot_move = (x, y);
                        n++;

                        Reload();
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return n;
    }
    public void check_end()  // kijken of we aan het eind van het spel zijn
    {
        int n = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] != null && board[i, j].Color != StoneColor.Possible)
                {
                    n++;
                }
            }
        }
        if (n == size*size)
        {
            end_game = true;
        }
    }
    public void start_of_game() // het initialiseren van het bord
    {
        end_game = false;
        no_moves = false;
        if (new_size != 0)
        {
            size = new_size;
        }

        board = new Stone[size, size];
        board[size / 2 - 1, size / 2 - 1] = new Stone(StoneColor.Black);
        board[size / 2, size / 2 - 1] = new Stone(StoneColor.White);
        board[size / 2, size / 2] = new Stone(StoneColor.Black);
        board[size / 2 - 1, size / 2] = new Stone(StoneColor.White);

        StoneColor[] options = [StoneColor.Black, StoneColor.White];
        Random rnd = new Random();
        turn = options[rnd.Next(2)];
        robot_turn = other_color(turn);

        possible();
    }
    public (int,int) score() // reken snel de score uit
    {
        int black = 0;
        int white = 0;

        foreach (Stone stone in board)
        {
            if (stone != null)
            {
                if (stone.Color == StoneColor.Black)
                {
                    black++;
                }
                else if (stone.Color == StoneColor.White)
                {
                    white++;
                }
            }
        }
        return (white, black);
    }
    public float size_part()  // de formule waarmee we het midden van een box uitrekenen
    {
        return (350 - (size - 1) * 5) / size;
    }
    public StoneColor other_color(StoneColor color) // een simpele methode om de andere steen kleur te kunnen krijgen
    {
        if (color == StoneColor.White)
        {
            return StoneColor.Black;
        }
        else
        {
            return StoneColor.White;
        }
    }
    public static void Main() //het runnen van de klasse
    {
        Application.Run(new Reversi());
    }
}
