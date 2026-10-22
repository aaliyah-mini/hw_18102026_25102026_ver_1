using LocalAiApi.Models;
using LocalAiApi.Services;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;



namespace LocalAiApi.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {                
        private HttpClient httpClient = new()
        {
            BaseAddress = new Uri("https://localhost:7110/")  // или http://localhost:5156/
        };
                
        public bool isShow = true;
        public double baseWidth = 440; // все окно
        public double findWidth = 100; // свернутое


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
                
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPromptsAsync();
        }


        // загрузка списка промтов
        private async Task LoadPromptsAsync()
        {
            try
            {
                var prompts = await httpClient.GetFromJsonAsync<List<string>>("api/prompts");

                if (prompts != null && prompts.Count > 0)
                {
                    PromptCombo.ItemsSource = prompts;
                    PromptCombo.SelectedIndex = 0;
                }
                else
                {
                    PromptCombo.ItemsSource = new List<string> { "analyse" };
                    PromptCombo.SelectedIndex = 0;
                }
            }
            catch
            {                
                PromptCombo.ItemsSource = new List<string> { "analyse" };
                PromptCombo.SelectedIndex = 0;
            }

        }


        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPromptsAsync();
        }


        // перетаскивание окна  
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }

        
        public void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isShow = !isShow;

            Width = isShow ? baseWidth : findWidth;
            ToggleButton.Content = isShow ? "›" : "‹";

            // если сверунли скрывается форма и показывается полоса 
            ContentPanel.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            StripPanel.Visibility = isShow ? Visibility.Collapsed : Visibility.Visible;
            CopyButton.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;

        }


        // раскрытие при нажатии на полосу 
        private void StripPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isShow)
            {
                ToggleButton_Click(sender, e);
            }
        }

        
        public async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleBox.Text ?? "";
            var content = ContentBox.Text ?? "";
            var prompt = PromptCombo.SelectedItem as string ?? "analyse";

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Введите текст заметки", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            GenerateButton.IsEnabled = false;
            LoadingBar.Visibility = Visibility.Visible;
            ResponseTextBox.Text = "Идет анализ заметки... Ожидайте...";

            try
            {
                var request = new ChatRequest
                {
                    Message = JsonSerializer.Serialize(new
                    {
                        NoteTitle = title,
                        NoteContent = content,
                        NoteType = "Личное"
                    }),
                    Prompt = prompt
                };

                using var response = await httpClient.PostAsJsonAsync("api/chat", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    ResponseTextBox.Text = $"API вернул {(int)response.StatusCode}: {errorText}";

                    return;
                }

                var finalAnswer = await response.Content.ReadAsStringAsync();
                
                finalAnswer = finalAnswer.Trim('"');  // удаляю кавычки 

                ResponseTextBox.Text = string.IsNullOrWhiteSpace(finalAnswer) ? "ИИ не вернула ответ" : finalAnswer;
            }
            catch (Exception ex)
            {
                ResponseTextBox.Text = $"Не удалось получить анализ: {ex.Message}";
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                GenerateButton.IsEnabled = true;
            }

        }


        // копирование ответа 
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ResponseTextBox.Text))
            {
                Clipboard.SetText(ResponseTextBox.Text);
            }

        }


    }
}