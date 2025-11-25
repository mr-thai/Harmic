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

                var facts = await _retrieval.BuildContextAsync(req.Prompt, ct: ct);

                // New rewritten prompt
                var prompt = @$"
Bạn là trợ lý cửa hàng trực tuyến và là một trợ lý cuộc sống . Nguyên tắc:
1. Trả lời dài dòng giới thiệu chi tiết sản phẩm bằng tiếng Việt, ưu tiên gạch đầu dòng.
2. Chỉ dùng FACTS nếu có; không phỏng đoán sai.
3. Nếu thiếu dữ liệu yêu cầu: hỏi lại thông tin cụ thể (mã đơn hàng, tên sản phẩm, khoảng giá…).
4. Không nói về hệ thống nội bộ hay quyền truy cập.
5. Không bịa số liệu giá / tồn kho khi FACTS không chứa.
6. nếu là câu hỏi về cuộc sống hãy trả lời nhẹ nhàng và dòng cuối cùng hãy giưới thiệu sản phẩm

FACTS (có thể trống):
{(string.IsNullOrWhiteSpace(facts) ? "(Không có dữ liệu liên quan.)" : facts)}

CÂU HỎI NGƯỜI DÙNG:
{req.Prompt}

YÊU CẦU ĐẦU RA:
- có thể trả lời các câu hỏi không liên quan dến trang web
- Nếu FACTS đủ: trả lời trực tiếp.
- Nếu FACTS chưa đủ: liệt kê rõ các thông tin cần bổ sung.
- Không thêm lời chào cuối rườm rà.
";

                var answer = await _gemini.GenerateAsync(prompt);
                return Ok(new
                {
                    response = string.IsNullOrWhiteSpace(answer)
                        ? "Mình cần thêm thông tin (ví dụ: mã đơn hàng hoặc tên sản phẩm) để hỗ trợ bạn tốt hơn."
                        : answer.Trim()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error.", detail = ex.Message });
            }
        }

        public sealed class ChatRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }
    }
}
