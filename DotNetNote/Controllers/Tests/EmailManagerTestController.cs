using DotNetNote.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNote.Controllers
{
    public class EmailManagerTestController : Controller
    {
        private readonly IEmailManager _emailManager;

        public EmailManagerTestController(IEmailManager emailManager)
        {
            _emailManager = emailManager;
        }

        public IActionResult Index()
        {
            //_emailManager.SendEmailAsync("devlec@outlook.kr", "제목: 메일 전송 테스트", "내용: Test");
            //_emailManager.SendEmailCodeAsync("devlec@outlook.kr", "제목: 메일 전송 테스트", "<hr /><b>내용</b><hr />: Test");

            return View();
        }
    }
}
