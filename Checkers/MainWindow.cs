﻿using Checkers.Bot;
using Checkers.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Checkers
{

    public partial class MainWindow : Form, IBoard
    {
        private Random random;

        private PawnType[,] pawn_matrix;
        private Label[,] field_matrix;
        private Color[,] selected_color_matrix;
        private Color[,] normal_color_matrix;

        private int selected_row, selected_col;

        private PawnColor current_turn, cpu_color, human_color;
        private int white_score, black_score;
        private int white_counter_old, black_counter_old, stagnation_counter;

        private IBotEngine bot;
        private bool movement_done;

        public MainWindow()
        {
            InitializeComponent();
            this.Text = this.Text + " - Wersja " + Application.ProductVersion.ToString();
            this.random = new Random();

            this.pawn_matrix = new PawnType[8, 8];
            this.field_matrix = new Label[8, 8];
            this.selected_color_matrix = new Color[8, 8];
            this.normal_color_matrix = new Color[8, 8];

            this.selected_col = -1;
            this.selected_row = -1;

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    string cname = string.Format("p{0}{1}", (char)('A' + r), 1 + c);
                    this.field_matrix[r, c] = this.panel1.Controls[cname] as Label;

                    this.normal_color_matrix[r, c] = (r + c) % 2 == 0 ? Color.White : Color.Silver;
                    this.selected_color_matrix[r, c] = (r + c) % 2 == 0 ? Color.FromArgb(255-50, 255, 255-50) : Color.FromArgb(192-50, 192, 192-50);

                    this.field_matrix[r, c].BackColor = this.normal_color_matrix[r, c];
                }

            this.current_turn = PawnColor.None;
            this.cpu_color = PawnColor.None;
            this.human_color = PawnColor.None;
            this.UpdateCheckboard();

            // uruchomienie bota
            this.bot = new MojBot();
            this.lblBotAuthor.Text = this.bot.GetAuthor();
            this.lblBotDescription.Text = this.bot.GetDescripion();

            try
            {
                this.bot.Initialize();
            } catch(Exception ex)
            {
                MessageBox.Show(string.Format("Wyjątek podczas uruchamiania metody Bot.Initialize():\n{0}", ex.Message),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pion_Click(object sender, EventArgs e)
        {
            if (this.current_turn == PawnColor.None || this.human_color == PawnColor.None)
                return;

            /* */
            if (this.current_turn != this.human_color) // czy ruch CPU/bota?
            {
                System.Media.SystemSounds.Hand.Play();
                return;
            }
            /* */


            // kliknięcie na pion
            string field_addr = (sender as Label).Name.Substring(1);
            PawnType pointed_color = this.GetPawn(field_addr);

            if (Pawn.EqualColor(this.current_turn, pointed_color))
            {
                // człowiek kliknał na inny pion (swój)
                SelectPawn(field_addr);
                return;
            }

            if (pointed_color == PawnType.None && this.selected_col != -1 && this.selected_row != -1)
            {
                this.movement_done = false;
                MoveSelectedPawnTo(field_addr);

                this.CheckStopConditions();
            }
        }

        private void CheckStopConditions()
        {
            // policz piony
            int cwhite = 0, cblack = 0;
            for(int r = 0; r < 8;r++)
                for (int c = 0; c < 8; c++)
                {
                    if (Pawn.GetColor(this.pawn_matrix[r, c]) == PawnColor.Black) cblack++;
                    if (Pawn.GetColor(this.pawn_matrix[r, c]) == PawnColor.White) cwhite++;
                }

            bool stop_game = false;
            if (cwhite == 0)
            {
                MessageBox.Show("Czarne piony wygrały!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                stop_game = true;
            }

            if (cblack == 0)
            {
                MessageBox.Show("Białe piony wygrały!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                stop_game = true;
            }

            if (white_counter_old == cwhite && black_counter_old == cblack)
                this.stagnation_counter++;
            else
            {
                this.white_counter_old = cwhite;
                this.black_counter_old = cblack;
                this.stagnation_counter = 0;
            }

            if (this.stagnation_counter >= 15)
            {
                MessageBox.Show("REMIS", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                stop_game = true;
            }

            if (stop_game)
            {
                this.panelBlack.Enabled = false;
                this.panelBlack.Enabled = false;
                this.richTextBox1.Enabled = false;
                this.btnBotStep.Enabled = false;
                this.btnNextTurn.Enabled = false;
            }
        }

        private void NextTurn()
        {
            this.current_turn = this.current_turn == PawnColor.Black ? PawnColor.White : PawnColor.Black;
            //this.movement_done = false;
            this.DeselectPawn();
            this.UpdateCheckboard();
        }

        public bool MoveSelectedPawnTo(string field_addr)
        {
            if (this.movement_done)
                throw new GameException("Nie można wykonać dwóch ruchów!");

            if (!CanMoveSelectedPawnTo(field_addr))
                return false;

            Point pselected = new Point(this.selected_col, this.selected_row);
            Point pdest = Pawn.FieldAddressToPoint(field_addr);

            Point delta = pdest.Subtract(pselected);

            if (Pawn.IsNormalPawn(this.GetSelectedPawn())) // czy na wybranym polu stoi zwykły pion (nie dama)
            {
                if ((Math.Abs(delta.X) == 1 && Math.Abs(delta.Y) == 1)) // przesun pion
                {
                    PawnType pc = this.pawn_matrix[pselected.Y, pselected.X];
                    this.pawn_matrix[pselected.Y, pselected.X] = PawnType.None;
                    this.pawn_matrix[pdest.Y, pdest.X] = pc;

                    AddPlayerLog(string.Format("Ruch z {0} na {1} (pion)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));
                    this.movement_done = true;
                }

                if ((Math.Abs(delta.X) == 2 && Math.Abs(delta.Y) == 2)) // 
                {
                    PawnType pc = this.pawn_matrix[pselected.Y, pselected.X];
                    this.pawn_matrix[pselected.Y, pselected.X] = PawnType.None;
                    this.pawn_matrix[pdest.Y, pdest.X] = pc;

                    // usun piona przeciwnika i dodaj punkt graczowi
                    Point mid_point = pdest.Midpoint(pselected);
                    this.pawn_matrix[mid_point.Y, mid_point.X] = PawnType.None;
                    if (this.current_turn == PawnColor.Black)
                        this.black_score += 2;
                    else
                        this.white_score += 2;

                    AddPlayerLog(string.Format("Bicie z {0} na {1} (pion)", Pawn.PointToFieldAddress(pselected), Pawn.PointToFieldAddress(pdest)));
                    this.movement_done = true;
                }

                if (Pawn.IsNormalPawn(this.GetPawn(pdest))) // czy zwykły pion pojawił się na pierwszym wierszu pola przeciwnika?
                    if (this.current_turn == PawnColor.White && pdest.Y == 7||
                        this.current_turn == PawnColor.Black && pdest.Y == 0)
                    {
                        // awans na damę!
                        this.pawn_matrix[pdest.Y, pdest.X] = this.current_turn == PawnColor.Black ? PawnType.BlackQueen : PawnType.WhiteQueen;
                        AddPlayerLog(string.Format("Koronacja na {0}", Pawn.PointToFieldAddress(pdest)));
                    }

                this.NextTurn();


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

                    if (this.GetPawn(p) != PawnType.None) // Zdejmuj pion przeciwnika!
                    {
                        this.pawn_matrix[p.Y, p.X] = PawnType.None;
                        cap_counter++;
                        AddPlayerLog(string.Format("Bicie na {0} (dama)", Pawn.PointToFieldAddress(p)));
                    }
                }

                PawnType pc = this.pawn_matrix[pselected.Y, pselected.X];
                this.pawn_matrix[pselected.Y, pselected.X] = PawnType.None;
                this.pawn_matrix[pdest.Y, pdest.X] = pc;

                // punkty
                if (cap_counter > 0)
                {
                    if (this.current_turn == PawnColor.Black)
                        this.black_score += (cap_counter == 1) ? 2 : (cap_counter - 1) * 5;
                    else
                        this.white_score += (cap_counter == 1) ? 2 : (cap_counter - 1) * 5;
                }

                this.movement_done = true;
                this.NextTurn();

            }

            this.UpdateCheckboard();
            return true;
        }



        public bool CanMoveSelectedPawnTo(string field_addr)
        {
            Point pselected = new Point(this.selected_col, this.selected_row);
            Point pdest = Pawn.FieldAddressToPoint(field_addr);

            Point delta = pdest.Subtract(pselected);

            if (Pawn.IsNormalPawn(this.GetSelectedPawn())) // czy na wybranym polu stoi zwykły pion (nie dama)
            {
                // ruch po przekątnej o trzy pola (bicie) - możliwe zarówno do tyłu jak i do przodu
                if (Math.Abs(delta.X) == 2 && Math.Abs(delta.Y) == 2)
                {
                    Point mid = pdest.Midpoint(pselected);
                    PawnType midpawn_color = GetPawn(mid);
                    if (this.current_turn == PawnColor.Black && Pawn.GetColor(midpawn_color) == PawnColor.White) // czarny zbija białego piona
                        return true;
                    if (this.current_turn == PawnColor.White && Pawn.GetColor(midpawn_color) == PawnColor.Black) // biał zbija czarnego piona
                        return true;

                    return false; // cos jest nie tak z midpawn_color...
                }

                // zwykły ruch tylko do PRZODU (białe w dół, czarne w górę)
                if (this.current_turn == PawnColor.White && delta.Y < 1 ||
                    this.current_turn == PawnColor.Black && delta.Y > -1)
                    return false;

                // ruch po przekątnej o jedno pole
                if ((Math.Abs(delta.X) == 1 && Math.Abs(delta.Y) == 1))
                    return true;

                return false; // błędny ruch
            }

            if (Pawn.IsQueenPawn(this.GetSelectedPawn())) // czy to dama?
            {
                // ruch tylko po przekątnej
                if (Math.Abs(delta.X) != Math.Abs(delta.Y))
                    return false;

                // mozna bić, jeśli pion przeciwnika ma wolne pole po nim (przed może stać dama)
                Point p = pselected;
                bool opponent_pawn_found = false;
                for (int i = 0; i < Math.Abs(pselected.X - pdest.X); i++)
                {
                    // przejdź o jedno pole w kierunku pdest
                    p = new Point(p.X + Math.Sign(delta.X), p.Y + Math.Sign(delta.Y));

                    if (Pawn.GetColor(this.GetPawn(p)) == this.current_turn)
                        return false; // dama nie może przeskoczyć nad własnym pionem (tego samego koloru)

                    if (this.GetPawn(p) != PawnType.None && opponent_pawn_found) // dwa piony przciwnika pod rząd - nie można bić
                        return false;
                    opponent_pawn_found = this.GetPawn(p) != PawnType.None;
                }

                return true; // mozna bić!
            }

            return false;
        }




        public void DeselectPawn()
        {

            if (this.selected_col == -1 && this.selected_row == -1)
                return;

            this.field_matrix[this.selected_row, this.selected_col].BorderStyle = BorderStyle.None;
            this.field_matrix[this.selected_row, this.selected_col].BackColor = this.normal_color_matrix[this.selected_row, this.selected_col];
            this.selected_col = -1;
            this.selected_row = -1;
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

            if (p.Y == this.selected_row && p.X == this.selected_col)
                return pawn_matrix[p.Y, p.X];

            DeselectPawn();

            PawnType cl = pawn_matrix[p.Y, p.X];
            if (cl != PawnType.None)
            {
                this.selected_col = p.X;
                this.selected_row = p.Y;
                this.field_matrix[this.selected_row, this.selected_col].BorderStyle = BorderStyle.FixedSingle;
                this.field_matrix[this.selected_row, this.selected_col].BackColor = this.selected_color_matrix[this.selected_row, this.selected_col];
            }

            return cl;
        }
        

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            this.ResetCheckboard();

            // zaczyna zawsze biały
            if (this.random.Next() % 2 == 1)
            {
                this.cpu_color = PawnColor.White;
                this.human_color = PawnColor.Black;
            } else
            {
                this.cpu_color = PawnColor.Black;
                this.human_color = PawnColor.White;
            }
            this.current_turn = PawnColor.White;


            this.richTextBox1.Clear();
            this.AddSystemLog(string.Format("Nowa gra. Rozpoczyna biały - {0}",
                this.cpu_color == PawnColor.White ? "BOT/CPU" : "CZŁOWIEK"));

            this.panelBlack.Enabled = true;
            this.panelBlack.Enabled = true;
            this.richTextBox1.Enabled = true;
            this.btnBotStep.Enabled = true;
            this.btnNextTurn.Enabled = true;

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

            this.white_score = 0;
            this.black_score = 0;
            this.UpdateCheckboard();

        }

        private void ResetCheckboard()
        {
            bool test_mode = false;

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
                {
                    this.pawn_matrix[r, c] = PawnType.None;
                    if ((r + c) % 2 == 1 && r < 3)
                        this.pawn_matrix[r, c] = PawnType.WhitePawn;
                    if ((r + c) % 2 == 1 && r > 6 && c > 4)
                        this.pawn_matrix[r, c] = PawnType.BlackPawn;
                }

            this.pawn_matrix[6, 1] = PawnType.WhitePawn;
            this.pawn_matrix[6, 3] = PawnType.BlackPawn;
        }

        private void btnNextTurn_Click(object sender, EventArgs e)
        {
            // zabierz jeden punkt
            if (this.current_turn == PawnColor.Black)
                this.black_score--;
            else
                this.white_score--;

            this.NextTurn();
            this.CheckStopConditions();
        }

        private void btnBotStep_Click(object sender, EventArgs e)
        {
            if (this.current_turn != this.cpu_color)
            {
                // to nie kolej bota
                System.Media.SystemSounds.Hand.Play();
                return;
            }

            try
            {
                this.movement_done = false;

                this.bot.MakeMove();
                PawnColor current = this.current_turn;
                if (!this.movement_done) // bot nie przesunął piona - poddał ruch
                    if (current == PawnColor.Black)
                        this.black_score--;
                    else
                        this.white_score--;

                //this.NextTurn(); // następny gracz
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Wyjątek podczas uruchamiania metody Bot.NewGame():\n{0}", ex.Message),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.UpdateCheckboard();
            this.CheckStopConditions();
        }

        private void UpdateCheckboard()
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
                            lbl.Image =  Properties.Resources.white;
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


        public PawnType GetPawn(string field_address)
        {
            Point p = Pawn.FieldAddressToPoint(field_address);
            return this.GetPawn(p);
        }

        private PawnType GetPawn(Point p)
        {
            if (p.X < 0 || p.X > 7)
                throw new GameException("Niepoprawne współrzędne piona (X)");
            if (p.Y < 0 || p.Y > 7)
                throw new GameException("Niepoprawne współrzędne piona (Y)");
            return pawn_matrix[p.Y, p.X];
        }

        public PawnType GetSelectedPawn()
        {
            if (this.selected_col == -1 || this.selected_row == -1)
                return PawnType.None;

            return pawn_matrix[this.selected_row, this.selected_col];
        }


    }





}