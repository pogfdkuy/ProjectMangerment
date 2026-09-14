using System.Net;
using System.Net.Mail;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 用 .NET 內建的 System.Net.Mail 透過一般 SMTP 寄信，設定值統一放在 appsettings.json 的 "Smtp" 區段
/// （Host/Port/EnableSsl/UserName/Password/FromAddress/FromDisplayName）。
/// 正式環境建議改用 User Secrets 或環境變數存放 Smtp:Password，不要直接寫進 appsettings.json 進版控（尤其是這個專案已經上傳到 GitHub）。
/// </summary>
public class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IAppEmailSender
{
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var section = configuration.GetSection("Smtp");
        var host = section["Host"];

        // 還沒設定 SMTP 主機時，不要整個操作失敗（例如「建立帳號」），只記一筆警告，
        // 讓管理者事後在 appsettings.json 補上設定即可。
        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("尚未設定 Smtp:Host，略過寄送信件給 {ToEmail}（主旨：{Subject}）。", toEmail, subject);
            return;
        }

        var port = section.GetValue<int?>("Port") ?? 25;
        var enableSsl = section.GetValue<bool?>("EnableSsl") ?? true;
        var userName = section["UserName"];
        var password = section["Password"];
        var fromAddress = section["FromAddress"] ?? userName ?? "noreply@localhost";
        var fromDisplayName = section["FromDisplayName"] ?? "專案管理系統";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };
        if (!string.IsNullOrWhiteSpace(userName))
        {
            client.Credentials = new NetworkCredential(userName, password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromDisplayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}
