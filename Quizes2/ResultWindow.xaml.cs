using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Quizes2
{
    public partial class ResultWindow : Window
    {
        public ResultWindow(string title, string poz, int score, int maxscore, string result)
        {
            InitializeComponent();

            TitleText.Text = "Спасибо за прохождение теста!";
            //ScoreText.Text = $"Баллы: {score}";
            //ResultText.Text = result;

            // передаём значения в куб
            Cube3.Text1 = title;
            Cube3.ScoreText = $"Баллы: {score} из {maxscore}";
            Cube3.ResultText = result;
            Cube3.ImageSide = new BitmapImage(new Uri("Assets/123.png", UriKind.Relative));

            Cube3.CreateCube();
        }
    }
}
