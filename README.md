# 專案管理網站（Blazor）

## ⚠️ 請先閱讀：這份程式碼尚未在任何環境實際編譯驗證過

我開發程式的這個雲端環境，對外連線被組織政策擋掉了 `api.nuget.org`（NuGet 套件來源），
所以在這裡完全無法執行 `dotnet restore` / `dotnet build`，也就無法實際編譯驗證這份程式碼。
我已經非常仔細地手動檢查過每一支檔案（命名空間、using、API 用法、Razor 語法、大括號配對都核對過），
但沒有實際跑過編譯器，不能保證 100% 沒有筆誤或小錯誤。

**請你在自己有網路的機器上，第一件事就是跑 `dotnet restore` + `dotnet build`**（步驟見下方），
如果有編譯錯誤，把錯誤訊息貼給我，我可以立刻幫你修正。

---

## 這份程式做了什麼、沒做什麼

根據你確認的需求，這一版範圍是：

| 功能 | 狀態 |
|---|---|
| 登入帳號權限控管（Admin/Manager/Member/Viewer 四種角色） | ✅ 已實作 |
| 專案 + 子工作項目（預估時數） | ✅ 已實作 |
| 專案總時數／完成度自動計算 | ✅ 已實作（子工作項目只有完成/未完成二態） |
| 一般需求（直接更新進度/狀態） | ✅ 已實作 |
| 一般需求進度歷程紀錄（誰、何時、改成多少） | ✅ 已實作 |
| 專案／需求負責人（單一負責人） | ✅ 已實作 |
| 系統類別（SFCS/機況/標籤/品質…，單選） | ✅ 已實作，可在後台新增 |
| 站內通知（被指派、進度被更新時） | ✅ 已實作（站內通知鈴鐺；尚未接 Email，見下方） |
| 附件上傳/下載 | ✅ 已實作（存在 wwwroot 之外，下載需登入） |
| 績效指標(KPI)評分與統計 | ❌ 依你的指示，這版**完全不做** |
| 與既有系統（SFCS等）資料串接 | ❌ 依你的指示不需要 |

## 技術棧

- .NET 8 / Blazor Server（Interactive Server render mode）
- ASP.NET Core Identity（帳號、角色）
- Entity Framework Core 8 + **SQL Server**（對應你說的 Windows Server + SQL Server 部署環境）
- Bootstrap 5（模板內建）

## 專案結構

```
ProjectManagement.sln
src/ProjectManagement.Web/
  Data/                      ApplicationDbContext、ApplicationUser（自訂 Identity 使用者）
  Models/                    Department、SystemCategory、Project、ProjectSubTask、
                             GeneralRequirement、GeneralRequirementProgressLog、
                             Attachment、Notification、Enums（含角色名稱 RoleNames）
  Services/                  業務邏輯（ProjectService、RequirementService、
                             AttachmentService、NotificationService、DbSeeder）
  Components/Pages/
    Home.razor               首頁儀表板
    Projects/                專案列表／新增／詳情（子工作項目、附件）
    Requirements/            一般需求列表／新增／詳情（進度更新、歷程、附件）
    Admin/                   單位維護／系統類別維護／使用者與角色（僅 Admin）
    Account/                 登入/註冊/個人資料（ASP.NET Core Identity 內建）
  Components/Shared/
    NotificationBell.razor   導覽列的通知鈴鐺
```

## 建置與執行步驟（請在有網路的機器上進行）

### 1. 安裝需求

- .NET 8 SDK
- SQL Server（本機開發可用 SQL Server Express / LocalDB；正式環境用你們的 Windows Server + SQL Server）

### 2. 還原套件並建置（第一步，務必先做）

```bash
cd ProjectManagement
dotnet restore
dotnet build
```

如果這步有錯誤，請把完整錯誤訊息貼給我。

### 3. 設定資料庫連線字串

編輯 `src/ProjectManagement.Web/appsettings.json`（或用 `appsettings.Development.json` / User Secrets 覆寫，正式環境建議用環境變數，不要把正式連線字串放進原始碼庫）：

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=你的SQLServer位址;Database=ProjectManagement;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### 4. 建立資料庫結構（Migrations）

這份程式碼本身**還沒有附上 Migration 檔案**——因為產生 Migration 需要先成功編譯，而這在目前環境做不到。
請在你自己的機器上，restore/build 成功後執行：

```bash
dotnet tool install --global dotnet-ef   # 如果還沒裝過
cd src/ProjectManagement.Web
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. 執行

```bash
dotnet run --project src/ProjectManagement.Web
```

### 6. 預設管理員帳號

第一次啟動時會自動建立：

- 帳號：`admin@projectmanagement.local`
- 密碼：`Admin#12345`

**請務必登入後立刻改密碼**，或直接改 `Services/DbSeeder.cs` 裡的常數後再啟動。

## 使用方式

- Admin 到「使用者與角色」頁面建立同事的帳號、指派角色與單位（也可以用內建的 `/Account/Register` 頁面自行註冊，但既然是內部系統，建議正式上線前把這個公開註冊頁面關掉，改成全部由 Admin 建立帳號）。
- Admin 到「單位維護」「系統類別維護」把公司實際的單位、系統類別（SFCS、機況、標籤、品質…）建好。
- Manager/Admin 建立專案，在專案詳情頁新增子工作項目並填預估時數；子工作項目打勾「完成」後，專案總時數與完成度會自動重算。
- Manager/Admin 建立一般需求；負責人（或 Manager/Admin）在需求詳情頁「更新進度」，每次更新都會留下歷程紀錄。
- 兩者都可以上傳附件、指定負責人；負責人被指派或需求進度被更新時，會收到站內通知（畫面右上角的鈴鐺）。

## 之後可以再加的東西（目前先不做）

- **KPI 績效指標評分與統計**：你先前給我的困難度(1-5)、效益性三項指標(2/5/10分)的規則我都記錄下來了，等你需要時我可以直接依那個規格加回來，資料庫結構也預留了擴充空間。
- **Email 通知**：目前 `INotificationService` 只寫站內通知，要接 SMTP 寄信的話只要在 `NotificationService.NotifyAsync` 裡加寄信邏輯即可，不用動呼叫端的程式。
- **多人指派 / 分工**：目前是單一負責人；如果之後某些專案需要多人分工，需要把 `ResponsibleUserId` 改成多對多的指派表。
