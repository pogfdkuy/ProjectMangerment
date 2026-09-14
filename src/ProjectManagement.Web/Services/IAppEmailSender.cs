namespace ProjectManagement.Web.Services;

/// <summary>
/// 給系統一般用途（例如「建立帳號後寄送預設密碼」）寄信用的抽象介面。
/// 跟 ASP.NET Core Identity 內建的 IEmailSender&lt;ApplicationUser&gt;（IdentityNoOpEmailSender）不同，
/// 那個只服務 Identity 內建流程（確認信箱連結／忘記密碼連結／驗證碼），無法直接拿來寄自訂內容的信件。
/// </summary>
public interface IAppEmailSender
{
    /// <summary>寄送一封 HTML 內容的信件。實作應自行處理寄送失敗的例外（呼叫端通常會包 try/catch 避免整個操作失敗）。</summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
