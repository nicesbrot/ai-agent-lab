namespace AgentLab.Api.Endpoints.ChatAgent
{
    public class FileService(string faqPath)
    {
        public string LoadFaqContent()
        {
            if (!File.Exists(faqPath))
            {
                throw new FileNotFoundException("FAQ file not found.", faqPath);
            }
            return File.ReadAllText(faqPath);
        }
    }
}
