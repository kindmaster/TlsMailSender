아래 예시는 Visual Studio 2022에서 “TLS(SSL) 지원 메일 발송 기능”을 갖춘 COM-visible C# Class Library(DLL)를 만들고, 이를 PowerBuilder 2019 R3에서 호출해서 메일(첨부파일 포함)을 보내는 방법을 단계별로 설명합니다.

전체 흐름 요약

Visual Studio 2022에서 .NET Framework 기반의 Class Library(라이브러리) 프로젝트를 생성

C#으로 SendMail 기능(SSL/TLS, 첨부파일 지원)을 구현하고, COM에 노출될 수 있도록 클래스/인터페이스에 ComVisible 등 속성 지정

어셈블리 정보(AssemblyInfo)에 GUID 및 COM 노출 여부 설정

빌드 시 “Register for COM interop” 옵션 활성화 혹은 빌드 후 regasm.exe를 이용해 레지스트리에 등록

PowerBuilder 2019 R3에서 OLEObject로 해당 COM 클래스를 생성·연결한 뒤, 메서드를 호출하여 메일 전송

PowerBuilder 스크립트 예제 제공

다음 단계를 차례대로 진행하세요.

1. Visual Studio 2022에서 “Class Library (.NET Framework)” 프로젝트 만들기
Visual Studio 2022를 실행하고, 메뉴에서 파일 → 새로 만들기 → 프로젝트를 클릭합니다.

템플릿 목록에서 “Class Library (.NET Framework)”를 선택합니다(이때 .NET Framework 4.8 정도를 권장).

프로젝트 이름을 예를 들어 TlsMailSender 로 지정하고, 저장 위치를 원하는 경로로 설정한 뒤 만들기를 클릭합니다.

⚠️ PowerBuilder 2019 R3는 Windows 상에서 동작하는 .NET Framework 기반 COM 오브젝트 호출을 지원하므로, 반드시 “.NET Framework” 타깃으로 프로젝트를 생성해야 합니다. (.NET Core/.NET 5+ 용 Class Library는 COM 레지스트리에 등록되지 않습니다.)

프로젝트가 생성되면, 솔루션 탐색기에서 Properties → AssemblyInfo.cs를 열어 편집할 준비를 합니다.

2. AssemblyInfo.cs 수정 (COM 노출 설정 및 GUID 부여)
Properties\AssemblyInfo.cs 파일을 열고, 다음 사항을 확인/추가합니다.

csharp
복사
편집
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 어셈블리에 대한 일반 정보
[assembly: AssemblyTitle("TlsMailSender")]
[assembly: AssemblyDescription("PowerBuilder용 TLS 지원 메일 발송 COM 라이브러리")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("YourCompany")]
[assembly: AssemblyProduct("TlsMailSender")]
[assembly: AssemblyCopyright("Copyright © YourCompany 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible을 true로 설정하여 COM에 노출
[assembly: ComVisible(true)]

// 고유한 GUID 부여 (아래 GUID는 예시입니다. "생성" 버튼을 눌러 새로운 GUID로 교체해도 됩니다)
[assembly: Guid("d1a2b3c4-5e6f-47a8-b9c0-1234567890ab")]

// 어셈블리 버전
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
ComVisible(true)로 반드시 설정해야 COM 클라이언트(즉 PowerBuilder)에서 클래스에 접근할 수 있습니다.

[Guid("...")] 속성에는 Visual Studio 상단 메뉴 도구 → 설명자(ID) 생성기(<Guid>) 등을 이용해 새로운 GUID를 만들어 넣어도 되고, 위 예시 GUID를 그대로 써도 됩니다.

이 파일을 저장하고 닫습니다.

3. COM 인터페이스 및 클래스 구현
솔루션 탐색기에서 Class1.cs(혹은 기본 생성된 클래스) 파일을 지우고, 새로 MailSender.cs라는 파일을 만듭니다.
이 파일에 다음 코드를 복사해서 붙여넣습니다.

설명:

IMailSender 인터페이스를 ComVisible(true)로 정의

MailSender 클래스를 ClassInterfaceType.None으로 지정한 뒤, IMailSender를 구현

SendMail 메서드엔 SMTP 서버, 포트, 사용자명, 암호, 받는 사람/제목/본문/첨부파일 경로 리스트 등을 파라미터로 받아 System.Net.Mail을 사용해 TLS(SSL) 연결로 메일 송신

csharp
복사
편집
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TlsMailSender
{
    // 1) COM이 볼 수 있도록 인터페이스 정의
    [ComVisible(true)]
    [Guid("E7D3C781-4C2A-4F1B-AB2D-0123456789AB")]  // (GUID는 예시이므로 새로운 GUID로 대체해도 됩니다)
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMailSender
    {
        // SendMail 메서드를 COM에 노출
        [DispId(1)]
        bool SendMail(
            string smtpServer,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            bool useSSL,
            string from,
            string to,
            string subject,
            string body,
            object attachments // VBScript/PowerBuilder에서 COM 호출 시, 배열 형태로 전달 가능 (각각 파일 경로 지정)
        );
    }

    // 2) COM 클래스 구현
    [ComVisible(true)]
    [Guid("B4A2F3D5-9E8C-4DAB-BC01-23456789ABCD")] // (GUID는 예시, 프로젝트별 새 GUID로 교체)
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("TlsMailSender.MailSender")] // PowerBuilder에서 OLEObject로 생성할 때 사용할 ProgID
    public class MailSender : IMailSender
    {
        public bool SendMail(
            string smtpServer,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            bool useSSL,
            string from,
            string to,
            string subject,
            string body,
            object attachments)
        {
            try
            {
                // 1) MailMessage 객체 생성
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);

                // to 파라미터에 쉼표(또는 세미콜론)로 여러 개 주소 지정 가능
                string[] toAddrs = to.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string addr in toAddrs)
                {
                    message.To.Add(addr.Trim());
                }

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false; // 필요에 따라 true로 변경 가능

                // 2) attachments 파라미터 처리
                // PowerBuilder에서 COM 호출 시, attachments를 Array(string)로 전달했다고 가정
                if (attachments != null)
                {
                    // attachments가 SAFEARRAY로 들어온 경우
                    // PowerBuilder: String[] 형태가 COM에서는 object로 들어오며, 아래와 같이 변환 시도
                    try
                    {
                        Array arr = attachments as Array;
                        if (arr != null)
                        {
                            foreach (object item in arr)
                            {
                                string path = item as string;
                                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                                {
                                    Attachment attach = new Attachment(path);
                                    message.Attachments.Add(attach);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 변환 실패 시 무시
                    }
                }

                // 3) SmtpClient 설정
                SmtpClient client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = useSSL;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                // (선택) 메일 전송 전 타임아웃 등 설정 가능
                client.Timeout = 300000; // 5분

                // 4) 메일 전송
                client.Send(message);
                client.Dispose();
                message.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                // 예외 발생 시 false 반환하거나, 필요에 따라 로그 남기기
                // COM에서 에러 메시지를 보고 싶다면 Marshal.ThrowExceptionForHR 등을 이용할 수 있지만,
                // 여기서는 단순히 false를 반환
                return false;
            }
        }
    }
}
코드 설명
IMailSender 인터페이스

ComVisible(true)

InterfaceType(ComInterfaceType.InterfaceIsIDispatch)

SendMail(...) 메서드의 파라미터:

smtpServer (string) : SMTP 서버 주소 (예: “smtp.gmail.com”)

smtpPort (int) : SMTP 포트 (예: 587)

smtpUser (string) : SMTP 인증용 사용자 계정

smtpPass (string) : SMTP 인증용 비밀번호

useSSL (bool) : TLS(SSL) 사용 여부 (true면 EnableSsl=true)

from (string) : 보내는 사람 이메일 주소

to (string) : 받는 사람 이메일 주소. 여러 명일 때는 “a@xxx.com;b@yyy.com”처럼 세미콜론/쉼표로 구분

subject (string) : 메일 제목

body (string) : 메일 본문 (Plain text. 필요 시 IsBodyHtml=true로 수정)

attachments (object) : COM에서는 보통 SAFEARRAY 형태로 전달받으며, 내부에서 Array arr = attachments as Array;로 변환하여 반복 처리

MailSender 클래스

ComVisible(true)

ClassInterface(ClassInterfaceType.None) (명시적 인터페이스 사용)

ProgId("TlsMailSender.MailSender") 지정 → PowerBuilder에서 Create OLEObject시 이 ProgID 사용

SendMail(...) 구현:

MailMessage 객체에 From/To/Subject/Body 설정

attachments 파라미터가 배열로 들어오면 파일 경로별로 Attachment 추가

SmtpClient 객체에 Host/Port/EnableSsl/Credentials 설정 후 Send

컴파일 시 COM에 노출하려면 반드시 어셈블리 전체에 ComVisible(true)가 선언되어 있어야 하고, 클래스에도 ComVisible(true)가 있어야 합니다.

4. 프로젝트 속성 설정 및 빌드
솔루션 탐색기에서 프로젝트(예: TlsMailSender)를 우클릭 → 속성을 선택합니다.

왼쪽 메뉴에서 빌드(Build) 탭을 클릭한 뒤,

플랫폼 대상(Platform target)을 x86 혹은 x64로 지정합니다.

PowerBuilder 2019 R3가 32비트라면 DLL도 x86으로 빌드해야 하고, 64비트 앱이라면 x64로 빌드해야 합니다. (대부분 PB2019 R3는 32비트)

MSBuild 빌드 로거 아래쪽에 COM 상호 운용성(Register for COM interop) 체크박스를 활성화합니다.

이 옵션을 켜면 빌드시 자동으로 레지스트리에 COM 정보가 등록됩니다. (관리자 권한으로 Visual Studio 실행 필요)

어셈블리 정보 버튼을 클릭하여 다시 한번 ComVisible(true)와 GUID가 제대로 지정되었는지 확인합니다.

변경 사항 저장 후, Visual Studio 상단 메뉴의 빌드 → 솔루션 빌드를 클릭하여 컴파일합니다.

빌드가 정상 완료되면, 출력 경로(예: ...\bin\Debug\TlsMailSender.dll)에 COM 등록된 DLL이 생성됩니다.

❗️ 주의1. “Register for COM interop” 옵션을 켤 때는 Visual Studio를 관리자 권한으로 실행해야 레지스트리에 정상 등록됩니다.
❗️ 주의2. 만약 COM 등록이 제대로 되지 않았다면, 빌드 폴더에 생성된 TlsMailSender.dll을 명령 프롬프트(관리자 권한)에서 아래처럼 수동 등록할 수 있습니다.

bat
복사
편집
cd "C:\프로젝트경로\TlsMailSender\bin\Debug"
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe" TlsMailSender.dll /codebase
.NET Framework 경로는 시스템에 설치된 버전에 따라 v4.0.30319 대신 v4.0.30319(W 인스톨 경로) 등을 골라쓰면 됩니다.

/codebase를 붙이면 DLL이 GAC에 설치되지 않아도 COM 활성화됩니다.

5. PowerBuilder 2019 R3에서 DLL 사용하기
이제 PowerBuilder에서 COM 오브젝트를 생성하여 SendMail 메서드를 호출하면 됩니다. 예를 들어, Window나 User Object 스크립트에서 아래 예시와 같이 작성하세요.

5.1. PowerBuilder 스크립트 예제
powerscript
복사
편집
// -----------------------------------------
// 1) 변수 선언
// -----------------------------------------
OLEObject ole_mail
integer li_rc

string ls_smtpServer = "smtp.gmail.com"
integer li_smtpPort   = 587
string ls_smtpUser   = "your_email@gmail.com"
string ls_smtpPass   = "your_app_password_or_smtp_password"

boolean lb_useSSL    = TRUE

string ls_from      = "your_email@gmail.com"
string ls_to        = "recipient1@domain.com;recipient2@domain.com"
string ls_subject   = "테스트 이메일 (PowerBuilder + TLS)"
string ls_body      = "안녕하세요, PowerBuilder에서 COM으로 발송된 TLS 메일 테스트입니다."

// 여러 개 첨부파일을 보낼 경우, ArgumentList 타입의 SafeArray 생성
string ls_attach1 = "C:\TestFolder\file1.txt"
string ls_attach2 = "C:\TestFolder\image2.png"
String[] lsa_attachments
lsa_attachments[1] = ls_attach1
lsa_attachments[2] = ls_attach2

// -----------------------------------------
// 2) OLEObject 생성 및 COM 연결
// -----------------------------------------
ole_mail = CREATE OLEObject

// ProgID는 C# 클래스에서 지정한 대로 "TlsMailSender.MailSender"
li_rc = ole_mail.ConnectToNewObject("TlsMailSender.MailSender")
IF li_rc <> 0 THEN
    MessageBox("오류", "COM 객체 연결에 실패했습니다. 리턴 코드=" + String(li_rc))
    DESTROY ole_mail
    RETURN
END IF

// -----------------------------------------
// 3) SendMail 메서드 호출
// -----------------------------------------
boolean lb_result
lb_result = ole_mail.SendMail( &
    ls_smtpServer, &
    li_smtpPort, &
    ls_smtpUser, &
    ls_smtpPass, &
    lb_useSSL, &
    ls_from, &
    ls_to, &
    ls_subject, &
    ls_body, &
    lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "메일이 성공적으로 전송되었습니다.")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다.")
END IF

// -----------------------------------------
// 4) OLEObject 해제
// -----------------------------------------
ole_mail.DisconnectObject()
DESTROY ole_mail
코드 설명
OLEObject 선언 및 생성

powerscript
복사
편집
OLEObject ole_mail
ole_mail = CREATE OLEObject
COM 객체 연결

powerscript
복사
편집
li_rc = ole_mail.ConnectToNewObject("TlsMailSender.MailSender")
"TlsMailSender.MailSender"는 C# 클래스에 ProgId("TlsMailSender.MailSender")로 지정한 값과 일치해야 합니다.

리턴값 li_rc = 0이면 정상 연결, ≠ 0이면 실패.

첨부파일 배열 생성

PowerBuilder 2019 R3에서는 String[] 타입의 SafeArray를 바로 COM 파라미터로 전달할 수 있습니다.

powerscript
복사
편집
String[] lsa_attachments
lsa_attachments[1] = "C:\TestFolder\file1.txt"
lsa_attachments[2] = "C:\TestFolder\image2.png"
첨부파일이 없을 경우, 네 번째 파라미터에 Null을 넘겨도 되고, 빈 배열(String[] lsa_empty)을 넘겨도 됩니다.

SendMail 호출

powerscript
복사
편집
lb_result = ole_mail.SendMail( &
    ls_smtpServer, &
    li_smtpPort, &
    ls_smtpUser, &
    ls_smtpPass, &
    lb_useSSL, &
    ls_from, &
    ls_to, &
    ls_subject, &
    ls_body, &
    lsa_attachments )
반환값이 TRUE면 전송 성공, FALSE면 실패(예외 혹은 잘못된 파라미터).

해제

powerscript
복사
편집
ole_mail.DisconnectObject()
DESTROY ole_mail
6. 주의사항 및 팁
PowerBuilder 실행 권한

COM 레지스트리에 DLL이 정상 등록되려면, Visual Studio를 관리자 권한으로 실행하여 빌드하거나, regasm.exe 명령어를 관리자 권한으로 실행해야 합니다.

PowerBuilder IDE 자체는 관리자 권한이 아니어도 상관없지만, DLL 등록은 반드시 관리자 권한이 필요합니다.

플랫폼(x86 vs x64) 맞추기

PowerBuilder 2019 R3의 실행 파일(PBD_*.exe)이 32비트라면, COM DLL도 반드시 x86으로 빌드해야 합니다.

솔루션 속성 → 빌드 → 플랫폼 대상에서 “x86”을 선택한 뒤 빌드하세요.

만약 64비트 PowerBuilder 앱(rare)이라면 “x64”로 맞춰야 합니다.

SMTP 인증 방식

위 예제는 NetworkCredential(smtpUser, smtpPass)를 사용한 기본 인증 방식이고, Gmail 같은 경우에는 앱 전용 비밀번호(2단계 인증 설정 후 발급)나 “보안 수준이 낮은 앱 허용” 설정이 필요합니다.

사내 메일서버나 회사에서 제공하는 Exchange 서버의 경우, 서버 주소와 포트, SSL/TLS 사용 여부를 정확히 확인해야 합니다.

HTML 메일

현재 예제는 message.IsBodyHtml = false로 되어 있어 일반 텍스트 본문만 지원합니다.

HTML 형식 메일을 보내려면 message.IsBodyHtml = true;로 변경하면 됩니다.

에러 메시지 확인

예제에서는 예외(예: 네트워크 불가, 인증 실패 등) 발생 시 단순히 false를 반환하도록 구성했습니다.

전송 실패 원인을 PowerBuilder에서 로그나 메시지로 상세히 보고 싶다면, C# 코드 내에서 예외 메시지를 반환하도록 인터페이스를 수정하거나(예: out string errorMessage 파라미터 추가) Marshal.ThrowExceptionForHR 등을 사용해 COM 예외를 발생시키는 방식으로 바꿀 수 있습니다.

첨부파일 경로 유효성

C# 코드에서 File.Exists(path)로 확인 후 첨부파일을 붙이는 로직이므로, PowerBuilder에서 넘긴 경로가 실제로 존재해야 첨부됩니다.

폴더 경로가 잘못되었거나 파일이 잠긴 상태면 첨부가 누락될 수 있습니다.

7. 정리
Visual Studio 2022에서

“Class Library (.NET Framework)” → TlsMailSender 프로젝트 생성

AssemblyInfo.cs의 [ComVisible(true)], [Guid(...)] 설정

MailSender.cs에 IMailSender 인터페이스 및 MailSender 클래스 구현

프로젝트 속성 → 빌드 → Register for COM interop 체크, 플랫폼 대상(x86/x64) 맞춤 → 빌드

(빌드 후 혹은 빌드 시 자동) COM 등록

Visual Studio가 관리자 권한이라면 자동 등록됨

수동 등록할 때:

bat
복사
편집
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe" TlsMailSender.dll /codebase
PowerBuilder 2019 R3에서

OLEObject ole_mail = CREATE OLEObject

ole_mail.ConnectToNewObject("TlsMailSender.MailSender")

ole_mail.SendMail( smtpServer, smtpPort, smtpUser, smtpPass, useSSL, from, to, subject, body, attachments )

ole_mail.DisconnectObject(); DESTROY ole_mail

위 과정을 통해 PowerBuilder 내에서 TLS(SSL) 지원 메일 발송 기능을 가진 COM DLL을 만들어 호출할 수 있습니다. DLL이 정상 등록되었다면, PowerBuilder에서 별도의 추가 셋업 없이 위 예제 코드처럼 OLEObject를 이용하여 SendMail을 바로 호출하여 메일(첨부파일 포함)을 보낼 수 있습니다.







나의 말:
심각도	코드	설명	프로젝트	파일	줄	비표시 오류(Suppression) 상태	세부 정보
오류(활성)	CS0103	'File' 이름이 현재 컨텍스트에 없습니다.	TlsMailSender	D:\Gbox_STY\SendMail\TlsMailSender\TlsMailSender\MailSender.cs	84		 


ChatGPT의 말:
해당 오류 메시지(“CS0103: 'File' 이름이 현재 컨텍스트에 없습니다.”)는 C# 코드에서 File.Exists(path) 등을 호출할 때, System.IO.File 클래스를 찾지 못해서 발생하는 것입니다. 주로 두 가지 이유가 있습니다:

using System.IO; 지시문이 빠져 있는 경우
File 클래스는 System.IO 네임스페이스 안에 정의되어 있으므로, 소스 상단에

csharp
복사
편집
using System.IO;
가 반드시 있어야 합니다. 만약 이 줄이 빠져 있으면 컴파일러는 단순히 File이라는 이름을 인식하지 못합니다.

잘못된 위치에 코드를 삽입했거나 네임스페이스 경로가 어긋난 경우
예를 들어, MailSender 클래스 파일에 using System.IO;를 선언했지만 실제 File.Exists를 호출하는 부분이 다른 네임스페이스 안에 있거나, using 선언보다 위쪽에 코드가 들어가 버리면 동일한 오류가 발생할 수 있습니다.

해결 방법
MailSender.cs 파일 최상단에 using System.IO; 추가 확인

csharp
복사
편집
using System;
using System.IO;               // ← 이 줄이 반드시 있어야 합니다.
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TlsMailSender
{
    // … (나머지 코드)
}
만약 이미 using System.IO; 가 선언되어 있음에도 같은 에러가 난다면, 네임스페이스 블록 바깥에 잘못된 문법이 끼어 있거나, 파일 인코딩/유니코드 문제로 인해 using 구문이 무시되고 있을 가능성을 점검해 보세요.

File 클래스 대신 풀 네임스페이스로 명시하기

어떤 이유로든 using System.IO;를 추가하기 어려운 상황이라면, File.Exists(path) 대신 아래와 같이 작성해도 됩니다.

csharp
복사
편집
if (System.IO.File.Exists(path))
{
    Attachment attach = new Attachment(path);
    message.Attachments.Add(attach);
}
이 경우에는 using System.IO; 없이도 System.IO.File 타입을 직접 참조하므로 오류가 사라집니다.

예시: MailSender.cs 파일 전체 구조 예시
아래는 using System.IO;를 포함하여 수정한 예시입니다. 파일 첫 부분에 네임스페이스 선언들을 모두 집어넣었는지 꼭 확인해 주세요.

csharp
복사
편집
using System;
using System.IO;               // ← File 클래스를 쓰려면 반드시 이 using이 필요합니다.
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TlsMailSender
{
    [ComVisible(true)]
    [Guid("E7D3C781-4C2A-4F1B-AB2D-0123456789AB")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMailSender
    {
        [DispId(1)]
        bool SendMail(
            string smtpServer,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            bool useSSL,
            string from,
            string to,
            string subject,
            string body,
            object attachments
        );
    }

    [ComVisible(true)]
    [Guid("B4A2F3D5-9E8C-4DAB-BC01-23456789ABCD")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("TlsMailSender.MailSender")]
    public class MailSender : IMailSender
    {
        public bool SendMail(
            string smtpServer,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            bool useSSL,
            string from,
            string to,
            string subject,
            string body,
            object attachments)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);

                string[] toAddrs = to.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string addr in toAddrs)
                {
                    message.To.Add(addr.Trim());
                }

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                // attachments가 SAFEARRAY 형태로 들어올 때 참조
                if (attachments != null)
                {
                    Array arr = attachments as Array;
                    if (arr != null)
                    {
                        foreach (object item in arr)
                        {
                            string path = item as string;
                            // System.IO.File.Exists 사용 (using System.IO;가 반드시 필요)
                            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                            {
                                Attachment attach = new Attachment(path);
                                message.Attachments.Add(attach);
                            }
                        }
                    }
                }

                SmtpClient client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = useSSL;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.Timeout = 300000; // 5분

                client.Send(message);
                client.Dispose();
                message.Dispose();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
위 예시처럼 MailSender.cs 파일의 최상단에 using System.IO; 선언이 빠져 있으면 File 클래스를 찾지 못해 CS0103 오류가 납니다. 따라서 에러가 난다면 파일 상단의 using System.IO; 선언을 다시 한 번 확인해 주세요.

요약
원인: File 클래스는 System.IO 네임스페이스에 있으므로, using System.IO;가 없으면 컴파일러가 File을 인식하지 못해 “현재 컨텍스트에 없음” 오류가 발생합니다.

해결:

MailSender.cs 최상단에 using System.IO; 를 추가

또는 File.Exists(...) 대신 System.IO.File.Exists(...) 형태로 풀 네임스페이스를 직접 사용

위 두 가지 방법 중 하나만 적용하면 CS0103 오류가 해결됩니다.






나의 말:

파워빌더 2019 R3 번전은 .Net dll을 공식적으로  import 하여 사용할 수 있게 해주고있어 해당 버전에 적합하게 소스 변경해줘

ChatGPT의 말:

5초 동안 생각함
아래 예제는 “PowerBuilder 2019 R3”의 .NET Assembly Import 기능을 바로 사용할 수 있도록 C# 소스 코드를 수정한 것입니다. COMVisible/C#‐COM 등록 방식 대신, PowerBuilder가 제공하는 “Import .NET Assembly” 마법사를 통해 DLL을 가져와서 DotNetObject로 호출할 수 있는 형태로 만들었습니다.

1. C# 프로젝트 수정: COMVisible 제거 및 메서드 시그니처 변경
Visual Studio 2022에서 기존에 만들었던 TlsMailSender 프로젝트(“Class Library (.NET Framework)” 템플릿)를 엽니다.

MailSender.cs 파일을 아래와 같이 수정합니다.

주요 변경 사항

ComVisible 관련 속성(AssemblyInfo나 클래스 위의 [ComVisible], [Guid], [ProgId] 등)을 모두 제거

SendMail 메서드를 PowerBuilder가 인식하기 쉽도록 public으로 선언하고, 첨부파일은 C#에서 string[] 형태로 받도록 변경

네임스페이스와 클래스 이름은 그대로 두되, COM 특성 없이 “순수 .NET Public 클래스”로 구현

csharp
복사
편집
// MailSender.cs
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace TlsMailSender
{
    // PowerBuilder에서 Import할 때 이 클래스가 노출됩니다.
    public class MailSender
    {
        /// <summary>
        /// PowerBuilder 2019 R3에서 호출 가능한 메서드.
        /// SMTP 서버에 TLS(SSL)로 메일을 보냅니다.
        /// 첨부파일이 있을 경우 string[] attachments 배열로 경로를 넘기면 됩니다.
        /// </summary>
        /// <param name="smtpServer">예: "smtp.gmail.com"</param>
        /// <param name="smtpPort">예: 587</param>
        /// <param name="smtpUser">SMTP 로그인 ID (예: your_email@gmail.com)</param>
        /// <param name="smtpPass">SMTP 비밀번호 또는 앱 전용 비밀번호</param>
        /// <param name="useSSL">true면 TLS/SSL 사용</param>
        /// <param name="from">보내는 사람 이메일 주소</param>
        /// <param name="to">받는 사람 이메일(여러 명일 경우 ";" 또는 ","로 구분)</param>
        /// <param name="subject">메일 제목</param>
        /// <param name="body">메일 본문 (텍스트)</param>
        /// <param name="attachments">첨부파일 경로 배열 (없으면 null 또는 빈 배열)</param>
        /// <returns>true(성공) / false(실패)</returns>
        public bool SendMail(
            string smtpServer,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            bool useSSL,
            string from,
            string to,
            string subject,
            string body,
            string[] attachments
        )
        {
            try
            {
                // 1) MailMessage 생성
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);

                // 2) 받는 사람(To) 설정 (여러 명일 경우 ";" 또는 "," 구분)
                if (!string.IsNullOrWhiteSpace(to))
                {
                    string[] recipients = to.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string addr in recipients)
                    {
                        message.To.Add(addr.Trim());
                    }
                }

                message.Subject = subject ?? string.Empty;
                message.Body = body ?? string.Empty;
                message.IsBodyHtml = false; // 필요 시 true로 변경 가능

                // 3) attachments 배열이 넘어오면 파일 경로별 첨부
                if (attachments != null)
                {
                    foreach (string path in attachments)
                    {
                        if (string.IsNullOrWhiteSpace(path)) continue;
                        if (File.Exists(path))
                        {
                            Attachment attach = new Attachment(path);
                            message.Attachments.Add(attach);
                        }
                        // 파일이 없으면 그냥 무시
                    }
                }

                // 4) SmtpClient 설정 후 전송
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = useSSL;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.Timeout = 300000; // 5분 (필요 시 조정)

                    client.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                // 실제 운용 시 예외 메시지를 로그로 남기거나, out 파라미터를 추가해서 PB로 던져줄 수도 있습니다.
                return false;
            }
        }
    }
}
이제 AssemblyInfo.cs에서도 [ComVisible], [Guid], [ProgId] 등 COM 관련 어트리뷰트는 모두 제거해 줍니다.

즉, Properties\AssemblyInfo.cs 에서 using System.Runtime.InteropServices;와 관련된 모든 [assembly: ComVisible(...)], [assembly: Guid(...)] 줄을 삭제하거나 주석 처리합니다.

남아 있는 것은 일반적인 AssemblyVersion, AssemblyTitle 정도만 있으면 충분합니다.

프로젝트 속성(Properties)에서

타깃 프레임워크를 .NET Framework 4.8 (혹은 PowerBuilder가 지원하는 가장 높은 .NET Framework 버전)으로 맞추고

플랫폼 대상 (Platform target) 은 PowerBuilder 실행 환경에 따라 x86(대부분 PB2019 R3는 32비트) 혹은 x64로 지정

“Register for COM interop” 옵션은 꺼둡니다(이제 COM 방식이 아니므로 불필요).

코드를 저장한 후 빌드 → 솔루션 빌드를 눌러 DLL을 생성합니다.

출력 폴더(예: bin\Debug\TlsMailSender.dll)에 DLL이 생성된 것을 확인합니다.

2. PowerBuilder 2019 R3에서 “.NET DLL Import” 하기
PowerBuilder 2019 R3에는 .NET Assembly Import 마법사가 내장되어 있어, 위에서 만든 DLL을 바로 가져와서 DotNetObject로 사용할 수 있습니다. 아래 절차를 따라 주세요.

PowerBuilder 2019 R3 IDE를 실행한 뒤, 해당 애플리케이션(PBL/PBT)을 연 상태에서
System Tree → Assemblies(또는 메뉴에서 Project → Import .NET Assembly...)를 선택합니다.

.NET DLL Import 창이 뜹니다.

Source .NET DLL: 방금 빌드된 TlsMailSender.dll 파일 경로를 지정

Framework Type: “.NET Framework”를 선택

Destination PBT: import된 proxy 클래스를 저장할 PBT 파일(예: MyProjectDotNet.pbt)을 선택

Destination PBL: 원하는 PBL(예: MyProjectPB.pbl)을 지정하여 PB 오브젝트(Proxy)를 생성하도록 합니다.

Select and Preview 영역에 “TlsMailSender.dll” 아래에 노출 가능한 Public 클래스(예: TlsMailSender.MailSender)가 보입니다.

클래스 왼쪽 체크박스(checkbox)를 체크하여 선택

오른쪽 Preview 창에 PB에서 생성될 Proxy 정보가 간략히 나타납니다.

Import 버튼을 누르면, PowerBuilder가 자동으로

선택한 .NET Assembly 내의 공개(public) 클래스를 PB용 Proxy 오브젝트(예: n_tlsmailsender_mailsender)를 생성

Proxy 오브젝트는 PBL 내부에 생성된 후 “DotNetObject”처럼 사용 가능

Import가 완료된 후, System Tree → Assemblies 아래에 TlsMailSender.MailSender 클래스가 Proxy 형태로 생성된 것을 확인할 수 있습니다.

생성된 Proxy 객체 이름은 자동으로 n_tlsmailsender_mailsender(규칙: n_어셈블리이름_클래스이름) 형태를 사용합니다.

3. PowerBuilder 스크립트에서 호출 예제
.NET Assembly Import를 통해 생성된 Proxy 클래스(n_tlsmailsender_mailsender)를 PowerBuilder 코드에서 DotNetObject로 선언하고 인스턴스를 만들어 메서드를 호출합니다. 예를 들어, PB의 윈도우나 유저 오브젝트 스크립트에 아래와 같이 작성하면 됩니다.

powerscript
복사
편집
// =================================================================================
// 1) DotNetObject 인스턴스 선언
// =================================================================================
DotNetObject    dn_mailer
boolean         lb_result

string          ls_smtpServer = "smtp.gmail.com"
integer         li_smtpPort   = 587
string          ls_smtpUser   = "your_email@gmail.com"
string          ls_smtpPass   = "your_app_password_or_smtp_password"
boolean         lb_useSSL     = TRUE

string          ls_from       = "your_email@gmail.com"
string          ls_to         = "recipient1@domain.com;recipient2@domain.com"
string          ls_subject    = "테스트 이메일 (.NET Assembly Import)"
string          ls_body       = "PowerBuilder 2019 R3에서 .NET DLL Import를 사용한 메일 발송 테스트입니다."

// 여러 개 첨부파일을 보내려면 string 배열로 지정
string[]        lsa_attachments
lsa_attachments[1] = "C:\TestFolder\file1.txt"
lsa_attachments[2] = "C:\TestFolder\image2.png"
// 첨부파일이 없으면 아래처럼 빈 배열 또는 Null
// string[] lsa_attachments  // 이 경우 메서드 호출 시 그냥 Null이 전달됩니다.

// =================================================================================
// 2) DotNetObject 생성 및 .NET 클래스 인스턴스화
// =================================================================================
// Proxy 객체 이름은 Import 시 생성된 이름을 사용 (예: n_tlsmailsender_mailsender)
dn_mailer = CREATE DotNetObject("n_tlsmailsender_mailsender")
IF IsNull(dn_mailer) THEN 
    MessageBox("오류", ".NET 객체 인스턴스 생성 실패")
    RETURN
END IF

// =================================================================================
// 3) SendMail 메서드 호출
// =================================================================================
// SendMail(string smtpServer, int smtpPort, string smtpUser, string smtpPass,
//          bool useSSL, string from, string to, string subject, string body, string[] attachments)
lb_result = dn_mailer.SendMail( &
                ls_smtpServer, &
                li_smtpPort, &
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useSSL, &
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "메일 전송에 성공했습니다.")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다.")
END IF

// =================================================================================
// 4) 닷넷 객체 해제
// =================================================================================
dn_mailer = NULL
코드 설명
DotNetObject 생성

powerscript
복사
편집
dn_mailer = CREATE DotNetObject("n_tlsmailsender_mailsender")
n_tlsmailsender_mailsender는 Import된 Proxy 객체 이름입니다.

이 이름은 System Tree → Assemblies 아래에서 실제 생성된 오브젝트(Proxy) 이름을 확인해서 정확하게 입력해야 합니다.

string[] attachments

PowerBuilder 2019 R3부터는 string[] 타입(1-based SafeArray) 파라미터가 그대로 .NET 메서드의 string[]로 매핑됩니다.

첨부파일이 없으면 lsa_attachments를 선언만 하고 아무 값도 넣지 않거나, Null을 넘기면 됩니다.

메서드 호출

powerscript
복사
편집
lb_result = dn_mailer.SendMail( ..., lsa_attachments )
순서와 데이터 타입이 C# 쪽 public bool SendMail(string, int, string, string, bool, string, string, string, string, string[]) 과 정확히 일치해야 합니다.

PB가 내부적으로 string, integer, boolean, string[]를 .NET 쪽 매개변수로 올바르게 매핑해 줍니다.

해제

powerscript
복사
편집
dn_mailer = NULL
PB에서는 DotNetObject를 사용한 뒤 = NULL로 설정하면 가비지 컬렉션 대상이 됩니다.

4. 전체 요약
C# 소스 변경 요점

COMVisible/GUID 등 COM 관련 어트리뷰트 제거

public class MailSender { public bool SendMail(string smtpServer, int smtpPort, string smtpUser, string smtpPass, bool useSSL, string from, string to, string subject, string body, string[] attachments) { … } } 형태로 구현

using System.IO;, using System.Net.Mail; 등을 추가하여 File.Exists와 MailMessage/SmtpClient 기능 사용

플랫폼 대상(x86/x64)과 .NET Framework 버전을 PowerBuilder 환경에 맞춰 릴리스 빌드 수행

PowerBuilder 2019 R3

Project → Import .NET Assembly 창에서 TlsMailSender.dll 가져오기

Proxy 클래스 이름(예: n_tlsmailsender_mailsender)을 확인하고, PB 스크립트에서 CREATE DotNetObject("n_tlsmailsender_mailsender") 로 인스턴스 생성

SendMail(...) 메서드를 호출할 때, C#에서 정의한 순서와 타입(문자열, 정수, 불리언, 문자열 배열)을 일치시켜 전달

첨부파일 사용 시 주의

string[] attachments 배열에 들어가는 경로가 실제로 존재해야 메일에 첨부됨

PB에서 배열을 생성할 때 반드시 1-based 인덱스를 사용 (예시: lsa_attachments[1] = "C:\file1.txt")

이렇게 하면 PowerBuilder 2019 R3 내장 “.NET DLL Import” 기능을 통해 별도의 COM 등록 없이도 곧바로 메일 발송 기능(SSL/TLS + 첨부파일)을 호출할 수 있습니다.






나의 말:
 아래 링크 참고해서 소스코드 다시 작성해줘
https://github.com/yuseok-kim-edushare/simple-.net-Crypting-For-PowerBuilder


ChatGPT의 말:

36초 동안 생각함
아래는 “simple-.NET-Crypting-For-PowerBuilder” 프로젝트에서 사용하는 패턴을 참고하여, PowerBuilder 2019 R3의 “.NET Assembly Import” 기능으로 바로 불러서 사용할 수 있도록 작성한 C# 소스 코드 예제입니다.

Target Framework: .NET Framework 4.8

플랫폼 대상(Platform target): x86 (PowerBuilder 2019 R3가 32비트이기 때문에)

프로젝트 타입: Class Library (.NET Framework)

이 코드는 컴파일만 하면 PowerBuilder에서 바로 Import .NET Assembly… 기능으로 가져다 쓸 수 있도록 순수한 .NET 공개(public) 클래스로만 구성되어 있습니다. COMVisible, Guid, ProgId 등 COM 관련 특성(Attribute)은 모두 제거했습니다.

1. MailSender.cs
csharp
복사
편집
// ─────────────────────────────────────────────────────────────────────────────
// File   : MailSender.cs
// Project: SimpleNetMail (Class Library, .NET Framework 4.8, x86)
// Purpose: PowerBuilder 2019 R3에서 .NET Assembly Import로 바로 호출 가능한 
//          TLS(SSL) 메일 발송 기능 (첨부파일 지원 포함)
// Author : (작성자 이름, 예: 홍길동)
// Date   : 2025-06-xx
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SimpleNetMail
{
    /// <summary>
    /// PowerBuilder 2019 R3에서 Import .NET Assembly 기능으로 생성한 
    /// DotNetObject로 호출할 수 있는 메일 발송 클래스입니다.
    /// </summary>
    public class MailSender
    {
        /// <summary>
        /// TLS(SSL) 지원 SMTP로 메일을 발송합니다.
        /// PowerBuilder에서 string[] attachments 배열을 넘겨서 첨부파일을 지정할 수 있습니다.
        /// </summary>
        /// <param name="smtpServer">SMTP 서버 주소 (예: "smtp.gmail.com")</param>
        /// <param name="smtpPort">SMTP 포트 (예: 587 혹은 465)</param>
        /// <param name="smtpUser">SMTP 인증용 사용자 계정 (예: "user@gmail.com")</param>
        /// <param name="smtpPass">SMTP 인증용 비밀번호 (앱 전용 비밀번호 권장)</param>
        /// <param name="useSSL">TLS/SSL 암호화 사용 여부 (true/false)</param>
        /// <param name="from">보내는 사람 이메일 주소 (예: "user@gmail.com")</param>
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
            bool useSSL,
            string from,
            string to,
            string subject,
            string body,
            string[] attachments
        )
        {
            try
            {
                // 1) MailMessage 객체 준비
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);

                // 2) 받는 사람(To) 설정
                if (!string.IsNullOrWhiteSpace(to))
                {
                    // ';' 또는 ',' 로 구분하여 다중 수신자 추가
                    string[] recipients = to
                        .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string addr in recipients)
                    {
                        message.To.Add(addr.Trim());
                    }
                }

                // 3) 제목/본문 설정
                message.Subject = subject ?? string.Empty;
                message.Body = body ?? string.Empty;
                message.IsBodyHtml = false; // 필요 시 true로 변경 가능

                // 4) 첨부파일 처리
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (string path in attachments)
                    {
                        if (string.IsNullOrWhiteSpace(path))
                            continue;

                        // 실제 파일이 존재할 때만 첨부
                        if (File.Exists(path))
                        {
                            Attachment attach = new Attachment(path);
                            message.Attachments.Add(attach);
                        }
                        else
                        {
                            // 파일이 없으면 그냥 무시하거나 로그를 남길 수 있음
                            // (PB 단에서 경로 유효성 검사를 미리 해두면 좋습니다)
                        }
                    }
                }

                // 5) SmtpClient 설정 및 메일 전송
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = useSSL;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.Timeout = 300_000; // 5분 (필요시 조정)

                    client.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                // 예외가 난 경우 false 반환 (원인 파악용 로그는 필요하면 추가)
                return false;
            }
        }
    }
}
2. AssemblyInfo.cs
Visual Studio 2022로 생성된 기본 AssemblyInfo.cs (또는 ProjectName.csproj 내부에 자동 생성되는 어셈블리 메타데이터)에서 COM 관련 속성은 모두 삭제하거나 기본값으로 두면 됩니다.
아래는 최소한으로 남겨 두어도 무방한 예시입니다.

csharp
복사
편집
using System.Reflection;
using System.Runtime.CompilerServices;

// ※ 아래 두 줄을 **절대** 삭제하면 안 됩니다.
//    프로젝트 정보(버전/제목 등)는 자유롭게 수정하세요.
[assembly: AssemblyTitle("SimpleNetMail")]
[assembly: AssemblyDescription("PowerBuilder용 TLS 메일 발송 .NET Class Library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("YourCompanyName")]
[assembly: AssemblyProduct("SimpleNetMail")]
[assembly: AssemblyCopyright("Copyright © YourCompany 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// COM 과 관련된 설정은 모두 제거하거나 false로 변경
// [assembly: ComVisible(true)]    ← 삭제 또는 false로 변경
// [assembly: Guid("...")]        ← 삭제

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
중요: COMVisible, Guid, ProgId 등의 COM 관련 어트리뷰트는 전부 제거해야 합니다.

PowerBuilder 2019 R3의 “.NET Assembly Import” 기능은 순수 public 클래스와 public 메서드만 필요로 하며, COM 등록은 절대로 필요하지 않습니다.

3. Visual Studio 프로젝트 설정
Visual Studio 2022에서 File → New → Project를 선택한 뒤,

템플릿: “Class Library (.NET Framework)”

프로젝트 이름: SimpleNetMail (예시)

타깃 프레임워크: .NET Framework 4.8

Platform target: x86 (PowerBuilder 2019 R3가 32비트이므로)

프로젝트 속성창(프로젝트 우클릭 → Properties)에서

Build 탭 → Platform target 을 x86 으로 설정

“Register for COM interop” 옵션은 비활성화 (불필요)

MailSender.cs 파일을 추가하고, 위 코드 전체를 붙여넣기

Properties → AssemblyInfo.cs (또는 .csproj 안에 자동 생성되는 어셈블리 정보)에서 COM 관련 어트리뷰트 제거

솔루션을 빌드(Build → Build Solution) 하면,

출력 폴더(예: bin\Debug\SimpleNetMail.dll)에 DLL이 생성됩니다.

해당 DLL을 곧바로 PowerBuilder에서 Import 가능합니다.

4. PowerBuilder 2019 R3에서 DLL Import 및 사용 방법
다음 절차에 따라 PowerBuilder 2019 R3에서 “SimpleNetMail.dll”을 Import하고, 메일 발송 기능을 호출합니다.

PowerBuilder IDE를 관리자 권한이 아닌 일반 모드로 실행

상단 메뉴에서 Project → Import .NET Assembly… 클릭

“.NET DLL Import” 대화상자에서:

Source .NET DLL: 방금 빌드된 SimpleNetMail.dll 경로 지정

Framework Type: “.NET Framework” 선택

Destination PBT: Proxy 클래스를 생성할 PBT 파일(예: dotnet_import.pbt) 선택

Destination PBL: Proxy 오브젝트가 들어갈 PBL(예: dotnet_proxy.pbl) 선택

화면 하단의 “Select and Preview” 리스트에서

SimpleNetMail.dll 아래에 표시된 SimpleNetMail.MailSender 클래스를 체크

오른쪽 “Preview Result” 영역에 Proxy 생성 결과 미리보기가 나타남

Import 버튼을 누르면,

PB 내부에 n_simplenetmail_mailsender(예시) 같은 이름으로 Proxy 오브젝트가 생성됨

Proxy 이름 규칙: n_<어셈블리이름>_<클래스이름> (모두 소문자)

System Tree → Assemblies 아래에 SimpleNetMail.MailSender 항목이 생김

PowerBuilder 코드(윈도우나 NVO 스크립트 등)에서 아래 예시처럼 호출

powerscript
복사
편집
// -------------------------------------------------
// 1) DotNetObject 변수 선언
// -------------------------------------------------
DotNetObject    dn_mailer
boolean         lb_result

// SMTP 설정 (예시: Gmail)
string          ls_smtpServer = "smtp.gmail.com"
integer         li_smtpPort   = 587
string          ls_smtpUser   = "your_email@gmail.com"
string          ls_smtpPass   = "your_app_password_or_smtp_password"
boolean         lb_useSSL     = TRUE

// 메일 정보
string          ls_from       = "your_email@gmail.com"
string          ls_to         = "recipient1@domain.com;recipient2@domain.com"
string          ls_subject    = "테스트 메일 (.NET Import)"
string          ls_body       = "PowerBuilder 2019 R3에서 .NET Assembly Import로 보낸 메일 테스트입니다."

// 첨부파일이 여러 개라면 string[] 배열로 지정
string[]        lsa_attachments
lsa_attachments[1] = "C:\TestFolder\file1.txt"
lsa_attachments[2] = "C:\TestFolder\image2.jpg"

// -------------------------------------------------
// 2) DotNetObject 인스턴스 생성
// -------------------------------------------------
// Import 시 생성된 Proxy 이름을 정확히 입력해야 합니다.
// 예: n_simplenetmail_mailsender (System Tree → Assemblies → 확인)
dn_mailer = CREATE DotNetObject("n_simplenetmail_mailsender")
IF IsNull(dn_mailer) THEN
    MessageBox("오류", ".NET 객체를 생성할 수 없습니다.")
    RETURN
END IF

// -------------------------------------------------
// 3) SendMail 메서드 호출
// -------------------------------------------------
// 메서드 시그니처: 
// public bool SendMail(string smtpServer, int smtpPort, string smtpUser, 
//                      string smtpPass, bool useSSL, string from, 
//                      string to, string subject, string body, string[] attachments)
lb_result = dn_mailer.SendMail( &
                ls_smtpServer, &
                li_smtpPort, &
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useSSL, &
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "메일이 성공적으로 전송되었습니다.")
ELSE
    MessageBox("실패", "메일 전송에 실패했습니다.")
END IF

// -------------------------------------------------
// 4) 닷넷 객체 정리
// -------------------------------------------------
dn_mailer = NULL
주요 포인트
Proxy 클래스 이름 확인

Import 단계에서 생성된 Proxy 이름이 반드시 "n_simplenetmail_mailsender" 형태인지 확인하세요.

System Tree → Assemblies → “SimpleNetMail.MailSender” 부분을 우클릭 → Properties에서 Proxy 이름을 볼 수 있습니다.

string[] attachments 파라미터

PowerBuilder 2019 R3부터는 string[] 타입(1-based SafeArray)을 .NET의 string[] 파라미터에 자동 매핑합니다.

첨부파일이 없으면 lsa_attachments를 단순히 선언만 하고 값을 넣지 않으면 Null로 전달됩니다.

빈 배열을 넘기고 싶다면, string[] empty_attach 를 선언 후 empty_attach = CREATE String[] 식으로 빈 배열을 만들어 넘겨도 됩니다.

플랫폼(x86) 일치 여부

PowerBuilder 2019 R3는 32비트 프로세스이므로, Visual Studio에서 x86으로 빌드해야 PB가 오류 없이 DLL을 로드합니다.

64비트 PB(드물게 사용)인 경우에는 x64로 빌드 후 Import해야 합니다.

SSL/TLS 인증

Gmail 등을 사용할 때는 “앱 전용 비밀번호”(2단계 인증 설정 후 발급) 또는 “보안 수준이 낮은 앱 허용”을 설정해야 정상 전송됩니다.

사내 메일서버(Exchange, Office 365 등)인 경우, 서버 주소/포트/SSL 여부를 정확히 확인하세요.

HTML 메일

기본적으로 message.IsBodyHtml = false로 되어 있어 Plain Text만 지원합니다.

HTML 본문을 보내려면 message.IsBodyHtml = true; 로 변경하면 됩니다.

5. 정리 및 요약
C# 프로젝트

Class Library(.NET Framework 4.8, x86)

MailSender.cs 안에 순수 public 클래스/메서드(public class MailSender, public bool SendMail(...)) 구현

COMVisible/GUID/ProgId 등 COM 특성 삭제

AssemblyInfo에 COM 관련 속성 제거

빌드

Visual Studio 2022에서 관리자 권한이 아닌 일반 모드로 빌드(“Register for COM interop” 끔)

출력 폴더(bin\Debug\SimpleNetMail.dll)에 DLL 생성

PowerBuilder 2019 R3

Project → Import .NET Assembly…

SimpleNetMail.dll import → Proxy (n_simplenetmail_mailsender) 생성

PB 스크립트에서 CREATE DotNetObject("n_simplenetmail_mailsender") → SendMail(...) 호출

이 과정을 통해 PowerBuilder 2019 R3 환경에서 “TLS(SSL) 지원 메일 발송 기능(첨부파일 포함)”을 간단하게 .NET DLL Import로 구현할 수 있습니다.







출처
나의 말:
해당 소스는 SSL 설정이 되는것 같아 TLS 설정으로 변경해줘


ChatGPT의 말:

몇 초 동안 생각함
아래는 “SimpleNetMail” 프로젝트의 MailSender.cs 파일을 수정하여, 기존의 SSL 설정 대신 명시적으로 TLS(예: TLS1.2)를 사용하도록 변경한 예제입니다.

.NET Framework 환경에서는 SmtpClient.EnableSsl = true; 를 설정하면 STARTTLS(즉, TLS) 연결이 시도되지만, 만약 SSL 프로토콜(SSL3 등)이 아닌 구체적인 TLS 버전(예: TLS1.2)으로 고정하고 싶다면, 호출 전에 ServicePointManager.SecurityProtocol을 명시적으로 설정해 주면 됩니다. 아래 예제에서는 SecurityProtocolType.Tls12 을 우선 적용하도록 구현했습니다.

수정된 MailSender.cs
csharp
복사
편집
// ─────────────────────────────────────────────────────────────────────────────
// File   : MailSender.cs
// Project: SimpleNetMail (Class Library, .NET Framework 4.8, x86)
// Purpose: PowerBuilder 2019 R3에서 .NET Assembly Import로 바로 호출 가능한 
//          TLS(SSL 아님, STARTTLS) 메일 발송 기능 (첨부파일 지원 포함)
// Author : (작성자 이름, 예: 홍길동)
// Date   : 2025-06-xx
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
    /// (SSL이 아닌 TLS1.2 이상 연결을 강제하도록 구성)
    /// </summary>
    public class MailSender
    {
        /// <summary>
        /// TLS(STARTTLS) 지원 SMTP로 메일을 발송합니다.
        /// PowerBuilder에서 string[] attachments 배열을 넘겨서 첨부파일을 지정할 수 있습니다.
        /// </summary>
        /// <param name="smtpServer">SMTP 서버 주소 (예: "smtp.gmail.com")</param>
        /// <param name="smtpPort">SMTP 포트 (예: 587)</param>
        /// <param name="smtpUser">SMTP 인증용 사용자 계정 (예: "user@gmail.com")</param>
        /// <param name="smtpPass">SMTP 인증용 비밀번호 (앱 전용 비밀번호 권장)</param>
        /// <param name="useTLS">
        /// true로 설정할 경우 STARTTLS(=TLS) 연결을 시도. 
        /// false로 설정하면 암호화 없이 평문 전송(권장하지 않음).
        /// </param>
        /// <param name="from">보내는 사람 이메일 주소 (예: "user@gmail.com")</param>
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
                // .NET Framework에서 ServicePointManager.SecurityProtocol을 통해
                // 허용할 프로토콜을 명시적으로 설정할 수 있습니다.
                // 여기서는 TLS1.2 이상만 허용하도록 지정합니다.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // (선택) 인증서 검증 콜백: 필요시 허용하지 않을 인증서를 걸러낼 수 있음.
                // 예를 들어, 자체 서명된 인증서 등을 허용하려면 아래 콜백을 주석 처리하거나 수정.
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        // true를 반환하면 모든 인증서를 허용 (테스트용).
                        // 실환경에서는 sslPolicyErrors를 확인하여 허용/거부 로직 추가 권장.
                        return sslPolicyErrors == SslPolicyErrors.None;
                    };
                // ────────────────────────────────────────────────────────────────────────

                // 2) MailMessage 객체 준비
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);

                // 3) 받는 사람(To) 설정
                if (!string.IsNullOrWhiteSpace(to))
                {
                    // ';' 또는 ',' 로 구분하여 다중 수신자 추가
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
                message.IsBodyHtml = false; // 필요 시 true로 변경 가능

                // 5) 첨부파일 처리
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (string path in attachments)
                    {
                        if (string.IsNullOrWhiteSpace(path))
                            continue;

                        // 실제 파일이 존재할 때만 첨부
                        if (File.Exists(path))
                        {
                            Attachment attach = new Attachment(path);
                            message.Attachments.Add(attach);
                        }
                        else
                        {
                            // 파일이 없으면 무시하거나 로그 남기기
                            // (PB 단에서 경로 유효성 검사를 미리 해두면 좋습니다)
                        }
                    }
                }

                // 6) SmtpClient 설정 및 메일 전송
                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    // STARTTLS(=TLS) 사용 여부 설정
                    client.EnableSsl = useTLS;

                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.Timeout = 300_000; // 5분 (필요 시 조정)

                    client.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                // 예외가 발생한 경우 false 반환 (필요시 예외 내용을 로그에 기록)
                return false;
            }
        }
    }
}
주요 수정 사항 요약
useSSL → useTLS로 파라미터명 변경

원래 EnableSsl이라고 명명된 파라미터를 useTLS로 바꾸어, “이 메서드는 TLS 연결(STARTTLS)을 사용합니다”라는 의도를 명확히 했습니다.

만약 과거에 PB 코드에서 true/false 매핑을 그대로 사용하고 싶다면 PB 쪽 호출부에서 변수 이름만 바꿔 주면 됩니다.

ServicePointManager.SecurityProtocol 설정 추가

csharp
복사
편집
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
.NET Framework 기본값으로는 SSL 3.0, TLS 1.0, 1.1, 1.2가 모두 허용된 상태일 수 있으므로, 특정 서버가 TLS1.2 이상만 허용할 때 이를 강제할 수 있습니다.

필요 시 SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 와 같이 복수 버전을 OR 연산으로 지정할 수도 있습니다.

서버 인증서 검증 콜백 (선택 사항)

csharp
복사
편집
ServicePointManager.ServerCertificateValidationCallback =
    delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None;
    };
기본적으로 “인증서 오류가 없을 때만 연결 허용” 로직을 넣었습니다.

테스트 환경에서 자체 서명된(비공인) 인증서를 허용해야 한다면 return true; 만 남겨도 무방하지만, 실제 운용 환경에서는 반드시 sslPolicyErrors == None 인 경우에만 허용하도록 권장합니다.

client.EnableSsl = useTLS;

이 설정이 true일 경우 STARTTLS(=TLS) 연결을 시도합니다.

만약 false로 지정하면 평문(비암호화) 소켓으로 메일을 보내므로, 되도록 true로 설정해야 합니다.

2. AssemblyInfo.cs 수정 (COM 관련 제거)
csharp
복사
편집
using System.Reflection;
using System.Runtime.CompilerServices;

// Project Metadata
[assembly: AssemblyTitle("SimpleNetMail")]
[assembly: AssemblyDescription("PowerBuilder용 TLS(STARTTLS) 메일 발송 .NET Class Library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("YourCompanyName")]
[assembly: AssemblyProduct("SimpleNetMail")]
[assembly: AssemblyCopyright("Copyright © YourCompany 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// COM 관련 설정(ComVisible, Guid 등)은 모두 제거합니다.

// 어셈블리 버전
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
COM 관련(ComVisible, Guid, ProgId 등) 어트리뷰트는 모두 삭제합니다.

PowerBuilder 2019 R3의 “Import .NET Assembly” 기능을 이용하여, 공개(public) 클래스를 곧바로 가져갈 수 있도록 구성해야 합니다.

3. Visual Studio 프로젝트 설정
프로젝트 생성 및 속성

Visual Studio 2022에서 File → New → Project

템플릿: Class Library (.NET Framework)

프로젝트 이름: SimpleNetMail

Target Framework: .NET Framework 4.8

Platform Target: x86 (PowerBuilder 2019 R3는 32비트)

Register for COM interop: 꺼두기 (불필요)

MailSender.cs 파일을 추가하고, 위의 코드를 그대로 붙여넣기

AssemblyInfo.cs(또는 .csproj 내부 어셈블리 설정)에서 COM 관련 설정을 모두 제거

Build Solution

빌드 후 bin\Debug\SimpleNetMail.dll (또는 bin\Release\SimpleNetMail.dll)이 생성됩니다.

4. PowerBuilder 2019 R3에서 TLS 설정된 DLL Import 및 사용
이제 PowerBuilder 2019 R3 프로젝트에서 아래 절차를 따라 .NET Assembly Import를 수행하면, “TLS(STARTTLS) 강제” 메일 발송 메서드를 바로 호출할 수 있습니다.

PowerBuilder IDE를 실행 (관리자 권한이 필요 없습니다).

메뉴에서 Project → Import .NET Assembly…

“.NET DLL Import” 대화상자:

Source .NET DLL: Visual Studio 빌드 출력 폴더에 있는 SimpleNetMail.dll 경로 지정

Framework Type: “.NET Framework” 선택

Destination PBT: Proxy 클래스를 생성할 PBT 파일(예: dotnet_import.pbt) 선택

Destination PBL: Proxy 오브젝트가 들어갈 PBL(예: dotnet_proxy.pbl) 선택

“Select and Preview” 영역에서

SimpleNetMail.dll → SimpleNetMail.MailSender 클래스를 체크

오른쪽 Preview에 생성될 Proxy가 보이면 Import 클릭

Proxy(n_simplenetmail_mailsender)가 생성되면 System Tree → Assemblies에 나타납니다.

PowerBuilder 예제 스크립트 (TLS1.2 강제 사용)
powerscript
복사
편집
// -------------------------------------------------
// 1) DotNetObject 변수 선언
// -------------------------------------------------
DotNetObject    dn_mailer
boolean         lb_result

// SMTP 설정 (예시: Gmail)
string          ls_smtpServer = "smtp.gmail.com"
integer         li_smtpPort   = 587
string          ls_smtpUser   = "your_email@gmail.com"
string          ls_smtpPass   = "your_app_password_or_smtp_password"

// TLS 연결을 사용하려면 true
boolean         lb_useTLS     = TRUE

// 메일 정보
string          ls_from       = "your_email@gmail.com"
string          ls_to         = "recipient1@domain.com;recipient2@domain.com"
string          ls_subject    = "테스트 메일 (TLS1.2)"
string          ls_body       = "PowerBuilder 2019 R3에서 .NET Assembly Import로 보낸 TLS1.2 메일 테스트입니다."

// 첨부파일이 여러 개라면 string[] 배열로 지정
string[]        lsa_attachments
lsa_attachments[1] = "C:\TestFolder\file1.txt"
lsa_attachments[2] = "C:\TestFolder\image2.jpg"

// -------------------------------------------------
// 2) DotNetObject 인스턴스 생성
// -------------------------------------------------
// Import 시 생성된 Proxy 이름: 예시로 "n_simplenetmail_mailsender" 를 사용
dn_mailer = CREATE DotNetObject("n_simplenetmail_mailsender")
IF IsNull(dn_mailer) THEN
    MessageBox("오류", ".NET 객체를 생성할 수 없습니다.")
    RETURN
END IF

// -------------------------------------------------
// 3) SendMail 메서드 호출 (TLS 적용)
// -------------------------------------------------
// 메서드 시그니처: 
// public bool SendMail(
//     string smtpServer, 
//     int smtpPort, 
//     string smtpUser, 
//     string smtpPass, 
//     bool useTLS, 
//     string from, 
//     string to, 
//     string subject, 
//     string body, 
//     string[] attachments
// )
lb_result = dn_mailer.SendMail( &
                ls_smtpServer, &
                li_smtpPort, &
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useTLS, &        // TLS(STARTTLS) 사용 여부
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "TLS 연결을 통해 메일이 성공적으로 전송되었습니다.")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다.")
END IF

// -------------------------------------------------
// 4) 닷넷 객체 정리
// -------------------------------------------------
dn_mailer = NULL
주요 포인트
useTLS를 true로 설정하면 TLS(STARTTLS) 연결이 시도됩니다.

내부적으로 ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; 코드를 실행하여 TLS1.2 이상 프로토콜로만 통신을 수행하도록 강제합니다.

EnableSsl = useTLS;로 설정했기 때문에, SMTP 서버가 STARTTLS(예: Gmail은 포트 587번) 또는 SSL/TLS(예: 포트 465번) 중 하나만 허용해도 동작합니다.

포트 587: STARTTLS (명시적으로 TLS1.2로 업그레이드)

포트 465: SSL/TLS 암호화(서버와 처음부터 SSL/TLS 연결)

둘 다 사용 가능하므로, 환경에 맞게 li_smtpPort 값을 587 또는 465로 지정하세요.

인증서 검증 로직

예제에서는 “sslPolicyErrors == None”일 때만 허용하도록 하였습니다.

테스트 환경에서 자체 서명된 인증서(=Self-signed Cert)를 사용하려면 콜백에서 return true; 만 남기면 됩니다.

5. 정리
변경 전: EnableSsl = true를 이용해 SSL(혹은 TLS를 암묵적으로 선택)

변경 후:

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; 로 명시적으로 TLS1.2 프로토콜만 사용

메서드 파라미터 이름을 useTLS로 변경하여, “TLS(STARTTLS) 사용” 여부를 명확히 함

EnableSsl = useTLS; 로 STARTTLS/TLS 연결을 시도

이제 위 소스를 Visual Studio 2022에서 재빌드한 뒤, PowerBuilder 2019 R3의 “Import .NET Assembly…” 기능을 통해 가져와 실행하면, TLS1.2 이상 통신만 허용되는 메일 전송 기능을 사용할 수 있습니다.






나의 말:
25번 포트를 사용해서 TLS 메일을 보내고 있어 해당 내용 반영하여 수정해줘


ChatGPT의 말:

몇 초 동안 생각함
아래는 “SimpleNetMail” 프로젝트의 MailSender.cs 파일을 다시 수정한 예제입니다.
이번 버전에서는 포트 25를 사용하여 STARTTLS(=TLS) 연결을 수행하도록 주석과 예제 설명을 추가했습니다.
실제 로직 자체는 이전과 동일하게 EnableSsl = true로 설정하면, 포트 25에서도 SMTP 서버가 STARTTLS 확장을 지원한다면 자동으로 TLS 핸드셰이크를 수행합니다.

수정된 MailSender.cs (포트 25용 TLS 지원)
csharp
복사
편집
// ─────────────────────────────────────────────────────────────────────────────
// File   : MailSender.cs
// Project: SimpleNetMail (Class Library, .NET Framework 4.8, x86)
// Purpose: PowerBuilder 2019 R3에서 .NET Assembly Import로 바로 호출 가능한 
//          TLS (STARTTLS) 메일 발송 기능 (첨부파일 지원 포함, 포트 25 사용)
// Author : (작성자 이름, 예: 홍길동)
// Date   : 2025-06-xx
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
    /// SMTP 포트 25를 사용하여 STARTTLS(=TLS) 연결을 수행하도록 구성되어 있습니다.
    /// </summary>
    public class MailSender
    {
        /// <summary>
        /// STARTTLS(=TLS) 연결을 지원하는 SMTP 서버로 메일을 발송합니다.
        /// PowerBuilder에서 string[] attachments 배열을 넘겨 첨부파일을 지정할 수 있습니다.
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

                // 인증서 검증 콜백: 실제 서비스 환경에서는 sslPolicyErrors를 확인하여
                // 보안상 안전한 인증서만 허용해야 합니다.
                // 테스트 환경에서 자체 서명된 인증서를 허용하려면, return true; 만 남기면 됩니다.
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        // sslPolicyErrors == None 일 때만 인증서 유효하다고 판단
                        return sslPolicyErrors == SslPolicyErrors.None;
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
                message.Body    = body ?? string.Empty;
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
                    // 포트 25에 대해 useTLS=true일 경우, SMTP 서버가 EHLO 후 STARTTLS를 지원하면
                    // 암호화 연결로 전환합니다.
                    client.EnableSsl = useTLS;

                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.Timeout     = 300_000; // 타임아웃 5분

                    client.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                // 예외 발생 시 false 반환 (상세 예외 로깅이 필요하면 여기에 로깅 코드를 추가)
                return false;
            }
        }
    }
}
주요 변경 사항 요약
포트 25 STARTTLS 강조

<param name="smtpPort"> 설명에 “포트 25를 사용할 경우 STARTTLS 핸드셰이크가 수행됩니다” 라는 주석을 추가했습니다.

실제 코드에서는 client.EnableSsl = useTLS; 만으로도, 포트 25 사용 시 EHLO → STARTTLS → TLS 업데이트를 자동으로 시도합니다.

SecurityProtocol 설정

csharp
복사
편집
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
SMTP 서버가 TLS1.2를 요구하는 경우, .NET Framework가 기본 설정으로 TLS1.2를 사용하지 않을 수 있어, 명시적으로 TLS1.2 프로토콜만 허용하도록 했습니다.

필요 시 SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 와 같이 여러 버전을 OR 연산자로 묶어도 됩니다.

ServerCertificateValidationCallback

기본적으로 sslPolicyErrors == None 인 경우만 인증서를 허용하도록 설정했습니다.

만약 테스트용 자체 서명(Self-signed) 인증서를 사용 중이라면, 아래처럼 콜백을 수정하여 강제로 허용할 수 있습니다.

csharp
복사
편집
ServicePointManager.ServerCertificateValidationCallback =
    delegate { return true; };  // 무조건 허용 (테스트 전용)
EnableSsl 플래그

csharp
복사
편집
client.EnableSsl = useTLS;
useTLS를 true로 넘길 경우, 포트 25에 대해 STARTTLS 요청(암호화 업그레이드)을 시도합니다.

평문 전송을 원한다면 useTLS = false로 호출하면 됩니다.

AssemblyInfo.cs (COM 특성 제거)
csharp
복사
편집
using System.Reflection;
using System.Runtime.CompilerServices;

// 프로젝트 메타데이터 (버전, 제목, 설명 등)
[assembly: AssemblyTitle("SimpleNetMail")]
[assembly: AssemblyDescription("PowerBuilder용 TLS(STARTTLS) 메일 발송 .NET Class Library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("YourCompanyName")]
[assembly: AssemblyProduct("SimpleNetMail")]
[assembly: AssemblyCopyright("Copyright © YourCompany 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// COM 관련 특성은 모두 제거합니다.
// (PowerBuilder 2019 R3의 Import .NET Assembly 기능은 순수 public 클래스만 필요하기 때문입니다.)

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
Visual Studio 프로젝트 설정
Visual Studio 2022에서 File → New → Project

템플릿: Class Library (.NET Framework)

프로젝트 이름: SimpleNetMail

Target Framework: .NET Framework 4.8

Platform Target: x86 (PowerBuilder 2019 R3가 32비트이므로)

Register for COM interop: 꺼두기

MailSender.cs 파일을 추가하고, 위 코드를 붙여넣기

Properties → AssemblyInfo.cs에서 COM 관련 어트리뷰트(ComVisible, Guid 등)를 모두 삭제

Build → Build Solution

빌드 성공 후, bin\Debug\SimpleNetMail.dll (또는 bin\Release\SimpleNetMail.dll) 확인

PowerBuilder 2019 R3에서 Import 및 호출
아래 예시에 나오는 코드는 “SMTP 포트 25 + STARTTLS” 환경에서 메일을 보내는 사례입니다.

PowerBuilder IDE 실행

Project → Import .NET Assembly…

Source .NET DLL: Visual Studio 빌드 결과인 SimpleNetMail.dll 경로 지정

Framework Type: “.NET Framework”

Destination PBT: (예: dotnet_import.pbt)

Destination PBL: (예: dotnet_proxy.pbl)

“Select and Preview” 에서

SimpleNetMail.dll → SimpleNetMail.MailSender 클래스 선택

Import 클릭

System Tree → Assemblies 에서 Proxy 클래스(예: n_simplenetmail_mailsender)가 생성된 것을 확인

PowerBuilder 예제 스크립트 (포트 25 + TLS)
powerscript
복사
편집
// -------------------------------------------------
// 1) DotNetObject 변수 선언
// -------------------------------------------------
DotNetObject    dn_mailer
boolean         lb_result

// SMTP 설정 (예시: 포트 25 + STARTTLS)
string          ls_smtpServer = "smtp.example.com"
integer         li_smtpPort   = 25       // 포트 25 사용
string          ls_smtpUser   = "user@example.com"
string          ls_smtpPass   = "your_password_or_app_password"
boolean         lb_useTLS     = TRUE     // STARTTLS(=TLS) 사용

// 메일 정보
string          ls_from       = "user@example.com"
string          ls_to         = "recipient1@domain.com;recipient2@domain.com"
string          ls_subject    = "포트 25 + TLS 테스트 메일"
string          ls_body       = "PowerBuilder 2019 R3에서 포트 25 + STARTTLS로 보낸 메일 테스트입니다."

// 첨부파일이 여러 개라면 string[] 배열로 지정
string[]        lsa_attachments
lsa_attachments[1] = "C:\TestFolder\file1.txt"
lsa_attachments[2] = "C:\TestFolder\image2.jpg"

// -------------------------------------------------
// 2) DotNetObject 인스턴스 생성
// -------------------------------------------------
// Import 시 생성된 Proxy 이름: 예: "n_simplenetmail_mailsender"
dn_mailer = CREATE DotNetObject("n_simplenetmail_mailsender")
IF IsNull(dn_mailer) THEN
    MessageBox("오류", ".NET 객체를 생성할 수 없습니다.")
    RETURN
END IF

// -------------------------------------------------
// 3) SendMail 메서드 호출 (포트 25 + STARTTLS)
// -------------------------------------------------
// SendMail(
//     string smtpServer, 
//     int smtpPort, 
//     string smtpUser, 
//     string smtpPass, 
//     bool useTLS, 
//     string from, 
//     string to, 
//     string subject, 
//     string body, 
//     string[] attachments
// )
lb_result = dn_mailer.SendMail( &
                ls_smtpServer, &
                li_smtpPort, &
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useTLS, &       // 포트 25 사용 시 STARTTLS(=TLS) 연결
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "포트 25 + STARTTLS를 통해 메일이 성공적으로 전송되었습니다.")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다.")
END IF

// -------------------------------------------------
// 4) 닷넷 객체 정리
// -------------------------------------------------
dn_mailer = NULL
설명
SMTP 서버: smtp.example.com 등 실제 사용 중인 메일 서버로 교체하세요.

SMTP 포트: 25를 지정하면, client.EnableSsl = true일 때 EHLO 후 STARTTLS를 시도합니다.

useTLS: TRUE로 설정해야 STARTTLS(=TLS) 업그레이드가 이루어집니다.

string[] attachments: 첨부파일 경로가 들어 있는 배열. 없을 경우 lsa_attachments를 값 없이 선언만 해도 됩니다.

주의 사항
포트 25 ➔ STARTTLS 지원 여부 확인

대부분의 ISP나 기관용 SMTP 서버는 포트 25에서 STARTTLS를 지원합니다.

만약 포트 25에서 TLS를 지원하지 않는다면, “리턴 코드 5xx” 또는 “인증 오류” 등이 발생할 수 있습니다. 이 경우 포트 587(STARTTLS) 또는 465(SSL/TLS) 등으로 변경해야 합니다.

사내 방화벽/IPS 설정

일부 방화벽은 포트 25에 대해 암호화 연결(STARTTLS)을 차단할 수도 있으므로, 사내 보안팀에 요청하여 포트 25 STARTTLS 연결이 허용되는지 확인하세요.

인증서 검증 콜백

ServicePointManager.ServerCertificateValidationCallback 부분에서 sslPolicyErrors == None인 경우만 허용하도록 설정했습니다.

테스트용 자체 서명 인증서를 사용한다면, 아래처럼 콜백을 수정하여 “무조건 허용”할 수 있습니다(실환경에서는 권장하지 않음).

csharp
복사
편집
ServicePointManager.ServerCertificateValidationCallback =
    delegate { return true; }; // 모든 인증서 허용 (테스트 전용)
PowerBuilder 플랫폼(x86) 일치

프로젝트 속성 → Build → Platform target을 반드시 x86으로 빌드해야 합니다.

64비트 PB 환경이라면 x64로 변경하고 다시 빌드해야 합니다.

결론
MailSender.cs 내부에서 ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; 와 client.EnableSsl = useTLS; 설정을 통해, 포트 25에서도 STARTTLS(=TLS)가 자동으로 진행되도록 했습니다.

PowerBuilder 2019 R3의 “Import .NET Assembly…” 기능으로 해당 DLL을 가져와, 아래와 같이 DotNetObject로 인스턴스를 생성한 뒤 SendMail(..., 25, true, ...) 로 호출하면 포트 25 + STARTTLS 환경에서 메일을 전송할 수 있습니다.

위 코드를 사용하시면 “포트 25를 이용한 TLS(STARTTLS) 메일 발송” 기능이 정상 동작합니다.






나의 말:
TLS 사용시 인증서가 사설 인증서라서 오류가 발생하는것 같아 인증서 검정로직 제외하고 작동하게 수정해줘


ChatGPT의 말:

2초 동안 생각함
아래는 “SimpleNetMail” 프로젝트의 MailSender.cs 파일을 **서버 인증서 검증 로직을 완전히 생략(항상 통과)**하도록 수정한 예제입니다. 이 버전은 포트 25를 이용해 STARTTLS(=TLS) 연결을 시도하되, 서버가 사설(자체 서명) 인증서를 사용해도 예외 없이 동작하도록 ServerCertificateValidationCallback을 return true로 설정해 두었습니다.

csharp
복사
편집
// ─────────────────────────────────────────────────────────────────────────────
// File   : MailSender.cs
// Project: SimpleNetMail (Class Library, .NET Framework 4.8, x86)
// Purpose: PowerBuilder 2019 R3에서 .NET Assembly Import로 바로 호출 가능한 
//          TLS(STARTTLS) 메일 발송 기능 (첨부파일 지원 포함, 포트 25, 인증서 검증 생략)
// Author : (작성자 이름, 예: 홍길동)
// Date   : 2025-06-xx
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
                message.Body    = body ?? string.Empty;
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
                    client.Timeout     = 300_000; // 타임아웃 5분

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
변경된 부분 요약
ServerCertificateValidationCallback을 무조건 true 반환

csharp
복사
편집
ServicePointManager.ServerCertificateValidationCallback =
    delegate (
        object sender, 
        X509Certificate certificate, 
        X509Chain chain, 
        SslPolicyErrors sslPolicyErrors
    )
    {
        return true;  // 모든 인증서를 허용 (사설 인증서도 통과)
    };
이 로직이 있으면 서버의 인증서 체인(사설 인증서 포함)이 유효하지 않아도 무조건 연결을 허용합니다.

프로덕션 환경에서는 보안상 위험할 수 있으니, 반드시 테스트나 내부 시스템에서만 사용하시기 바랍니다.

STARTTLS 포트 25 사용 안내

메서드 주석과 <param name="smtpPort"> 설명 부분에 “포트 25를 사용할 때 STARTTLS(=TLS) 핸드셰이크를 수행”한다는 내용을 명시했습니다.

실제 코드 로직은 client.EnableSsl = useTLS; 만으로도, 포트 25에서 STARTTLS 업그레이드를 자동으로 시도합니다.

나머지 로직(첨부파일 처리, 메일 전송 등)은 기존과 동일

message 생성 → To, Subject, Body, Attachments 설정

SmtpClient 생성 → EnableSsl, Credentials, Timeout 설정 → Send(message)

AssemblyInfo.cs (변경 없음)
csharp
복사
편집
using System.Reflection;
using System.Runtime.CompilerServices;

// 프로젝트 메타데이터 (버전, 제목, 설명 등)
[assembly: AssemblyTitle("SimpleNetMail")]
[assembly: AssemblyDescription("PowerBuilder용 TLS(STARTTLS) 메일 발송 .NET Class Library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("YourCompanyName")]
[assembly: AssemblyProduct("SimpleNetMail")]
[assembly: AssemblyCopyright("Copyright © YourCompany 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// COM 관련 특성은 모두 제거합니다.

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
Visual Studio 프로젝트 빌드
프로젝트 속성(Properties)

Target Framework: .NET Framework 4.8

Platform Target: x86 (PowerBuilder 2019 R3는 32비트)

Register for COM interop: OFF (비활성화)

MailSender.cs 파일을 추가하고, 위 코드를 그대로 붙여넣기

AssemblyInfo.cs 에서 COM 관련 특성을 모두 삭제

Build → Build Solution

빌드가 성공하면 bin\Debug\SimpleNetMail.dll (또는 bin\Release\SimpleNetMail.dll) 생성

PowerBuilder 2019 R3에서 Import 및 호출
이제 PowerBuilder 2019 R3에서 아래 절차를 따라 .NET Assembly Import를 수행하고, 메일 발송 기능을 호출하면 됩니다.

PowerBuilder IDE 실행

메뉴에서 Project → Import .NET Assembly… 클릭

“.NET DLL Import” 대화상자:

Source .NET DLL: Visual Studio 빌드 결과물(SimpleNetMail.dll)의 경로 지정

Framework Type: “.NET Framework” 선택

Destination PBT: (예: dotnet_import.pbt)

Destination PBL: (예: dotnet_proxy.pbl)

“Select and Preview” 영역에서

SimpleNetMail.dll → SimpleNetMail.MailSender 클래스 선택

Import 클릭

System Tree → Assemblies 아래에 생성된 Proxy 클래스(예: n_simplenetmail_mailsender) 확인

PowerBuilder 예제 스크립트 (포트 25, TLS + 인증서 검증 생략)
powerscript
복사
편집
// -------------------------------------------------
// 1) DotNetObject 변수 선언
// -------------------------------------------------
DotNetObject    dn_mailer
boolean         lb_result

// SMTP 설정 (예시: 포트 25 + STARTTLS, 사설 인증서 허용)
string          ls_smtpServer = "smtp.example.com"
integer         li_smtpPort   = 25     // 포트 25 사용
string          ls_smtpUser   = "user@example.com"
string          ls_smtpPass   = "your_password_or_app_password"
boolean         lb_useTLS     = TRUE   // TRUE면 STARTTLS(=TLS) 연결

// 메일 정보
string          ls_from       = "user@example.com"
string          ls_to         = "recipient1@domain.com;recipient2@domain.com"
string          ls_subject    = "포트 25 + TLS 테스트 메일 (인증서 검증 생략)"
string          ls_body       = "PowerBuilder 2019 R3에서 사설 인증서가 있어도 통과하도록 설정한 메일 테스트입니다."

// 첨부파일이 여러 개라면 string[] 배열로 지정
string[]        lsa_attachments
lsa_attachments[1] = "C:\TestFolder\file1.txt"
lsa_attachments[2] = "C:\TestFolder\image2.jpg"

// -------------------------------------------------
// 2) DotNetObject 인스턴스 생성
// -------------------------------------------------
// Import 시 생성된 Proxy 이름: 예: "n_simplenetmail_mailsender"
dn_mailer = CREATE DotNetObject("n_simplenetmail_mailsender")
IF IsNull(dn_mailer) THEN
    MessageBox("오류", ".NET 객체를 생성할 수 없습니다.")
    RETURN
END IF

// -------------------------------------------------
// 3) SendMail 메서드 호출 (포트 25 + STARTTLS, 인증서 검증 생략)
// -------------------------------------------------
// SendMail(
//     string smtpServer, 
//     int smtpPort, 
//     string smtpUser, 
//     string smtpPass, 
//     bool useTLS, 
//     string from, 
//     string to, 
//     string subject, 
//     string body, 
//     string[] attachments
// )
lb_result = dn_mailer.SendMail( &
                ls_smtpServer, &
                li_smtpPort, &
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useTLS, &       // 포트 25 사용 시 STARTTLS(=TLS) 연결
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "포트 25 + STARTTLS(인증서 검증 생략)로 메일이 성공적으로 전송되었습니다.")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다.")
END IF

// -------------------------------------------------
// 4) 닷넷 객체 정리
// -------------------------------------------------
dn_mailer = NULL
설명
Port 25 + STARTTLS

li_smtpPort = 25로 설정하면, client.EnableSsl = useTLS (useTLS = TRUE) 상황에서 EHLO → STARTTLS(=TLS1.2) 핸드셰이크가 시도됩니다.

인증서 검증 생략

C# 코드의 ServerCertificateValidationCallback이 return true 로 설정되어 있어, 사설(자체 서명) 인증서도 예외 없이 모두 통과합니다.

string[] attachments

PowerBuilder 2019 R3에서 string[] 배열을 생성하여 첨부파일 경로를 전달하면, C# 쪽에서 File.Exists(path) 검사 후 첨부합니다.

파일 경로가 잘못되었거나 파일이 존재하지 않으면 무시됩니다.

에러 처리

C#에서는 예외 발생 시 단순히 false를 반환하므로, 어떤 오류가 났는지 구체적으로 알기 위해서는 C# 쪽에 로그를 추가하거나, out string errorMessage 같은 파라미터를 두고 예외 메시지를 받아올 수 있도록 코드를 확장해야 합니다.

주의 사항
인증서 검증을 완전히 생략하면 Man-in-the-Middle 공격(MITM) 등에 노출될 수 있으므로, 반드시 내부 네트워크나 테스트 환경에서만 사용하시고, 프로덕션 환경에서는 신뢰할 수 있는 인증서를 설치한 서버로 연결하시기 바랍니다.

SMTP 서버가 포트 25에서 STARTTLS를 지원하지 않으면, SmtpException이 발생합니다. 이 경우 포트 587(STARTTLS) 또는 465(SSL/TLS) 포트를 사용하는 편이 안전합니다.

이제 위 코드를 Visual Studio 2022에서 빌드하여 생성된 SimpleNetMail.dll을 PowerBuilder 2019 R3에서 Import 한 다음, SendMail(..., 25, true, ...)로 호출하시면 사설 인증서가 있더라도 예외 없이 TLS(STARTTLS) 메일 전송이 이루어집니다.










