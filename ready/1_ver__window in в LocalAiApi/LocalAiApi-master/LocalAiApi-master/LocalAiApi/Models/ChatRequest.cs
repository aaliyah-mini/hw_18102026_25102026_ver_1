namespace LocalAiApi.Models
{
    public class ChatRequest 
    {
        public string Message { get; set; }   // текст  
        public string Prompt { get; set; } = "analyse";  // из Dictionary файла PromptProvider.cs 
    }
}
