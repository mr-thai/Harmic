using Microsoft.AspNetCore.Mvc;
using Harmic.Services;

namespace Harmic.Controllers
{
    [ApiController]
    [Route("ai")]
    public class AiController : Controller
    {
        private readonly GeminiService _gemini;
        private readonly ChatRetrievalService _retrieval;

        public AiController(GeminiService gemini, ChatRetrievalService retrieval)
        {
            _gemini = gemini;
            _retrieval = retrieval;
        }

        // JSON endpoint for the chat widget
        [HttpPost("chat-api")]
        [Produces("application/json")]
        public async Task<IActionResult> ChatApi([FromBody] ChatRequest req, CancellationToken ct)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.Prompt))
                    return BadRequest(new { error = "Prompt is required." });

                // 1) Pull relevant facts from SQL Server
                var facts = await _retrieval.BuildContextAsync(req.Prompt, ct: ct);

                // 2) Compose prompt (avoid replies like “không có quyền truy cập”)
                var prompt = string.IsNullOrWhiteSpace(facts)
                    ? "Bạn là trợ lý cửa hàng. Hãy hỏi lại người dùng những thông tin cần thiết " +
                      "(ví dụ: mã đơn hàng, tên sản phẩm, mức giá mong muốn, danh mục) để tra cứu. " +
                      "Tuyệt đối không nói rằng bạn không có quyền truy cập dữ liệu. " +
                      $"Câu hỏi của người dùng: {req.Prompt}"
                    : "Bạn là trợ lý cửa hàng. Chỉ sử dụng dữ liệu FACTS bên dưới để trả lời ngắn gọn bằng tiếng Việt. " +
                      "Nếu FACTS chưa đủ, hãy hỏi người dùng thông tin còn thiếu (không nói rằng bạn không có quyền truy cập). " +
                      $"{facts}\nCâu hỏi của người dùng: " + req.Prompt;

                // 3) Call LLM
                var answer = await _gemini.GenerateAsync(prompt);
                return Ok(new { response = string.IsNullOrWhiteSpace(answer) ? "Mình cần thêm thông tin để hỗ trợ bạn tốt hơn." : answer });
            }
            catch (Exception ex)
            {
                // Always return JSON so client parsing won't crash
                return StatusCode(500, new { error = "Server error.", detail = ex.Message });
            }
        }

        public sealed class ChatRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }
    }
}
