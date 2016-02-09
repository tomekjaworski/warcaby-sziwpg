using Checkers.Bot;
using Checkers.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Checkers
{

    public partial class MainWindow : Form, IBoard
    {
        private Random random;
        private DebugWindow dbg;

        private PawnType[,] pawn_matrix;
        private Label[,] field_matrix;
        private Color[,] selected_color_matrix;
        private Color[,] normal_color_matrix;

        private Point selected_field;

        private PawnColor current_turn, cpu_color, human_color;
        private int white_score, black_score;
        private int white_counter_old, black_counter_old, stagnation_counter;

        private IBotEngine bot;
        private int move_count, capture_count;

        public MainWindow()
        {
            InitializeComponent();
            this.Text = this.Text + " - Wersja " + Application.ProductVersion.ToString();
            this.random = new Random();
            this.dbg = new DebugWindow();
            this.btnShowDebugWindow_Click(null, null);

            this.pawn_matrix = new PawnType[8, 8];
            this.field_matrix = new Label[8, 8];
            this.selected_color_matrix = new Color[8, 8];
            this.normal_color_matrix = new Color[8, 8];

            this.selected_field = PointExt.Invalid;

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    string cname = string.Format("p{0}{1}", (char)('A' + r), 1 + c);
                    this.field_matrix[r, c] = this.panel1.Controls[cname] as Label;

                    this.normal_color_matrix[r, c] = (r + c) % 2 == 0 ? Color.White : Color.Silver;
                    this.selected_color_matrix[r, c] = (r + c) % 2 == 0 ? Color.FromArgb(255 - 50, 255, 255 - 50) : Color.FromArgb(192 - 50, 192, 192 - 50);

                    this.field_matrix[r, c].BackColor = this.normal_color_matrix[r, c];
                }

            this.current_turn = PawnColor.None;
            this.cpu_color = PawnColor.None;
            this.human_color = PawnColor.None;
            this.ShowGameState();

            // uruchomienie bota
            this.bot = new MojBot();
            this.lblBotAuthor.Text = this.bot.GetAuthor();
            this.lblBotDescription.Text = this.bot.GetDescripion();

            try
            {
                this.bot.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Wyjątek podczas uruchamiania metody Bot.Initialize():\n{0}", ex.Message),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool CheckStopConditions()
        {
            if (this.dbg.chkDontCheckStopConditions.Checked)
                return false; // skoro nie, to nie :)

            // policz piony
            // policz mozliwe ruchy
            int cwhite = 0, cblack = 0;
            int move_counter = 0;
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    if (Pawn.GetColor(this.pawn_matrix[r, c]) == PawnColor.Black) cblack++;
                    if (Pawn.GetColor(this.pawn_matrix[r, c]) == PawnColor.White) cwhite++;

                    if (Pawn.IsNormalPawn(this.pawn_matrix[r, c]))
                    {
                        move_counter += this.GetMovementCoordinates(Pawn.PointToFieldAddress(r, c), PawnType.None).Length;
                        move_counter += this.GetCaptureCoordinates(Pawn.PointToFieldAddress(r, c), PawnType.None).Length;
                    }
                }

            bool stop_game = false;
            if (cwhite == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("CZARNE piony wygrały!\n");
                sb.AppendFormat("Uzyskane punkty: {0}\n", this.black_score);
                sb.AppendFormat("Typ gracza: {0}\n", this.cpu_color == PawnColor.Black ? "Bot/CPU" : "Człowiek");

                MessageBox.Show(sb.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                stop_game = true;
            }

            if (cblack == 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("BIAŁE piony wygrały!\n");
                sb.AppendFormat("Uzyskane punkty: {0}\n", this.white_score);
                sb.AppendFormat("Typ gracza: {0}\n", this.cpu_color == PawnColor.White ? "Bot/CPU" : "Człowiek");

                MessageBox.Show(sb.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                stop_game = true;
            }

            if (white_counter_old == cwhite && black_counter_old == cblack && move_counter == 0)
                this.stagnation_counter++;
            else
            {
                this.white_counter_old = cwhite;
                this.black_counter_old = cblack;
                this.stagnation_counter = 0;
            }

            if (this.stagnation_counter >= 15)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("REMIS!");
                sb.AppendFormat("Liczba czarnych pionów: {0}\n", cblack);
                sb.AppendFormat("Liczba białych pionów: {0}\n", cwhite);
                MessageBox.Show(sb.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                stop_game = true;
            }

            if (stop_game)
            {
                this.panelBlack.Enabled = false;
                this.panelWhite.Enabled = false;
                this.richTextBox1.Enabled = false;
                this.btnDoBotMove.Enabled = false;
                this.btnEndTurn.Enabled = false;
            }
            return stop_game;
        }

        private void EndTurn(bool new_game)
        {
            // test
            bool test = false;
            test |= move_count == 1 && capture_count == 0;  // był tylko ruch
            test |= move_count == 0 && capture_count >= 1;  // było jedno lub więcej bić
            test |= !(move_count > 0 && move_count > 0);    // niemozliwe: ruch i bicie
            Debug.Assert(test);


            if (new_game)
            {
                // przygotowania do nowej gry
                this.current_turn = PawnColor.White;
                this.white_score = 0;
                this.black_score = 0;
            }
            else
            {
                // zakończenie kolejnej tury w ramach danej gry
                // jeśli pion przeciwnika jest na pierwszym polu (najbliżej gracza) to awansuj go na damę
                foreach (PawnColor pc in new PawnColor[] { PawnColor.Black, PawnColor.White })
                    for (int c = 0; c < 8; c++)
                    {
                        Point p = PointExt.Invalid;
                        if (pc == PawnColor.Black)
                            p = new Point(c, 0);
                        else
                            p = new Point(c, 7);

                        bool coronation = false;
                        if (Pawn.GetColor(this.GetPawn(p)) == pc && Pawn.IsNormalPawn(this.GetPawn(p)))
                        {
                            this.InternalSetPawn(p, Pawn.GetQueenByColor(pc));
                            coronation = true;
                        }
                        if (coronation)
                            AddPlayerLog(string.Format("Koronacja na {0}", Pawn.PointToFieldAddress(p)));
                    }

                // podlicz punkty
                int points = 0;
                if (this.move_count == 0 && this.capture_count == 0)
                {
                    // człowiek lub bot nie wykonali żadnego ruchu - poddali ruch
                    points = -1;
                }
                else
                {
                    if (this.capture_count == 1) // jedno bicie
                        points += 2;
                    if (this.capture_count > 1) // wiele bić :)
                        points += 5 * this.capture_count;
                }

                if (this.current_turn == PawnColor.Black)
                    this.black_score += points;
                else
                    this.white_score += points;

                // zmiana tury
                this.current_turn = this.current_turn == PawnColor.Black ? PawnColor.White : PawnColor.Black;
            }

            this.capture_count = 0;
            this.move_count = 0;

            this.DeselectPawn();
            this.ShowGameState();
        }

        #region Generowanie list możliwych ruchów piona/damy: z biciem i bez bicia
        public Point[] GetMovementCoordinates(string field_address, PawnType hypotetical_pawn_type)
        {
            Point pselected = Pawn.FieldAddressToPoint(field_address);
            List<Point> directions = new List<Point>();

            // jeżeli typ hipotetycznego piona nie jest pusty (none) to udaj, że PT(pselected) == hipotetyczny
            PawnType pt_selected = this.GetPawn(pselected);
            if (hypotetical_pawn_type != PawnType.None)
                pt_selected = hypotetical_pawn_type;

            if (Pawn.IsNormalPawn(pt_selected))
            {
                // sprawdź wszystkie kierunki dla piona
                Point[] deltas;
                if (Pawn.GetColor(pt_selected) == PawnColor.White)
                {
                    // białe - tylko w dół
                    deltas = new Point[] { new Point(1, 1), new Point(-1, 1) };
                }
                else
                {
                    // czarne - tylko w górę
                    deltas = new Point[] { new Point(1, -1), new Point(-1, -1) };
                }

                if (this.dbg.chkAllowNormalPawnMoveBack.Checked) // hack :)
                    deltas = new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) }; // wszystkie cztery przekątne

                // w najbliższym sąsiedztwie musi być wolne pole
                foreach (Point delta in deltas)
                {
                    Point pn = pselected.Add(delta);
                    if (Pawn.IsNone(this.GetPawn(pn, PawnType.None)) && Pawn.InBound(pn))
                        directions.Add(pn);
                }
            }

            if (Pawn.IsQueenPawn(pt_selected))
            {
                // sprawdź wszystkie kierunki dla damy
                foreach (Point delta in new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) })
                {
                    // mozna bić, jeśli pion przeciwnika ma wolne pole po nim (przed może stać dama)
                    Point p = pselected;
                    p = p.Add(delta);
                    while (Pawn.InBound(p))
                    {
                        PawnType pt = this.GetPawn(p);
                        if (!Pawn.IsNone(pt))
                            break; // już tutaj i dalej dama nie może się poruszać

                        directions.Add(p);
                        p = p.Add(delta); // przejdź o jedno pole w kierunku pdest
                    }
                }
            }
            return directions.ToArray();
        }

        public string[] GetMovementFields(string field_address)
        {
            return Pawn.PointToFieldAddress(this.GetMovementCoordinates(field_address, PawnType.None));
        }

        public string[] GetMovementFields(string field_address, PawnType hypotetical_pawn_type)
        {
            return Pawn.PointToFieldAddress(this.GetMovementCoordinates(field_address, hypotetical_pawn_type));
        }

        public Point[] GetCaptureCoordinates(string field_address, PawnType hypotetical_pawn_type)
        {
            Point pselected = Pawn.FieldAddressToPoint(field_address);
            List<Point> directions = new List<Point>();

            // jeżeli typ hipotetycznego piona nie jest pusty (none) to udaj, że PT(pselected) == hipotetyczny
            PawnType pt_selected = this.GetPawn(pselected);
            if (hypotetical_pawn_type != PawnType.None)
                pt_selected = hypotetical_pawn_type;

            if (Pawn.IsNormalPawn(pt_selected))
            {
                // w najbliższym sąsiedztwie musi być pion/dama o przeciwnym kolorze oraz następne pola muszą być puste
                foreach (PawnColor pc in new PawnColor[] { PawnColor.White, PawnColor.Black })
                    foreach (Point delta in new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) })
                    {
                        Point pn = pselected.Add(delta);
                        Point pnn = pn.Add(delta);
                        if (!Pawn.InBound(pn) || !Pawn.InBound(pnn))
                            continue;
                        if ((Pawn.GetColor(pt_selected) == pc) &&
                            (Pawn.GetColor(this.GetPawn(pn, PawnType.None)) == Pawn.GetOpponentColor(pc)) &&
                            (Pawn.IsNone(this.GetPawn(pnn, PawnType.None))))
                            directions.Add(pnn);
                    }
            }

            if (Pawn.IsQueenPawn(pt_selected))
            {
                PawnColor my_color = Pawn.GetColor(pt_selected);
                PawnColor opponent_color = Pawn.GetOpponentColor(my_color);

                // dama może bić w każdym kierunku
                foreach (Point delta in new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) })
                {
                    Point p = pselected;
                    bool opponent_pawn_found = false;
                    int capture_count = 0; // licznik zbić dam/pionów przeciwnika
                    p = p.Add(delta);
                    while (Pawn.InBound(p))
                    {
                        PawnType pt = this.GetPawn(p);

                        // jeśli miejsce jest puste a coś udało mi się zbić, to mogę tutaj postawić damę
                        if (Pawn.IsNone(pt) && (capture_count > 0))
                            directions.Add(p);

                        // dama nie może przeskoczyć nad własnym pionem (tego samego koloru)
                        if (Pawn.GetColor(pt) == my_color)
                            break;

                        if (!Pawn.IsNone(pt) && opponent_pawn_found) // dwa piony przciwnika pod rząd - nie można bić
                            break;
                        opponent_pawn_found = !Pawn.IsNone(pt);

                        if (opponent_pawn_found)
                            capture_count++;

                        p = p.Add(delta); // przejdź o jedno pole w kierunku pdest
                    }
                }
            }

            return directions.ToArray();
        }

        public string[] GetCaptureFields(string field_address)
        {
            return Pawn.PointToFieldAddress(this.GetCaptureCoordinates(field_address, PawnType.None));
        }

        public string[] GetCaptureFields(string field_address, PawnType hypotetical_pawn_type)
        {
            return Pawn.PointToFieldAddress(this.GetCaptureCoordinates(field_address, hypotetical_pawn_type));
        }

        public bool IsCaptureAvailable(string field_address)
        {
            return GetCaptureCoordinates(field_address, PawnType.None).Length > 0;
        }


        #endregion

        public PawnMoveResult MoveSelectedPawnTo(string field_addr)
        {
            field_addr = Pawn.NormalizeFieldAddress(field_addr);
            // czy gracz próbuje wykonać drugi ruch?
            if (this.move_count > 0)
                throw new GameException("Nie można wykonać dwóch ruchów!");

            // można wykonać wiele bić - po biciu nie zmienia się runda (gracz)
            // ale po biciu nie można wykonać ruchu
            if (this.capture_count > 0)
            {
                string[] arr = this.GetCaptureFields(Pawn.PointToFieldAddress(this.selected_field));
                if (!arr.Contains(field_addr))
                    throw new GameException("Po udanym biciu można wykonać tylko kolejne; nie można wykonać zwykłego ruchu!");
            }


            if (!CanMoveSelectedPawnTo(field_addr))
                return PawnMoveResult.Nothing;

            Point pselected = this.selected_field;
            Point pdest = Pawn.FieldAddressToPoint(field_addr);

            Point delta = pdest.Subtract(pselected);

            if (Pawn.IsNormalPawn(this.GetSelectedPawn())) // czy na wybranym polu stoi zwykły pion (nie dama)
            {
                if ((Math.Abs(delta.X) == 1 && Math.Abs(delta.Y) == 1)) // przesun pion
                {
                    this.move_count++;
                    this.InternalMovePawn(pdest, pselected);
                    AddPlayerLog(string.Format("Ruch z {0} na {1} (pion)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));

                    this.ShowGameState();
                    return PawnMoveResult.NormalMove;
                }

                if ((Math.Abs(delta.X) == 2 && Math.Abs(delta.Y) == 2)) // bicie
                {
                    this.capture_count++;
                    this.InternalMovePawn(pdest, pselected);

                    // usun piona przeciwnika i dodaj punkt graczowi
                    Point mid_point = pdest.Midpoint(pselected);
                    this.pawn_matrix[mid_point.Y, mid_point.X] = PawnType.None;

                    AddPlayerLog(string.Format("Bicie z {0} na {1} (pion)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));
                    this.ShowGameState();
                    return PawnMoveResult.Capture;
                }

                throw new Exception("Błąd współrzędnych ruchu; to się nie powinno zdarzyć");
            }

            if (Pawn.IsQueenPawn(this.GetSelectedPawn()))
            {
                AddPlayerLog(string.Format("Ruch z {0} na {1} (dama)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));

                Point p = pselected;
                int cap_counter = 0;
                for (int i = 0; i < Math.Abs(pselected.X - pdest.X); i++)
                {
                    // przejdź o jedno pole w kierunku pdest
                    p = new Point(p.X + Math.Sign(delta.X), p.Y + Math.Sign(delta.Y));

                    if (!Pawn.IsNone(this.GetPawn(p))) // Zdejmuj pion przeciwnika!
                    {
                        this.pawn_matrix[p.Y, p.X] = PawnType.None;
                        cap_counter++;
                        AddPlayerLog(string.Format("Bicie na {0} (dama)", Pawn.PointToFieldAddress(p)));
                    }
                }

                if (cap_counter > 0)
                    this.capture_count += capture_count;
                else
                    this.move_count++;

                this.InternalMovePawn(pdest, pselected);
                this.ShowGameState();

                // punkty
                if (cap_counter > 0)
                {
                    if (this.current_turn == PawnColor.Black)
                        this.black_score += cap_counter * 2;
                    else
                        this.white_score += cap_counter * 2;

                    return PawnMoveResult.Capture;
                }

                return PawnMoveResult.NormalMove;
            }

            // błąd
            Debug.Assert(false);
            return PawnMoveResult.Nothing;
        }

        public bool CanMoveSelectedPawnTo(string field_address)
        {
            Point pselected = this.selected_field;
            Point pdest = Pawn.FieldAddressToPoint(field_address);

            if (pselected.IsInvalid())
                throw new GameException("Nie wybrano pionu metodą SelectPawn(). Nie możesz użyć metody CanMoveSelectedPawnTo()");

            // cel musi być pustym polem
            if (!Pawn.IsNone(this.GetPawn(field_address)))
                return false;

            // pobierz wszystkie mozliwe ruchy dla piona
            Point[] simple_moves = this.GetMovementCoordinates(Pawn.PointToFieldAddress(pselected), PawnType.None); // ruchy bez bicia
            Point[] capture_moves = this.GetCaptureCoordinates(Pawn.PointToFieldAddress(pselected), PawnType.None); // ruchy z biciem
            Point[] avail_moves = simple_moves.Union(capture_moves).Distinct().ToArray(); //  C# fajny jest :)

            if (true)
            {
                Console.WriteLine("CanMoveSelectedPawnTo(): Pion {0} z {1} na {2}", this.GetPawn(pselected), Pawn.PointToFieldAddress(pselected), field_address);
                Console.WriteLine("  GetMovementCoordinates() = [{0}]", string.Join(", ", Pawn.PointToFieldAddress(simple_moves)));
                Console.WriteLine("  GetCaptureCoordinates() = [{0}]", string.Join(", ", Pawn.PointToFieldAddress(capture_moves)));
            }

            return avail_moves.Contains(pdest);

        }

        public void DeselectPawn()
        {
            if (this.selected_field.IsInvalid())
                return;

            this.field_matrix[this.selected_field.Y, this.selected_field.X].BorderStyle = BorderStyle.None;
            this.field_matrix[this.selected_field.Y, this.selected_field.X].BackColor = this.normal_color_matrix[this.selected_field.Y, this.selected_field.X];
            this.selected_field = PointExt.Invalid;
        }

        public PawnType SelectPawn(string field_address)
        {
            PawnType pc = this.GetPawn(field_address);

            if (!Pawn.EqualColor(this.current_turn, pc)) // czy próba wybrania nie swojego piona?
            {
                System.Media.SystemSounds.Hand.Play();
                return PawnType.None;
            }

            //

            Point p = Pawn.FieldAddressToPoint(field_address);

            if (p == this.selected_field)
                return pawn_matrix[p.Y, p.X];

            DeselectPawn();

            PawnType cl = pawn_matrix[p.Y, p.X];
            if (cl != PawnType.None)
            {
                this.selected_field = p;
                this.field_matrix[p.Y, p.X].BorderStyle = BorderStyle.FixedSingle;
                this.field_matrix[p.Y, p.X].BackColor = this.selected_color_matrix[p.Y, p.X];
            }

            return cl;
        }

        private void InternalSetPawn(string field_address, PawnType pt)
        {
            Point p = Pawn.FieldAddressToPoint(field_address);
            this.pawn_matrix[p.Y, p.X] = pt;
        }

        private void InternalSetPawn(Point p, PawnType pt)
        {
            Debug.Assert(Pawn.InBound(p));
            this.pawn_matrix[p.Y, p.X] = pt;
        }

        private void InternalMovePawn(Point dest, Point source)
        {
            Debug.Assert(Pawn.InBound(dest) && Pawn.InBound(source));
            PawnType pc = this.pawn_matrix[source.Y, source.X];
            this.pawn_matrix[source.Y, source.X] = PawnType.None;
            this.pawn_matrix[dest.Y, dest.X] = pc;
        }

        private void ResetCheckboard()
        {
            bool test_mode = this.dbg.chk4PawnNewGame.Checked;

            if (!test_mode)
            {
                for (int r = 0; r < 8; r++)
                    for (int c = 0; c < 8; c++)
                    {
                        this.pawn_matrix[r, c] = PawnType.None;
                        if ((r + c) % 2 == 1 && r < 3)
                            this.pawn_matrix[r, c] = PawnType.WhitePawn;
                        if ((r + c) % 2 == 1 && r > 4)
                            this.pawn_matrix[r, c] = PawnType.BlackPawn;
                    }

                return;
            }

            //tryb testowy
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    this.pawn_matrix[r, c] = PawnType.None;

            this.InternalSetPawn("F3", PawnType.BlackPawn);
            this.InternalSetPawn("F7", PawnType.BlackPawn);

            this.InternalSetPawn("B3", PawnType.WhitePawn);
            this.InternalSetPawn("B7", PawnType.WhitePawn);

            //for (int r = 0; r < 8; r++)
            //    for (int c = 0; c < 8; c++)
            //    {
            //        this.pawn_matrix[r, c] = PawnType.None;
            //        if ((r + c) % 2 == 1 && r < 3)
            //            this.pawn_matrix[r, c] = PawnType.WhitePawn;
            //        if ((r + c) % 2 == 1 && r > 6 && c > 4)
            //            this.pawn_matrix[r, c] = PawnType.BlackPawn;
            //    }

            //this.InternalSetPawn("F5", PawnType.BlackPawn);
            //this.InternalSetPawn("H5", PawnType.WhiteQueen);
            //this.InternalSetPawn("G4", PawnType.BlackPawn);
            //this.internalSetPawn("H5", PawnType.WhiteQueen);
            //this.pawn_matrix[6, 3] = PawnType.BlackPawn;
        }

        public PawnType GetPawn(string field_address)
        {
            Point p = Pawn.FieldAddressToPoint(field_address);
            return this.GetPawn(p);
        }

        private PawnType GetPawn(Point p, PawnType default_type)
        {
            if (!Pawn.InBound(p))
                return default_type;

            return GetPawn(p);
        }

        private PawnType GetPawn(Point p)
        {
            if (!Pawn.InBound(p))
                throw new GameException("Niepoprawne współrzędne piona");
            return pawn_matrix[p.Y, p.X];
        }

        public PawnType GetSelectedPawn()
        {
            if (this.selected_field.IsInvalid())
                return PawnType.None;

            return pawn_matrix[this.selected_field.Y, this.selected_field.X];
        }

        public PawnType[,] GetCheckboard()
        {
            PawnType[,] copy = this.pawn_matrix.Clone() as PawnType[,];
            return copy;
        }



        #region Obsługa GUI

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            // zaczyna zawsze biały
            if (this.random.Next() % 2 == 1)
            {
                this.cpu_color = PawnColor.White;
                this.human_color = PawnColor.Black;
            }
            else
            {
                this.cpu_color = PawnColor.Black;
                this.human_color = PawnColor.White;
            }


            this.richTextBox1.Clear();
            this.AddSystemLog(string.Format("Nowa gra. Rozpoczyna biały - {0}",
                this.cpu_color == PawnColor.White ? "BOT/CPU" : "CZŁOWIEK"));

            this.panelBlack.Enabled = true;
            this.panelWhite.Enabled = true;
            this.richTextBox1.Enabled = true;
            this.btnDoBotMove.Enabled = true;
            this.btnEndTurn.Enabled = true;

            this.ResetCheckboard();
            this.DeselectPawn();

            // obsługa bota
            try
            {
                this.bot.NewGame(this.cpu_color, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Wyjątek podczas uruchamiania metody Bot.NewGame():\n{0}", ex.Message),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.EndTurn(true);
        }

        private void btnEndTurn_Click(object sender, EventArgs e)
        {
            this.EndTurn(false);
            this.CheckStopConditions();
        }

        private void btnBotStep_Click(object sender, EventArgs e)
        {
            if (this.current_turn != this.cpu_color)
            {
                // to nie kolej bota
                this.AddSystemLog("To nie jest ruch komputera.");
                System.Media.SystemSounds.Hand.Play();
                return;
            }

#if DEBUG && false
            // w trybie debugowania bot ma rzucać wyjątki, których nie wolno przechwytywać
            try
            {
#endif
            this.bot.MakeMove();
            this.EndTurn(false);
#if DEBUG && false
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Wyjątek podczas uruchamiania metody Bot.NewGame():\n{0}", ex.Message),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif

            this.ShowGameState();
            this.CheckStopConditions();
        }

        private void btnShowDebugWindow_Click(object sender, EventArgs e)
        {
            if (sender != null)
                this.dbg.ShowDialog();
            this.btnShowDebugWindow.ForeColor = this.dbg.IsAny() ? Color.Red : SystemColors.ControlText;

        }

        private void field_MouseClick(object sender, MouseEventArgs e)
        {
            Keys k = Control.ModifierKeys;
            string field_address = (sender as Label).Name.Substring(1); // adres tekstowy pola zawarty jest w nazwie kontrolki, np. pC4 = "C4"

            // kliknięcie prawym przyciskiem myszy na polu wyświetli możliwości ruchu bez oraz ze zbijaniem piona
            // przeciwnika (pod warunkiem, że na polu jest jakiś pion).
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("GetMovementFields: {0}", string.Join(" ", GetMovementFields(field_address)));
                Console.WriteLine("GetCaptureFields: {0}", string.Join(" ", GetCaptureFields(field_address)));
                return;
            }

            // klikanie na pole z wciśniętym klawiszem Ctrl pozwala zmieniać typ piona (cyklicznie)
            if ((k & Keys.Control) != 0)
            {
                PawnType pt = GetPawn(field_address);
                if (pt == PawnType.None)
                    pt = PawnType.BlackPawn;
                else if (pt == PawnType.BlackPawn)
                    pt = PawnType.BlackQueen;
                else if (pt == PawnType.BlackQueen)
                    pt = PawnType.WhitePawn;
                else if (pt == PawnType.WhitePawn)
                    pt = PawnType.WhiteQueen;
                else
                    pt = PawnType.None;
                InternalSetPawn(field_address, pt);
                this.ShowGameState();
                return;
            }

            // kliknięcie na planszę przed rozpoczęciem gry
            if (this.current_turn == PawnColor.None || this.human_color == PawnColor.None)
                return;

            // czy użytkownik może przestawiać piony bota (grać zamiast bota - PvP)
            if (!this.dbg.chkAllowMovingBotPawnsByMouse.Checked)
                if (this.current_turn != this.human_color) // czy ruch CPU/bota?
                {
                    System.Media.SystemSounds.Hand.Play();
                    return;
                }

            // kliknięcie na pion

            PawnType pointed_color = this.GetPawn(field_address);
            if (Pawn.EqualColor(this.current_turn, pointed_color))
            {
                // człowiek kliknał na inny pion (swój)
                SelectPawn(field_address);
                return;
            }

            if (pointed_color == PawnType.None && !this.selected_field.IsInvalid())
            {
                //this.no_move_moves = false;
                PawnMoveResult pmr = MoveSelectedPawnTo(field_address);
                if (pmr == PawnMoveResult.NormalMove) // zwykły ruch
                    this.EndTurn(false);
                else
                    if (pmr == PawnMoveResult.Capture) // było bicie
                {
                    // czy z tej pozycji jest jeszcze jakieś bicie? nie? to koniec ruchu
                    // w przeciwnym razie wybierz przesuniętego piona na nowym polu, może kolejne bicie?
                    // możliwość zignorowania bicia jest odstępstwem od reguł warcabów
                    if (!IsCaptureAvailable(field_address))
                        this.EndTurn(false);
                    else
                        this.SelectPawn(field_address);
                }

                this.CheckStopConditions();
            }
        }

        private void ShowGameState()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    Label lbl = this.field_matrix[r, c];

                    switch (this.pawn_matrix[r, c])
                    {
                        case PawnType.BlackPawn: // czarny pion
                            lbl.Image = Properties.Resources.black;
                            break;
                        case PawnType.BlackQueen: // czarna dama
                            lbl.Image = Properties.Resources.black_queen;
                            break;
                        case PawnType.WhitePawn: // biały pion
                            lbl.Image = Properties.Resources.white;
                            break;
                        case PawnType.WhiteQueen: // biała dama
                            lbl.Image = Properties.Resources.white_queen;
                            break;
                        case PawnType.None: // puste pole
                            lbl.Image = null;
                            break;
                        default:
                            throw new Exception("???");
                    }

                }

            switch (current_turn)
            {
                case PawnColor.Black:
                    this.panelWhite.BackColor = SystemColors.Control;
                    this.panelBlack.BackColor = Color.LightSteelBlue;
                    break;
                case PawnColor.White:
                    this.panelWhite.BackColor = Color.LightSteelBlue;
                    this.panelBlack.BackColor = SystemColors.Control;
                    break;
                case PawnColor.None:
                    this.panelBlack.BackColor = SystemColors.Control;
                    this.panelWhite.BackColor = SystemColors.Control;
                    break;
            }

            switch (cpu_color)
            {
                case PawnColor.Black: // czarne - cpu, białe - człowiek
                    this.lblWhiteLog.Text = "Człowiek";
                    this.lblWhitePlayer.Text = "Człowiek";
                    this.lblBlackLog.Text = "Bot/CPU";
                    this.lblBlackPlayer.Text = "Bot/CPU";
                    break;
                case PawnColor.White: // czarne - człowiek, białe - cpu
                    this.lblWhiteLog.Text = "Bot/CPU";
                    this.lblWhitePlayer.Text = "Bot/CPU";
                    this.lblBlackLog.Text = "Człowiek";
                    this.lblBlackPlayer.Text = "Człowiek";
                    break;
                case PawnColor.None:
                    this.lblWhiteLog.Text = "Gracz ??";
                    this.lblWhitePlayer.Text = "Gracz ??";
                    this.lblBlackLog.Text = "Gracz ??";
                    this.lblBlackPlayer.Text = "Gracz ??";
                    break;
            }

            this.lblBlackPoints.Text = this.black_score.ToString();
            this.lblWhitePoints.Text = this.white_score.ToString();
        }


        #endregion
    }




    public class VirtualBoard : IBoard
    {
        MainWindow base_form;
        DebugWindow dbg;


        PawnType[,] pawn_matrix;
        private Point selected_field;
        private int move_count, capture_count; // TODO: nie wiem, gdzie te pola są zerowane!!!!

        public VirtualBoard(MainWindow base_form, DebugWindow dbg, PawnType[,] pawn_matrix)
        {
            this.base_form = base_form;
            this.dbg = dbg;
            this.pawn_matrix = pawn_matrix.Clone() as PawnType[,];
        }

        #region OK

        public void AddPlayerLog(string msg)
        {
            this.base_form.AddPlayerLog(msg);
        }

        public void AddSystemLog(string msg)
        {
            this.base_form.AddSystemLog(msg);
        }

        public bool CanMoveSelectedPawnTo(string field_address)
        {

            Point pselected = this.selected_field;
            Point pdest = Pawn.FieldAddressToPoint(field_address);

            if (pselected.IsInvalid())
                throw new GameException("Nie wybrano pionu metodą SelectPawn(). Nie możesz użyć metody CanMoveSelectedPawnTo()");

            // cel musi być pustym polem
            if (!Pawn.IsNone(this.GetPawn(field_address)))
                return false;

            // pobierz wszystkie mozliwe ruchy dla piona
            Point[] simple_moves = this.GetMovementCoordinates(Pawn.PointToFieldAddress(pselected), PawnType.None); // ruchy bez bicia
            Point[] capture_moves = this.GetCaptureCoordinates(Pawn.PointToFieldAddress(pselected), PawnType.None); // ruchy z biciem
            Point[] avail_moves = simple_moves.Union(capture_moves).Distinct().ToArray(); //  C# fajny jest :)

            if (true)
            {
                Console.WriteLine("CanMoveSelectedPawnTo(): Pion {0} z {1} na {2}", this.GetPawn(pselected), Pawn.PointToFieldAddress(pselected), field_address);
                Console.WriteLine("  GetMovementCoordinates() = [{0}]", string.Join(", ", Pawn.PointToFieldAddress(simple_moves)));
                Console.WriteLine("  GetCaptureCoordinates() = [{0}]", string.Join(", ", Pawn.PointToFieldAddress(capture_moves)));
            }

            return avail_moves.Contains(pdest);


        }

        public void DeselectPawn()
        {
            if (this.selected_field.IsInvalid())
                return;

            this.selected_field = PointExt.Invalid;
        }

        public string[] GetCaptureFields(string field_address)
        {
            return Pawn.PointToFieldAddress(this.GetCaptureCoordinates(field_address, PawnType.None));
        }

        public string[] GetCaptureFields(string field_address, PawnType hypotetical_pawn_type)
        {
            return Pawn.PointToFieldAddress(this.GetCaptureCoordinates(field_address, hypotetical_pawn_type));
        }

        public PawnType[,] GetCheckboard()
        {
            PawnType[,] copy = this.pawn_matrix.Clone() as PawnType[,];
            return copy;
        }


        public string[] GetMovementFields(string field_address)
        {
            return Pawn.PointToFieldAddress(this.GetMovementCoordinates(field_address, PawnType.None));
        }

        public string[] GetMovementFields(string field_address, PawnType hypothetical_pawn_type)
        {
            return Pawn.PointToFieldAddress(this.GetMovementCoordinates(field_address, hypothetical_pawn_type));
        }

        public PawnType GetPawn(string field_address)
        {
            Point p = Pawn.FieldAddressToPoint(field_address);
            return this.GetPawn(p);
        }

        private PawnType GetPawn(Point p, PawnType default_type)
        {
            if (!Pawn.InBound(p))
                return default_type;

            return GetPawn(p);
        }

        private PawnType GetPawn(Point p)
        {
            if (!Pawn.InBound(p))
                throw new GameException("Niepoprawne współrzędne piona");
            return pawn_matrix[p.Y, p.X];
        }

        #endregion




        public PawnType GetSelectedPawn()
        {
            if (this.selected_field.IsInvalid())
                return PawnType.None;

            return pawn_matrix[this.selected_field.Y, this.selected_field.X];
        }

        public bool IsCaptureAvailable(string field_address)
        {
            return GetCaptureCoordinates(field_address, PawnType.None).Length > 0;
        }

        public PawnMoveResult MoveSelectedPawnTo(string field_addr)
        {
            field_addr = Pawn.NormalizeFieldAddress(field_addr);
            // czy gracz próbuje wykonać drugi ruch?
            if (this.move_count > 0)
                throw new GameException("XXXXXXXXXXNie można wykonać dwóch ruchów!");

            // można wykonać wiele bić - po biciu nie zmienia się runda (gracz)
            // ale po biciu nie można wykonać ruchu
            if (this.capture_count > 0)
            {
                string[] arr = this.GetCaptureFields(Pawn.PointToFieldAddress(this.selected_field));
                if (!arr.Contains(field_addr))
                    throw new GameException("XXXXXXXXXXXXXXPo udanym biciu można wykonać tylko kolejne; nie można wykonać zwykłego ruchu!");
            }


            if (!CanMoveSelectedPawnTo(field_addr))
                return PawnMoveResult.Nothing;

            Point pselected = this.selected_field;
            Point pdest = Pawn.FieldAddressToPoint(field_addr);

            Point delta = pdest.Subtract(pselected);

            if (Pawn.IsNormalPawn(this.GetSelectedPawn())) // czy na wybranym polu stoi zwykły pion (nie dama)
            {
                if ((Math.Abs(delta.X) == 1 && Math.Abs(delta.Y) == 1)) // przesun pion
                {
                    this.move_count++;
                    this.InternalMovePawn(pdest, pselected);
                    AddPlayerLog(string.Format("XXXXXXXXXXXXXXXRuch z {0} na {1} (pion)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));

                    /////////////// this.ShowGameState();
                    return PawnMoveResult.NormalMove;
                }

                if ((Math.Abs(delta.X) == 2 && Math.Abs(delta.Y) == 2)) // bicie
                {
                    this.capture_count++;
                    this.InternalMovePawn(pdest, pselected);

                    // usun piona przeciwnika i dodaj punkt graczowi
                    Point mid_point = pdest.Midpoint(pselected);
                    this.pawn_matrix[mid_point.Y, mid_point.X] = PawnType.None;

                    AddPlayerLog(string.Format("XXXXXXXXXXXBicie z {0} na {1} (pion)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));
                    ///////////////this.ShowGameState();
                    return PawnMoveResult.Capture;
                }

                throw new Exception("XXXXXXXXXXXXBłąd współrzędnych ruchu; to się nie powinno zdarzyć");
            }

            if (Pawn.IsQueenPawn(this.GetSelectedPawn()))
            {
                AddPlayerLog(string.Format("XXXXXXXXXXXXXXXRuch z {0} na {1} (dama)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));

                Point p = pselected;
                int cap_counter = 0;
                for (int i = 0; i < Math.Abs(pselected.X - pdest.X); i++)
                {
                    // przejdź o jedno pole w kierunku pdest
                    p = new Point(p.X + Math.Sign(delta.X), p.Y + Math.Sign(delta.Y));

                    if (!Pawn.IsNone(this.GetPawn(p))) // Zdejmuj pion przeciwnika!
                    {
                        this.pawn_matrix[p.Y, p.X] = PawnType.None;
                        cap_counter++;
                        AddPlayerLog(string.Format("XXXXXXXXXXXXXXXXBicie na {0} (dama)", Pawn.PointToFieldAddress(p)));
                    }
                }

                if (cap_counter > 0)
                    this.capture_count += capture_count;
                else
                    this.move_count++;

                this.InternalMovePawn(pdest, pselected);
                ///////////////////////////////this.ShowGameState();

                // punkty
                //if (cap_counter > 0)
                //{
                //    if (this.current_turn == PawnColor.Black)
                //        this.black_score += cap_counter * 2;
                //    else
                //        this.white_score += cap_counter * 2;

                //    return PawnMoveResult.Capture;
                //}

                return PawnMoveResult.NormalMove;
            }

            // błąd
            Debug.Assert(false);
            return PawnMoveResult.Nothing;

        }

        public PawnType SelectPawn(string field_address)
        {
            PawnType pc = this.GetPawn(field_address);

            //if (!Pawn.EqualColor(this.current_turn, pc)) // czy próba wybrania nie swojego piona?
            //{
            //    System.Media.SystemSounds.Hand.Play();
            //    return PawnType.None;
            //}

            //

            Point p = Pawn.FieldAddressToPoint(field_address);

            if (p == this.selected_field)
                return pawn_matrix[p.Y, p.X];

            DeselectPawn();

            PawnType cl = pawn_matrix[p.Y, p.X];
            if (cl != PawnType.None)
            {
                this.selected_field = p;
                ////this.field_matrix[p.Y, p.X].BorderStyle = BorderStyle.FixedSingle;
                ////this.field_matrix[p.Y, p.X].BackColor = this.selected_color_matrix[p.Y, p.X];
            }

            return cl;
        }


        //////////////////////

        public Point[] GetMovementCoordinates(string field_address, PawnType hypotetical_pawn_type)
        {
            Point pselected = Pawn.FieldAddressToPoint(field_address);
            List<Point> directions = new List<Point>();

            // jeżeli typ hipotetycznego piona nie jest pusty (none) to udaj, że PT(pselected) == hipotetyczny
            PawnType pt_selected = this.GetPawn(pselected);
            if (hypotetical_pawn_type != PawnType.None)
                pt_selected = hypotetical_pawn_type;

            if (Pawn.IsNormalPawn(pt_selected))
            {
                // sprawdź wszystkie kierunki dla piona
                Point[] deltas;
                if (Pawn.GetColor(pt_selected) == PawnColor.White)
                {
                    // białe - tylko w dół
                    deltas = new Point[] { new Point(1, 1), new Point(-1, 1) };
                }
                else
                {
                    // czarne - tylko w górę
                    deltas = new Point[] { new Point(1, -1), new Point(-1, -1) };
                }

                if (this.dbg.chkAllowNormalPawnMoveBack.Checked) // hack :)
                    deltas = new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) }; // wszystkie cztery przekątne

                // w najbliższym sąsiedztwie musi być wolne pole
                foreach (Point delta in deltas)
                {
                    Point pn = pselected.Add(delta);
                    if (Pawn.IsNone(this.GetPawn(pn, PawnType.None)) && Pawn.InBound(pn))
                        directions.Add(pn);
                }
            }

            if (Pawn.IsQueenPawn(pt_selected))
            {
                // sprawdź wszystkie kierunki dla damy
                foreach (Point delta in new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) })
                {
                    // mozna bić, jeśli pion przeciwnika ma wolne pole po nim (przed może stać dama)
                    Point p = pselected;
                    p = p.Add(delta);
                    while (Pawn.InBound(p))
                    {
                        PawnType pt = this.GetPawn(p);
                        if (!Pawn.IsNone(pt))
                            break; // już tutaj i dalej dama nie może się poruszać

                        directions.Add(p);
                        p = p.Add(delta); // przejdź o jedno pole w kierunku pdest
                    }
                }
            }
            return directions.ToArray();
        }


        public Point[] GetCaptureCoordinates(string field_address, PawnType hypotetical_pawn_type)
        {
            Point pselected = Pawn.FieldAddressToPoint(field_address);
            List<Point> directions = new List<Point>();

            // jeżeli typ hipotetycznego piona nie jest pusty (none) to udaj, że PT(pselected) == hipotetyczny
            PawnType pt_selected = this.GetPawn(pselected);
            if (hypotetical_pawn_type != PawnType.None)
                pt_selected = hypotetical_pawn_type;

            if (Pawn.IsNormalPawn(pt_selected))
            {
                // w najbliższym sąsiedztwie musi być pion/dama o przeciwnym kolorze oraz następne pola muszą być puste
                foreach (PawnColor pc in new PawnColor[] { PawnColor.White, PawnColor.Black })
                    foreach (Point delta in new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) })
                    {
                        Point pn = pselected.Add(delta);
                        Point pnn = pn.Add(delta);
                        if (!Pawn.InBound(pn) || !Pawn.InBound(pnn))
                            continue;
                        if ((Pawn.GetColor(pt_selected) == pc) &&
                            (Pawn.GetColor(this.GetPawn(pn, PawnType.None)) == Pawn.GetOpponentColor(pc)) &&
                            (Pawn.IsNone(this.GetPawn(pnn, PawnType.None))))
                            directions.Add(pnn);
                    }
            }

            if (Pawn.IsQueenPawn(pt_selected))
            {
                PawnColor my_color = Pawn.GetColor(pt_selected);
                PawnColor opponent_color = Pawn.GetOpponentColor(my_color);

                // dama może bić w każdym kierunku
                foreach (Point delta in new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) })
                {
                    Point p = pselected;
                    bool opponent_pawn_found = false;
                    int capture_count = 0; // licznik zbić dam/pionów przeciwnika
                    p = p.Add(delta);
                    while (Pawn.InBound(p))
                    {
                        PawnType pt = this.GetPawn(p);

                        // jeśli miejsce jest puste a coś udało mi się zbić, to mogę tutaj postawić damę
                        if (Pawn.IsNone(pt) && (capture_count > 0))
                            directions.Add(p);

                        // dama nie może przeskoczyć nad własnym pionem (tego samego koloru)
                        if (Pawn.GetColor(pt) == my_color)
                            break;

                        if (!Pawn.IsNone(pt) && opponent_pawn_found) // dwa piony przciwnika pod rząd - nie można bić
                            break;
                        opponent_pawn_found = !Pawn.IsNone(pt);

                        if (opponent_pawn_found)
                            capture_count++;

                        p = p.Add(delta); // przejdź o jedno pole w kierunku pdest
                    }
                }
            }

            return directions.ToArray();
        }


        private void InternalMovePawn(Point dest, Point source)
        {
            Debug.Assert(Pawn.InBound(dest) && Pawn.InBound(source));
            PawnType pc = this.pawn_matrix[source.Y, source.X];
            this.pawn_matrix[source.Y, source.X] = PawnType.None;
            this.pawn_matrix[dest.Y, dest.X] = pc;
        }

    }


}
