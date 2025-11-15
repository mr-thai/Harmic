using System.Threading.Tasks;
using Mscc.GenerativeAI;

namespace Harmic.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private GenerativeModel? _model;
        private readonly string _modelId = Model.Gemini25Flash;

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey ?? string.Empty; // do not instantiate model here
        }

        private GenerativeModel GetModel()
        {
            _model ??= new GoogleAI(apiKey: _apiKey).GenerativeModel(model: _modelId);
            return _model;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return string.Empty;

            try
            {
                var response = await GetModel().GenerateContent(prompt);
                return response?.Text ?? string.Empty;
            }
            catch (System.ArgumentException)
            {
                return "API key không hợp lệ. Vui lòng cập nhật API key hợp lệ trong cấu hình.";
            }
            catch
            {
                return "Đã xảy ra lỗi khi gọi AI. Vui lòng thử lại sau.";
            }
        }
    }
}
