// ─────────────────────────────────────────────────────────────────────────────
// File   : MailSender.cs
// Project: SimpleNetMail (Class Library, .NET Framework 4.8, x86)
// Purpose: PowerBuilder 2019 R3에서 .NET Assembly Import로 바로 호출 가능한 
//          TLS(STARTTLS) 메일 발송 기능 (첨부파일 지원 포함, 포트 25, 인증서 검증 생략)
// Author : 
// Date   : 2025-06-02
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SimpleNetMail
{
    /// <summary>
    /// PowerBuilder 2019 R3에서 Import .NET Assembly 기능으로 생성한 
    /// DotNetObject로 호출할 수 있는 메일 발송 클래스입니다.
    /// SMTP 포트 25를 사용하여 STARTTLS(=TLS) 연결을 수행하고,
    /// 서버 인증서를 검증하지 않도록 설정되어 있습니다.
    /// </summary>
    public class MailSender
    {
        /// <summary>
        /// STARTTLS(=TLS) 연결을 지원하는 SMTP 서버로 메일을 발송합니다.
        /// PowerBuilder에서 string[] attachments 배열을 넘겨 첨부파일을 지정할 수 있습니다.
        /// 인증서 검증은 모두 생략되며, 사설 인증서도 오류 없이 통과합니다.
        /// </summary>
        /// <param name="smtpServer">
        /// SMTP 서버 주소 (예: "smtp.example.com"). 
        /// 포트 25를 사용할 때는 해당 서버가 STARTTLS를 지원해야 합니다.
        /// </param>
        /// <param name="smtpPort">
        /// SMTP 포트. 
        /// 포트 25를 사용할 경우 STARTTLS 핸드셰이크가 수행됩니다 (EnableSsl=true).
        /// 일반적으로 포트 587이나 465 대신, ISP/사내 환경에서 25번 포트를 사용할 때 이 파라미터를 25로 지정합니다.
        /// </param>
        /// <param name="smtpUser">SMTP 인증용 사용자 계정 (예: "user@example.com")</param>
        /// <param name="smtpPass">SMTP 인증용 비밀번호 (앱 전용 비밀번호 권장)</param>
        /// <param name="useTLS">
        /// true로 설정 시 STARTTLS(=TLS) 연결을 시도합니다. 
        /// false로 설정 시 암호화 없이 평문으로 전송되므로 보안상 권장되지 않습니다.
        /// </param>
        /// <param name="from">보내는 사람 이메일 주소 (예: "user@example.com")</param>
        /// <param name="to">
        /// 받는 사람 이메일. 여러 명일 경우 ";" 또는 "," 로 구분 (예: "a@domain.com;b@domain.com").
        /// </param>
        /// <param name="subject">메일 제목 (예: "테스트 메일")</param>
        /// <param name="body">메일 본문 (Plain Text). HTML을 원하면 IsBodyHtml을 true로 바꿔 주세요.</param>
        /// <param name="attachments">
        /// 첨부파일 경로 배열 (PowerBuilder에서 string[] 형태로 만들어서 넘김). 
        /// 배열이 null이거나 길이가 0이면 첨부 없음.
        /// </param>
        /// <returns>성공 시 true, 실패 시 false</returns>
        public bool SendMail(
            string smtpServer,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            bool useTLS,
            string from,
            string to,
            string subject,
            string body,
            string[] attachments
        )
        {
            try
            {
                // ── ❶ TLS 프로토콜 강제 설정 ──────────────────────────────────────────────
                // .NET Framework 기본값에는 SSL3.0, TLS1.0, TLS1.1, TLS1.2 등이 포함될 수 있습니다.
                // 여기서는 TLS1.2 이상만 허용하도록 명시적으로 설정했습니다.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // ── ❷ 인증서 검증 생략(항상 true 반환) ────────────────────────────────────
                // 사설(자체 서명) 인증서로도 연결이 이루어지도록, 
                // 항상 true(검증 통과)만 반환합니다.
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (
                        object sender,
                        X509Certificate certificate,
                        X509Chain chain,
                        SslPolicyErrors sslPolicyErrors
                    )
                    {
                        return true;  // 어떤 인증서이든 모두 허용
                    };
                // ────────────────────────────────────────────────────────────────────────

                // 2) MailMessage 객체 생성 및 기본 설정
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);

                // 3) 받는 사람(To) 설정
                if (!string.IsNullOrWhiteSpace(to))
                {
                    string[] recipients = to
                        .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string addr in recipients)
                    {
                        message.To.Add(addr.Trim());
                    }
                }

                // 4) 제목/본문 설정
                message.Subject = subject ?? string.Empty;
                message.Body = body ?? string.Empty;
                message.IsBodyHtml = false; // HTML 메일 전송을 원하면 true로 변경

                // 5) 첨부파일 처리
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (string path in attachments)
                    {
                        if (string.IsNullOrWhiteSpace(path))
                            continue;

                        if (File.Exists(path))
                        {
                            Attachment attach = new Attachment(path);
                            message.Attachments.Add(attach);
                        }
                        else
                        {
                            // 경로 오류나 파일이 없는 경우 무시
                            // (PB에서 사전에 경로 유효성 검사하는 것을 권장)
                        }
                    }
                }

                // 6) SmtpClient 설정 및 메일 전송
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    // STARTTLS(=TLS) 사용 여부를 지정.
                    // 포트 25, useTLS=true일 때, 서버가 EHLO 후 STARTTLS를 지원하면
                    // 암호화 연결로 전환합니다.
                    client.EnableSsl = useTLS;

                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.Timeout = 300_000; // 타임아웃 5분

                    client.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                // 예외 발생 시 false 반환 (필요하다면 여기서 예외 메시지를 로깅)
                return false;
            }
        }
    }
}
