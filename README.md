# SimpleNetMail: PowerBuilder 2019 R3+용 TLS(STARTTLS) 메일 발송 라이브러리 (.NET DLL Import 활용)

## 프로젝트 소개

이 프로젝트는 PowerBuilder 2019 R3 이상 버전에서 .NET Assembly Import 기능을 사용하여 간편하게 TLS (STARTTLS) 암호화 연결로 이메일을 발송할 수 있도록 돕는 .NET Framework Class Library 입니다. 첨부파일 지원 기능이 포함되어 있으며, 특히 사설 인증서를 사용하는 SMTP 서버 환경에서도 인증서 검증 오류 없이 메일을 보낼 수 있도록 설계되었습니다. 포트 25를 통한 STARTTLS 연결 및 사설 인증서 허용 기능에 초점을 맞추고 있습니다.

## 주요 특징

*   **PowerBuilder 2019 R3+ 지원**: PowerBuilder의 내장된 .NET Assembly Import 기능을 통해 별도의 COM 등록 없이 바로 사용 가능합니다.
*   **TLS (STARTTLS) 암호화**: SMTP 통신 시 STARTTLS를 사용하여 안전하게 데이터를 전송합니다. 특히 포트 25에서 STARTTLS를 지원하는 환경에 최적화되어 있습니다.
*   **첨부파일 지원**: 여러 개의 첨부파일을 간편하게 추가하여 발송할 수 있습니다.
*   **사설 인증서 지원**: `ServicePointManager.ServerCertificateValidationCallback` 설정을 통해 서버 인증서 유효성 검사를 생략하여, 사설 인증서 환경에서도 오류 없이 동작합니다 (주의: 보안 위험이 수반될 수 있습니다).
*   **발신/수신자 표시 이름 지원**: 메일 주소와 함께 표시 이름(Alias)을 지정할 수 있습니다.
*   **.NET Framework 4.8 기반**: PowerBuilder 2019 R3가 지원하는 .NET Framework 버전으로 개발되었습니다.
*   **x86 플랫폼**: 대부분의 PowerBuilder 2019 R3 환경(32비트)에 맞추어 x86 플랫폼으로 빌드됩니다.

## 개발 환경

*   Visual Studio 2022 (또는 .NET Framework 4.8 개발이 가능한 환경)
*   .NET Framework 4.8 SDK
*   PowerBuilder 2019 R3 이상 버전

## 설치 및 사용 방법

이 라이브러리는 COM 방식이 아닌, PowerBuilder 2019 R3에 내장된 **.NET Assembly Import** 기능을 사용하여 등록하고 호출합니다.

### 1. C# 프로젝트 빌드

1.  Visual Studio 2022를 실행합니다.
2.  "Class Library (.NET Framework)" 템플릿으로 새 프로젝트를 생성합니다. (예: `SimpleNetMail`)
3.  **타겟 프레임워크**를 `.NET Framework 4.8`로 설정합니다.
4.  프로젝트 속성(Project Properties)에서 **Build** 탭으로 이동합니다.
5.  **Platform target**을 `x86`으로 설정합니다. (대부분의 PowerBuilder 2019 R3 환경이 32비트입니다. 64비트 PB 환경이라면 `x64`로 설정해야 합니다.)
6.  **"Register for COM interop" 옵션은 체크 해제**합니다. (이 프로젝트는 .NET Import 방식을 사용하므로 COM 등록이 필요 없습니다.)
7.  프로젝트에 `MailSender.cs` 파일을 추가하고 필요한 C# 코드를 붙여넣습니다. (이 코드는 TLS1.2 설정, 인증서 검증 생략, 그리고 `SendMail` 및 `SendMailWithAlias` 메서드를 포함해야 합니다. 자세한 코드는 [MailSender.cs](MailSender.cs) 파일을 참고하세요.)
8.  `AssemblyInfo.cs` 파일 (Properties 폴더 아래)을 열어 **COM 관련 특성(Attribute)은 모두 제거**합니다. (예: `[assembly: ComVisible(...)]`, `[assembly: Guid(...)]` 등) 순수한 어셈블리 정보만 남깁니다.
9.  솔루션을 빌드(Build -> Build Solution)합니다.
10. 빌드가 성공하면 프로젝트의 `bin\Debug` 또는 `bin\Release` 폴더에 `SimpleNetMail.dll` 파일이 생성됩니다.

### 2. PowerBuilder 2019 R3+에서 .NET DLL Import

1.  PowerBuilder 2019 R3 이상 IDE를 실행하고 프로젝트를 엽니다. **(관리자 권한이 필요 없습니다.)**
2.  메뉴에서 `Project` -> `Import .NET Assembly...`를 선택합니다.
3.  `.NET DLL Import` 대화상자에서 아래와 같이 설정합니다.
    *   **Source .NET DLL**: 1단계에서 Visual Studio로 빌드한 `SimpleNetMail.dll` 파일 경로를 지정합니다.
    *   **Framework Type**: `.NET Framework`를 선택합니다.
    *   **Destination PBT**: Import된 Proxy 클래스를 저장할 PBT 파일 (예: `myproject.pbt`)을 선택하거나 새로 생성합니다.
    *   **Destination PBL**: Proxy 오브젝트(PB 오브젝트)를 생성할 PBL 파일 (예: `myproject.pbl`)을 선택합니다.
4.  화면 하단의 **Select and Preview** 영역에 `SimpleNetMail.dll` 내부에 있는 `SimpleNetMail.MailSender` 클래스가 보입니다.
5.  `SimpleNetMail.MailSender` 클래스 왼쪽에 있는 체크박스를 체크하여 선택합니다.
6.  오른쪽 **Preview Result** 창에서 PowerBuilder에서 생성될 Proxy 오브젝트 이름 (예: `n_simplenetmail_mailsender`)을 확인합니다. 이 이름은 `n_<어셈블리이름>_<클래스이름>` 규칙으로 자동 생성되며, 스크ript에서 DotNetObject를 생성할 때 사용됩니다.
7.  `Import` 버튼을 클릭하여 DLL을 Import하고 Proxy 오브젝트를 생성합니다.
8.  Import가 완료되면 System Tree의 해당 PBL 아래에 Proxy 오브젝트가 생성된 것을 확인할 수 있습니다.

### 3. PowerBuilder 스크립트에서 MailSender 호출

Import된 Proxy 클래스를 PowerBuilder 스크립트(윈도우, NVO 등)에서 `DotNetObject` 타입으로 선언하고 사용합니다.

#### 3.1. `SendMail` 메서드 사용 예제 (표시 이름 미지원)

```powerscript
// =================================================================================
// 1) DotNetObject 변수 선언
// =================================================================================
DotNetObject    dn_mailer
boolean         lb_result

// SMTP 설정 (예시: 포트 25 + STARTTLS, 사설 인증서 허용)
// 실제 SMTP 서버 주소, 사용자 계정, 비밀번호를 사용하세요.
string          ls_smtpServer = "smtp.your-server.com" // 실제 SMTP 서버 주소 입력
integer         li_smtpPort   = 25                   // 포트 25 사용 (STARTTLS 지원 서버 필요)
string          ls_smtpUser   = "your_email@your-server.com" // 실제 사용자 계정 입력
string          ls_smtpPass   = "your_password_or_app_password" // 실제 비밀번호 입력
boolean         lb_useTLS     = TRUE               // TRUE면 STARTTLS(=TLS) 연결 시도 (권장)

// 메일 정보
string          ls_from       = "your_email@your-server.com" // 보내는 사람 주소 입력
string          ls_to         = "recipient1@domain.com;recipient2@domain.com" // 받는 사람 주소 입력 (세미콜론 또는 콤마로 구분 가능)
string          ls_subject    = "PowerBuilder .NET Import 테스트 메일" // 메일 제목
string          ls_body       = "이 메일은 PowerBuilder 2019 R3 .NET Assembly Import로 발송되었습니다.\n\n포트 25 + STARTTLS + 인증서 검증 생략 설정이 적용되었습니다." // 메일 본문 (Plain Text)

// 첨부파일이 여러 개라면 string[] 배열로 지정
// 첨부파일 경로가 유효해야 메일에 첨부됩니다.
string[]        lsa_attachments
lsa_attachments = CREATE string[2] // 배열 크기 지정 (첨부할 파일 수)
lsa_attachments[1] = "C:\Path\To\Your\file1.txt" // 실제 파일 경로 입력
lsa_attachments[2] = "C:\Path\To\Your\image.png" // 실제 파일 경로 입력
// 첨부파일이 없을 경우: string[] lsa_attachments (선언만 하고 값 할당 없거나 Null)

// =================================================================================
// 2) DotNetObject 인스턴스 생성
// =================================================================================
// Import 시 생성된 Proxy 이름을 정확히 입력해야 합니다.
// System Tree -> Assemblies 아래에서 "SimpleNetMail.MailSender" 항목의 Properties를 확인하세요.
string ls_proxy_name = "n_simplenetmail_mailsender" // Proxy 이름 확인 후 필요시 수정

dn_mailer = CREATE DotNetObject(ls_proxy_name)
IF IsNull(dn_mailer) THEN
    MessageBox("오류", ".NET 객체를 생성할 수 없습니다." + &
               "Proxy 이름 확인: '" + ls_proxy_name + "'" + &
               "DLL 경로 및 .NET Framework 설치 확인.")
    RETURN
END IF

// =================================================================================
// 3) SendMail 메서드 호출 (포트 25 + STARTTLS, 인증서 검증 생략)
// =================================================================================
/*
C# 메서드 시그니처:
public bool SendMail(
    string smtpServer,
    int smtpPort,
    string smtpUser,
    string smtpPass,
    bool useTLS,        // <-- 이 파라미터가 TLS/STARTTLS 사용 여부 결정
    string from,
    string to,         // 받는 사람 주소, 세미콜론 또는 콤마로 구분 가능
    string subject,
    string body,
    string[] attachments
)
*/
lb_result = dn_mailer.SendMail( &
                ls_smtpServer, &
                li_smtpPort, &      // 25번 포트 사용
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useTLS, &        // TRUE로 설정 시 STARTTLS 시도
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments )

IF lb_result = TRUE THEN
    MessageBox("성공", "메일이 성공적으로 전송되었습니다.")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다." + &
               "SMTP 설정, 계정/비밀번호, 방화벽, 서버 STARTTLS 지원 여부 등을 확인하세요.")
END IF

// =================================================================================
// 4) 닷넷 객체 정리
// =================================================================================
dn_mailer = NULL // 객체 해제
```

#### 3.2. `SendMailWithAlias` 메서드 사용 예제 (표시 이름 지원)

`SendMail` 메서드와 동일한 파라미터에 더해, 보내는 사람과 받는 사람의 표시 이름(Display Name)을 추가로 지정할 수 있습니다.

```powerscript
// =================================================================================
// 1) DotNetObject 변수 선언
// =================================================================================
DotNetObject    dn_mailer
boolean         lb_result

// SMTP 설정 (예시: 포트 25 + STARTTLS, 사설 인증서 허용)
// 실제 SMTP 서버 주소, 사용자 계정, 비밀번호를 사용하세요.
string          ls_smtpServer = "smtp.your-server.com" // 실제 SMTP 서버 주소 입력
integer         li_smtpPort   = 25                   // 포트 25 사용 (STARTTLS 지원 서버 필요)
string          ls_smtpUser   = "your_email@your-server.com" // 실제 사용자 계정 입력
string          ls_smtpPass   = "your_password_or_app_password" // 실제 비밀번호 입력
boolean         lb_useTLS     = TRUE               // TRUE면 STARTTLS(=TLS) 연결 시도 (권장)

// 메일 정보 (표시 이름 포함)
string          ls_from       = "your_email@your-server.com" // 보내는 사람 주소 입력
string          ls_from_alias = "보내는사람 이름"          // 보내는 사람 표시 이름 (생략 가능 - "" 또는 Null)

string          ls_to         = "recipient1@domain.com;recipient2@domain.com" // 받는 사람 주소 입력 (세미콜론 또는 콤마로 구분 가능)
string          ls_to_alias   = "받는사람 이름"          // 모든 받는 사람에게 동일하게 적용될 표시 이름 (생략 가능 - "" 또는 Null)
// 참고: C# 코드 특성상 받는 사람별 개별 표시 이름 지정은 현재 지원되지 않습니다.

string          ls_subject    = "PowerBuilder .NET Import 테스트 (표시 이름 포함)" // 메일 제목
string          ls_body       = "이 메일은 PowerBuilder 2019 R3 .NET Assembly Import로 발송되었습니다.\n\n표시 이름, 포트 25 + STARTTLS + 인증서 검증 생략 설정이 적용되었습니다." // 메일 본문 (Plain Text)

// 첨부파일이 여러 개라면 string[] 배열로 지정
// 첨부파일 경로가 유효해야 메일에 첨부됩니다.
string[]        lsa_attachments
lsa_attachments = CREATE string[1] // 배열 크기 지정 (첨부할 파일 수)
lsa_attachments[1] = "C:\Path\To\Your\another_file.pdf" // 실제 파일 경로 입력
// 첨부파일이 없을 경우: string[] lsa_attachments (선언만 하고 값 할당 없거나 Null)


// =================================================================================
// 2) DotNetObject 인스턴스 생성
// =================================================================================
// Import 시 생성된 Proxy 이름을 정확히 입력해야 합니다.
// System Tree -> Assemblies 아래에서 "SimpleNetMail.MailSender" 항목의 Properties를 확인하세요.
string ls_proxy_name = "n_simplenetmail_mailsender" // Proxy 이름 확인 후 필요시 수정

dn_mailer = CREATE DotNetObject(ls_proxy_name)
IF IsNull(dn_mailer) THEN
    MessageBox("오류", ".NET 객체를 생성할 수 없습니다." + &
               "Proxy 이름 확인: '" + ls_proxy_name + "'" + &
               "DLL 경로 및 .NET Framework 설치 확인.")
    RETURN
END IF

// =================================================================================
// 3) SendMailWithAlias 메서드 호출 (포트 25 + STARTTLS, 인증서 검증 생략, 표시 이름 포함)
// =================================================================================
/*
C# 메서드 시그니처:
public bool SendMailWithAlias(
    string smtpServer,
    int smtpPort,
    string smtpUser,
    string smtpPass,
    bool useTLS,        // <-- 이 파라미터가 TLS/STARTTLS 사용 여부 결정
    string from,
    string to,
    string subject,
    string body,
    string[] attachments,
    string fromDisplayName = null, // <-- 보내는 사람 표시 이름 (옵션)
    string toDisplayName = null    // <-- 받는 사람 표시 이름 (옵션 - 세미콜론 또는 콤마로 구분 가능)
)
*/
lb_result = dn_mailer.SendMailWithAlias( &
                ls_smtpServer, &
                li_smtpPort, &      // 25번 포트 사용
                ls_smtpUser, &
                ls_smtpPass, &
                lb_useTLS, &        // TRUE로 설정 시 STARTTLS 시도
                ls_from, &
                ls_to, &
                ls_subject, &
                ls_body, &
                lsa_attachments, &
                ls_from_alias, &    // 보내는 사람 표시 이름 전달
                ls_to_alias )       // 받는 사람 표시 이름 전달

IF lb_result = TRUE THEN
    MessageBox("성공", "메일이 성공적으로 전송되었습니다 (표시 이름 포함).")
ELSE
    MessageBox("실패", "메일 전송 중 오류가 발생했습니다." + &
               "SMTP 설정, 계정/비밀번호, 방화벽, 서버 STARTTLS 지원 여부 등을 확인하세요.")
END IF

// =================================================================================
// 4) 닷넷 객체 정리
// =================================================================================
dn_mailer = NULL // 객체 해제
```

## 중요 고려사항 및 주의사항

*   **인증서 검증 생략 위험**: C# 코드에서 `ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };` 로 설정되어 있어 서버 인증서 유효성 검사를 수행하지 않습니다. **이는 보안상 위험할 수 있으며, 중간자 공격(Man-in-the-Middle Attack)에 취약해질 수 있습니다.** 반드시 내부 네트워크나 테스트 환경에서만 사용하고, 프로덕션 환경에서는 신뢰할 수 있는 공인 인증서를 사용하거나 인증서 검증 로직을 추가하는 것을 강력히 권장합니다.
*   **포트 25 및 STARTTLS 지원**: 사용하는 SMTP 서버가 포트 25에서 STARTTLS 확장을 지원해야 합니다. 일부 서버는 포트 25에서 STARTTLS를 지원하지 않거나, 방화벽/정책에 의해 차단될 수 있습니다. 이 경우 포트 587 (일반적인 STARTTLS 포트) 또는 포트 465 (SSL/TLS 포트)를 사용하도록 설정하거나, 방화벽 정책을 확인해야 합니다.
*   **PowerBuilder 플랫폼 일치**: Visual Studio에서 빌드하는 DLL의 Platform target (`x86` 또는 `x64`)은 PowerBuilder 실행 파일(`pb<버전>.exe`)의 비트수와 일치해야 합니다. 대부분의 PB 2019 R3는 32비트이므로 `x86`으로 빌드합니다. (64비트 PB에서 32비트 DLL을 실행하는데는 장애가 없으니 괜찮습니다.)
*   **SMTP 인증**: 사용하는 SMTP 서버의 인증 방식(계정/비밀번호)이 올바른지 확인해야 합니다. Gmail 같은 서비스는 앱 전용 비밀번호가 필요할 수 있습니다.
*   **첨부파일 경로**: PowerBuilder 스크립트에서 `lsa_attachments` 배열에 지정하는 파일 경로는 PowerBuilder 애플리케이션이 실행되는 환경에서 실제로 접근 가능한 유효한 경로여야 합니다. 파일이 존재하지 않으면 첨부되지 않습니다.
*   **에러 처리**: 현재 C# 코드는 예외 발생 시 단순히 C:\Temp\SimpleNetMail_Error.log 파일에 오류 로그를 남깁니다. 좀 더 상세한 로깅이 필요하다면 코드 수정이 필요합니다.

## 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 `LICENSE.txt` 파일을 참조하십시오.
