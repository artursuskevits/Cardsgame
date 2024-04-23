using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Cardsgame2
{
    public partial class MainPage : ContentPage
    {
        private List<PageClass> words;
        private const string FileName = "Words.txt";
        private string FolderPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public MainPage()
        {
            InitializeComponent();
            LoadWords();
            InitializeCarousel();
        }

        private async Task InitializeCarousel()
        {
            var carouselView = new CarouselView
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var stackLayout = new StackLayout
                    {
                        Padding = new Thickness(20),
                        Spacing = 10,
                        BackgroundColor = Color.FromRgb(211, 211, 211),
                        Margin = new Thickness(20),
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    };

                    var wordLabel = new Label
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        FontAttributes = FontAttributes.Bold
                    };
                    wordLabel.SetBinding(Label.TextProperty, "Word");

                    var showButton = new Button
                    {
                        Text = "Näita tõlget",
                        HorizontalOptions = LayoutOptions.CenterAndExpand
                    };
                    var addButton = new Button
                    {
                        Text = "Lisa sõna",
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        BackgroundColor = Color.FromRgb(0, 128, 0),
                        TextColor = Color.FromRgb(255, 255, 255)
                    };
                    var deleteButton = new Button
                    {
                        Text = "Kustta Šõna",
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        BackgroundColor = Color.FromRgb(255, 0, 0),
                        TextColor = Color.FromRgb(255, 255, 255)
                    };

                    var translationLabel = new Label
                    {
                        IsVisible = false,
                        HorizontalOptions = LayoutOptions.CenterAndExpand
                    };
                    translationLabel.SetBinding(Label.TextProperty, "Translate");

                    showButton.Clicked += (sender, e) =>
                    {
                        translationLabel.IsVisible = !translationLabel.IsVisible;
                        showButton.Text = translationLabel.IsVisible ? "Peida Translation" : "Näita tõlget";
                    };
                    addButton.Clicked += async (sender, e) => await AddWord();
                    deleteButton.Clicked += async (sender, e) => await DeleteWord();

                    stackLayout.Children.Add(wordLabel);
                    stackLayout.Children.Add(translationLabel);
                    stackLayout.Children.Add(showButton);
                    stackLayout.Children.Add(addButton);
                    stackLayout.Children.Add(deleteButton);

                    return stackLayout;
                })
            };

            carouselView.ItemsSource = words;

            Content = new ScrollView
            {
                Content = carouselView
            };
        }

        private async Task LoadWords()
        {
            try
            {
                words = await ReadWordsFromFile(FileName);
                if (words.Count == 0)
                {
                    words.Add(new PageClass("Переменная", "Variable"));
                    words.Add(new PageClass("Цикл", "Loop"));
                    words.Add(new PageClass("Функция", "Function"));
                    words.Add(new PageClass("Массив", "Array"));
                    words.Add(new PageClass("Условие", "Condition"));
                    await WriteWordsToFile(words, FileName);
                }

                // Update carousel pages count
                var carouselView = (CarouselView)((ScrollView)Content).Content;
                carouselView.ItemsSource = words;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while loading words: " + ex.Message);
            }
        }

        private async Task<List<PageClass>> ReadWordsFromFile(string fileName)
        {
            List<PageClass> words = new List<PageClass>();

            try
            {
                var filePath = Path.Combine(FolderPath, fileName);
                using (var stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            string[] parts = line.Split(',');
                            if (parts.Length == 2)
                            {
                                words.Add(new PageClass(parts[0].Trim(), parts[1].Trim()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading words from file: " + ex.Message);
            }

            return words;
        }

        private async Task WriteWordsToFile(List<PageClass> words, string fileName)
        {
            try
            {
                var filePath = Path.Combine(FolderPath, fileName);
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (PageClass word in words)
                    {
                        await writer.WriteLineAsync($"{word.Word},{word.Translate}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while writing words to file: " + ex.Message);
            }
        }
        private async Task AddWord()
        {
            try
            {
                string rusword = await DisplayPromptAsync("Lisa sõna", "Sisestage vene sõna ");
                string engword = await DisplayPromptAsync("Lisa sõna", "Sisestage ingliskeelne tõlge ");

                words.Add(new PageClass(rusword, engword));
                await WriteWordsToFile(words, "Words.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while adding word: " + ex.Message);
            }
        }

        private async Task DeleteWord()
        {
            try
            {
                string rusword = await DisplayPromptAsync("Kustuta Word", "Sisestage kustutatav venekeelne sõna");
                var wordToRemove = words.FirstOrDefault(w => w.Word == rusword);
                if (wordToRemove != null)
                {
                    words.Remove(wordToRemove);
                    await WriteWordsToFile(words, "Words.txt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while deleting word: " + ex.Message);
            }
        }

    }
}