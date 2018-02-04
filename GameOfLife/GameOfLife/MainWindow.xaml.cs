using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Collections.ObjectModel;


namespace GameOfLife
{

    public partial class MainWindow : Window
    {
        private const int paneWidth = 80;
        private const int paneHeight = 50;
        private Brush deadBrush = Brushes.LightGray;
        private Brush lifeBrush = Brushes.Turquoise;
        private Rectangle[,] fields = new Rectangle[paneHeight, paneWidth];
        private DispatcherTimer timer = new DispatcherTimer();
        private float interval = 0.7F;
        private const string defaultText = "Name of your pattern";
        private const string patternsPath = @"patterns.txt";
        private List<string> patterns = new List<string>();
        private const int primaryPatternsSize = 8;


        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(interval);
            timer.Tick += Timer_Tick;

            drawingPane.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            drawingPane.Arrange(new Rect(0.0, 0.0, drawingPane.DesiredSize.Width, drawingPane.DesiredSize.Height));

            for (int i = 0; i < paneHeight; i++)
            {
                for (int j = 0; j < paneWidth; j++)
                {

                    Rectangle rectangle = new Rectangle();
                    rectangle.Width = drawingPane.ActualWidth / paneWidth - 0.25;
                    rectangle.Height = drawingPane.ActualHeight / paneHeight - 0.25;
                    rectangle.Fill = deadBrush;
                    drawingPane.Children.Add(rectangle);
                    Canvas.SetLeft(rectangle, j * drawingPane.ActualWidth / paneWidth);
                    Canvas.SetTop(rectangle, i * drawingPane.ActualHeight / paneHeight);
                    rectangle.MouseDown += Rectangle_MouseDown;
                    fields[i, j] = rectangle;

                }
            }

            textBox.Text = defaultText;
            textBox.Foreground = Brushes.DarkGray;
            UpdatePatterns();
        }


        private void ButtonResetClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = ((Rectangle)sender);
            rectangle.Fill = rectangle.Fill == lifeBrush ? deadBrush : lifeBrush;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int[,] numberOfNeighbours = new int[paneHeight, paneWidth];

            bool isEmpty = true;

            for (int i = 0; i < paneHeight; i++)
            {

                for (int j = 0; j < paneWidth; j++)
                {

                    int neighbourCount = 0;

                    updateNeighbours(ref neighbourCount, i, j - 1);
                    updateNeighbours(ref neighbourCount, i - 1, j);
                    updateNeighbours(ref neighbourCount, i - 1, j - 1);
                    updateNeighbours(ref neighbourCount, i + 1, j - 1);
                    updateNeighbours(ref neighbourCount, i - 1, j + 1);
                    updateNeighbours(ref neighbourCount, i, j + 1);
                    updateNeighbours(ref neighbourCount, i + 1, j);
                    updateNeighbours(ref neighbourCount, i + 1, j + 1);

                    if (neighbourCount > 0)
                    {
                        isEmpty = false;
                    }

                    numberOfNeighbours[i, j] = neighbourCount;
                }
            }

            if (isEmpty)
            {
                ResetTimer();
            }

            for (int i = 0; i < paneHeight; i++)
            {

                for (int j = 0; j < paneWidth; j++)
                {
                    if (numberOfNeighbours[i, j] < 2 || numberOfNeighbours[i, j] > 3)
                    {
                        fields[i, j].Fill = deadBrush;
                    }
                    else if (numberOfNeighbours[i, j] == 3)
                    {
                        fields[i, j].Fill = lifeBrush;
                    }
                }
            }
        }


        private void updateNeighbours(ref int neighbourCount, int height, int width)
        {
            if (width >= 0 && width < paneWidth && height >= 0 && height < paneHeight)
            {
                if (fields[height, width].Fill == lifeBrush)
                {
                    neighbourCount++;
                }
            }
        }


        private void Simulate(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                ResetTimer();

            }
            else
            {
                timer.Start();
                simulationButton.Content = "Stop simulating";
            }
        }


        private void Reset()
        {
            for (int i = 0; i < paneHeight; i++)
            {
                for (int j = 0; j < paneWidth; j++)
                {
                    fields[i, j].Fill = deadBrush;
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {

            if (textBox.Text == defaultText)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            if (textBox.Text == string.Empty)
            {
                textBox.Foreground = Brushes.DarkGray;
                textBox.Text = defaultText;
            }
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = textBox.Text + ".txt";
            if (textBox.Text == defaultText)
            {
                MessageBox.Show("Choose a name for your pattern.");
                return;
            }

            if (patterns.Contains(textBox.Text) && patterns.IndexOf(textBox.Text) < primaryPatternsSize)
            {
                MessageBox.Show("Cannot overwrite this pattern");
                return;
            }


            if (!File.Exists(patternsPath))
            {
                new StreamWriter(patternsPath);
            }

            using (StreamWriter writer = File.AppendText(patternsPath))
            {

                if (!File.Exists(@fileName))
                {
                    writer.WriteLine(textBox.Text);
                }

                writer.Close();

            }


            using (StreamWriter writer = new StreamWriter(@fileName))
            {

                for (int i = 0; i < paneHeight; i++)
                {

                    for (int j = 0; j < paneWidth; j++)
                    {
                        int ifAlive = fields[i, j].Fill == lifeBrush ? 1 : 0;
                        writer.Write(ifAlive);
                    }
                    writer.WriteLine();
                }

                writer.Close();

            }

            UpdatePatterns();
        }


        private void UpdatePatterns()
        {
            try
            {
                using (StreamReader reader = new StreamReader(patternsPath))
                {

                    comboBox.Items.Clear();
                    patterns.Clear();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        comboBox.Items.Add(line);
                        patterns.Add(line);
                    }

                    reader.Close();

                }

                if (patterns.Any())
                {
                    comboBox.Text = patterns[0];
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private void ResetTimer()
        {
            timer.Stop();
            simulationButton.Content = "Simulate";
        }

        private void SetPatternButton_Click(object sender, RoutedEventArgs e)
        {

            ResetTimer();
            Reset();
            string fileName = comboBox.Text + ".txt";

            try
            {
                using (StreamReader reader = new StreamReader(@fileName))
                {
                    for (int i = 0; i < paneHeight; i++)
                    {
                        string line = reader.ReadLine();
                        char[] chars = line.ToCharArray();

                        for (int j = 0; j < chars.Length; j++)
                        {
                            fields[i, j].Fill = chars[j] == '1' ? lifeBrush : deadBrush;

                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }


        private void randomPatternButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
            Reset();
            Random random = new Random();

            for (int i = 0; i < slider.Value; i++)
            {

                int width = random.Next(0, paneWidth);
                int height = random.Next(0, paneHeight);

                fields[height, width].Fill = lifeBrush;
            }
        }


        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            string name = comboBox.Text;

            if (patterns.IndexOf(name) < primaryPatternsSize)
            {
                MessageBox.Show("Cannot delete this pattern");
                return;
            }


            try
            {
                patterns.Remove(name);


                using (StreamWriter writer = new StreamWriter(patternsPath))
                {

                    foreach (string line in patterns)
                    {
                        writer.WriteLine(line);
                    }

                    writer.Close();
                }

                string path = name + ".txt";
                File.Delete(path);

                UpdatePatterns();

            }
            catch (Exception e3)
            {
                Console.WriteLine(e3.Message);
            }
        }

        private void Interval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            interval = (float)(10 / slider.Value);
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(interval);
            timer.Start();

        }
    }
}
