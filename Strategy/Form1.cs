using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Forms.ComboBox;

namespace Strategy
{
    public partial class StrategyForm : Form
    {
        public float ScrollStep {  get { return 0.01f / Board.Zoom; } }
        //ścieżka nie jest uniwersalna (działa tylko po wprowadzeniu odpowiedniej ścieżki do plików - własnej ścieżki)
        //potem można pomyśleć jak zrobić ścieżkę uniwersalną, aby móc wczytywac pliki do wyboru z danego folderu na danym komputerze
        private string folderPath = @"C:\Program Files (x86)\Dispel\Map";


        public StrategyForm()
        {
            InitializeComponent();
            HerosList.DataSource = Board.Game.ActivePlayer.Heroes;
            HerosList.DisplayMember = "Name";
            HerosList.BackColor = Board.Game.ActivePlayer.ID;
            Board.MouseWheel += TBoard1_MouseWheel;
            Board.Scroll += BoardScroll;
            Board.Game.OnResourceChanged = ResourceChanged;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            LoadFilesToComboBox1(folderPath);
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            comboBox1.DisplayMember= "Name";
        }

        private void BoardScroll(object sender, ScrollEventArgs e)
        {
            MapView.Invalidate();
        }

        private void MapView_Paint(object sender, PaintEventArgs e)
        {
            var scaleX = (float)MapView.Width / Board.Game.Map.Width;
            var scaleY = (float)MapView.Height / Board.Game.Map.Height;
            e.Graphics.ScaleTransform(scaleX, scaleY);
            e.Graphics.DrawImage(Board.Game.Map, new Rectangle(0, 0, Board.Game.Map.Width, Board.Game.Map.Height));
            e.Graphics.DrawRectangle(Pens.Blue, Board.Viewport);
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (Cursor.Position.X < Margin.Left)
                Board.ScrollPos -= new SizeF(ScrollStep, 0);
            else if (Cursor.Position.X > Width - Margin.Right)
                Board.ScrollPos += new SizeF(ScrollStep, 0);
            if (Cursor.Position.Y < Margin.Top)
                Board.ScrollPos -= new SizeF(0, ScrollStep);
            else if (Cursor.Position.Y > Height - Margin.Bottom)
                Board.ScrollPos += new SizeF(0, ScrollStep);
        }

        private void MapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var p = new PointF(e.X, e.Y);
                p.X /= MapView.Width;
                p.Y /= MapView.Height;
                Board.ScrollPos = p;
            }
        }

        private void TBoard1_MouseWheel(object sender, MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta < 0)
                Board.Zoom *= 0.9f;
            else
                Board.Zoom *= 1.1f;
            Board.Invalidate();
            MapView.Invalidate();
        }

        private void HerosList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Board.Game.ActivePlayer.SelectedHero = Board.Game.ActivePlayer.Heroes[HerosList.SelectedIndex];
            var hero = Board.Game.ActivePlayer.SelectedHero;
            var p = new PointF(hero.Cell.X, hero.Cell.Y);
            p.X /= Board.Game.Map.Width;
            p.Y /= Board.Game.Map.Height;
            Board.ScrollPos = p;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Board.Game.NextTurn();
            HerosList.DataSource = Board.Game.ActivePlayer.Heroes;
            HerosList.DisplayMember = "Name";
            HerosList.BackColor = Board.Game.ActivePlayer.ID;
        }

        private void ResourceChanged(object sender, EventArgs e)
        {
            var activePlayer = Board.Game.ActivePlayer;
            var names = Enum.GetNames(typeof(TResource.ResType));
            ResourceView.Items.Clear();
            for (int i = 0; i < activePlayer.Resources.Length; i++)
            {
                var item = new ListViewItem(names[i], i);
                item.SubItems.Add(activePlayer.Resources[i].ToString());
                ResourceView.Items.Add(item);
            }
        }

        private void StrategyForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.T: ImportTiles(); break;
            }
        }
        
        void ImportTiles()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Board.Game.ImportTiles(openFileDialog1.FileName);
                Board.Invalidate();
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        //dodany kod od tego momentu (poprawnie są wyświetlane nazwy plików w bieżącym katalogu)
        private void LoadFilesToComboBox1(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                comboBox1.Items.Clear();
                string[] files = Directory.GetFiles(folderPath);

                MessageBox.Show($"Znaleziono {files.Length} plików w folderze: {folderPath}"); //liczba plików, ale tak naprawdę tego nie musi tutaj być

                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        comboBox1.Items.Add(Path.GetFileName(file)); // Dodanie samej nazwy pliku, zamiast całej ścieżki do niego
                    }
                }
                else
                {
                    comboBox1.Items.Add("No files in this folder"); // W razie braku plików w folderze
                }
            }
            else
            {
                comboBox1.Items.Add("Folder does not exist"); //W razie braku istnienia folderu
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string selectedFile = comboBox1.SelectedItem.ToString();
                string fullFilePath = Path.Combine(folderPath, selectedFile);

                if (!File.Exists(fullFilePath))
                {
                    MessageBox.Show($"Błąd: Plik nie istnieje! {fullFilePath}");
                    return;
                }

                if (selectedFile.EndsWith(".gtl") || selectedFile.EndsWith(".btl") || selectedFile.EndsWith(".map"))
                {
                    try
                    {
                        Board.Game.ImportTiles(fullFilePath); 
                        Board.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd wczytywania pliku: {ex.Message}");
                    }
                }
            }
        }
        //tu się kończy dodany kod

        private void Board_Load(object sender, EventArgs e)
        {

        }
    }
}
