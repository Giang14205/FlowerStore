using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FlowerShop.Controllers
{
    public class ChatbotController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public ChatbotController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }
        // Thêm IgnoreAntiforgeryToken để tránh lỗi 400 khi gọi từ JS
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> GetResponse([FromBody] ChatRequest request)
        {
            // 1. Kiểm tra xem client có gửi tin nhắn lên không
            if (request == null || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Vui lòng nhập tin nhắn");
            }

            // 2. Lấy API Key từ appsettings.json
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, "Chưa cấu hình API Key trong server");
            }
            string MODEL_NAME = "gemini-2.5-flash-lite";//
            // 3. Gọi sang Google Gemini
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{MODEL_NAME}:generateContent?key={apiKey}";


            //return Ok(new { reply = "Tôi đang nghe đây" });


            //prompt
            string productInfo = @"
            - Bo Hong Nhung Kim Tuyen: 650000đ/bó
            - Bo Hong Phai Anh Kim: 1000000đ/kg
            - Binh Hoa Cam Tu Cau Mix: 920000đ/bó
            - Bo Hoa Cuoi Tuyet Trang: 900 000đ/bó 
            - Hop Hoa Hong Da Su Trang: 650 000đ/bó 
            - Bo Hoa Hong Vang Phu Quy: 750 000đ/bó 
            - Bo  Hong Do Nu Hoang: 850 000đ/bó 
            - Binh Hoa Vuong Thanh Lich: 550 000đ/bình
            - Bo Hoa Hong Tra Co Dien Vintagate: 990 000đ/bó 
            - Binh Hoa Vu Dieu Mua He: 950 000đ/bó 
            - Bo Hoa Mix Oai Huong Va Hong: 600 000đ/bó 
            - Bo Hoa  Hong Phan Nu Hoang: 600 000đ/bó 








";

            // Ghép vào prompt
            var prompt = $"Bạn là PanPan, nhân viên bán hoa vui tính. " +
                         $"Dưới đây là bảng giá của shop hôm nay:\n{productInfo}\n" +
                         $"Chỉ tư vấn các loại có trong menu. Nếu khách hỏi món không có thì khéo léo từ chối. " +
                         $"Khách hỏi: {request.Message}";

            // Đưa biến prompt vào payload
            var payload = new
            {
                contents = new[]
                {
                 new
                 {
                     parts = new[]
                     {
                     new { text = prompt }
                     }
                 }
                 }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    // Lấy nội dung trả lời từ JSON của Google
                    var reply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return Ok(new { reply = reply?.Replace("\n", "<br>") });
                }

                // Nếu Google trả lỗi
                return StatusCode((int)response.StatusCode, "Lỗi từ Gemini: " + responseString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Server: " + ex.Message);
            }
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
