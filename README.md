# PomodoroWPF

一个精心打造的 WPF 番茄钟桌面应用，采用 .NET 8 + MVVM 架构，**零外部依赖**，开箱即用。

> 极简设计 · 功能完备 · 性能卓越 · 无需安装

## 功能特性

### 计时器
- **番茄倒计时** — 可配置工作时长（默认 25 分钟），环形进度条可视化，最后 10 秒滴答提示音
- **自动休息** — 短休息（5 分钟）和长休息（15 分钟）自动切换，每 N 个番茄后触发长休息
- **正计时模式** — 独立的秒表功能，HH:MM:SS 格式显示，适合灵活计时场景

### 任务管理
- 添加 / 编辑 / 删除任务，支持优先级（高 / 中 / 低）和预估番茄数
- 设定当前专注任务，番茄完成后自动累计实际番茄数
- 任务完成状态标记，进度一目了然
- 支持任务列表管理，随时切换专注目标

### 数据统计
- 今日 / 本周 / 本月 / 累计番茄数和专注时长统计
- 连续打卡天数和最长连续记录追踪
- GitHub 风格 16 周热力图可视化，直观展示专注习惯
- 数据导出为 CSV 或 JSON 格式，方便二次分析

### 成就系统
- 5 个精心设计的成就：初窥门径（首个番茄）、日进四茄、一周坚持、月不间断、百茄达成
- 解锁时系统托盘通知提醒（与番茄完成通知合并显示，避免通知覆盖），激励持续专注
- 成就进度持久化保存

### 系统集成
- **系统托盘**：最小化到托盘驻留，双击恢复，右键菜单快捷操作
- **全局热键**：`Ctrl+Shift+P` 开始/暂停，`Ctrl+Shift+R` 重置（通过 `RegisterHotKey` 系统级注册，仅在倒计时页面激活时响应）
- **全屏模式**：F11 一键切换全屏沉浸式专注，ESC 退出全屏
- **多主题**：4 套精美主题 — 黑金（默认）、深蓝、森林、极简白，全局实时切换

## 技术栈

- **运行时**：.NET 8.0 (`net8.0-windows`)
- **UI 框架**：WPF + Windows Forms（仅用于 NotifyIcon 托盘支持）
- **架构模式**：MVVM（手动 Composition Root，无 DI 容器）
- **依赖管理**：零 NuGet 包，全部使用 BCL 和 Win32 P/Invoke
- **序列化**：System.Text.Json
- **音频**：SoundPlayer（程序合成 WAV + 异步播放）
- **字体**：使用系统内置微软雅黑（Microsoft YaHei），全局样式统一应用，无需嵌入字体文件

## 项目结构

```
PomodoroWPF/
├── App.xaml / App.xaml.cs          # 应用入口，Composition Root
├── MainWindow.xaml / .xaml.cs      # 主窗口 XAML + ProgressRing 桥接
├── PomodoroWPF.csproj
│
├── Infrastructure/                 # MVVM 基础设施
│   ├── ViewModelBase.cs            #   INotifyPropertyChanged 基类
│   ├── RelayCommand.cs             #   ICommand 实现（含泛型版本）
│   └── ServiceLocator.cs           #   静态服务定位器（依赖注入）
│
├── Models/                         # 数据模型层
│   ├── AppSettings.cs              #   应用设置（主题、时长等）
│   ├── DailyStats.cs               #   每日统计数据
│   ├── PomodoroTask.cs             #   任务模型（含优先级、番茄计数）
│   └── Achievement.cs              #   成就定义与状态
│
├── Services/                       # 业务服务层
│   ├── TimerService.cs             #   DispatcherTimer 封装（倒计时/正计时）
│   ├── PersistenceService.cs       #   JSON 文件持久化（设置、任务、统计）
│   ├── TaskService.cs              #   任务管理（CRUD + 当前任务）
│   ├── StatsService.cs             #   统计聚合（日/周/月/累计 + 热力图数据）
│   ├── AchievementService.cs       #   成就检测与解锁通知
│   ├── HotkeyService.cs            #   全局热键（RegisterHotKey P/Invoke）
│   └── DataExportService.cs        #   数据导出（CSV / JSON）
│
├── ViewModels/                     # 视图模型层
│   ├── MainViewModel.cs            #   主导航协调 + 番茄完成流程
│   ├── HomeViewModel.cs            #   首页（时钟、统计摘要、每日目标）
│   ├── CountdownViewModel.cs       #   倒计时（工作/休息自动循环）
│   ├── StopwatchViewModel.cs       #   正计时（秒表模式）
│   ├── TaskListViewModel.cs        #   任务列表管理
│   ├── StatsViewModel.cs           #   统计面板（热力图 + 成就展示）
│   └── SettingsViewModel.cs        #   设置面板（主题、时长配置）
│
├── Views/                          # 视图层（纯 C# 代码构建，无 XAML）
│   ├── TaskListWindow.cs           #   任务列表窗口
│   ├── AddTaskWindow.cs            #   添加/编辑任务对话框
│   ├── SettingsWindow.cs           #   设置窗口
│   ├── StatsWindow.cs              #   统计窗口（含热力图 + 成就列表）
│   └── InfoDialogWindow.cs         #   通用信息对话框
│
├── Controls/                       # 自定义控件
│   └── HeatmapControl.cs           #   GitHub 风格热力图控件
│
├── Converters/                     # 值转换器
│   └── BoolToVisibilityConverter.cs
│
├── ProgressRing.cs                 # 环形进度条控件（首页双环显示）
├── RoundButton.cs                  # 圆角按钮控件（支持主题感知）
├── SoundManager.cs                 # 提示音生成与播放（滴答声、完成音）
├── ThemeManager.cs                 # 多主题管理（4 套主题，全局实时切换）
└── TrayManager.cs                  # 系统托盘集成（NotifyIcon）
```

### 架构说明

采用轻量级 MVVM 模式，核心设计原则：

- **Composition Root**：`App.xaml.cs` 的 `OnStartup` 按依赖顺序创建所有服务 → 注册到 ServiceLocator → 创建 ViewModel（构造函数注入）→ 创建窗口 → 连线事件
- **页面导航**：`MainViewModel` 通过 `PageType` 枚举和 `BoolToVisibility` 绑定切换三个页面（首页 / 倒计时 / 正计时），无需导航框架
- **ViewModel 通信**：子 ViewModel 通过 C# 事件向上传递（如 `PomodoroCompleted`、`BreakCompleted`、`TasksChanged`），`MainViewModel` 订阅并协调跨 ViewModel 状态同步
- **数据绑定**：主窗口使用 XAML `{Binding}`，所有子窗口（设置、统计、任务列表等）通过 C# 代码动态构建 UI，避免 XAML 冗余
- **ProgressRing 桥接**：code-behind 订阅 ViewModel 的 `PropertyChanged` 事件，调用命令式 `Set()` 方法更新环形进度条，实现数据驱动 UI
- **主题系统**：`ThemeManager` 通过 `DynamicResource` 和 `ThemeChanged` 事件实现全局实时切换，`RoundButton` 通过 `ThemeRole` 属性自动感知主题变化
- **异常容错**：关键服务（音频、热键、托盘）初始化失败时优雅降级，不影响核心功能

### 字体配置

项目使用 Windows 系统内置的微软雅黑（Microsoft YaHei）字体，无需嵌入字体文件，减小发布体积。

- **全局样式**：`App.xaml` 为 `TextBlock` 和 `TextBox` 定义隐式样式，默认应用微软雅黑
- **显式指定**：XAML 和 C# 代码中的文本控件均显式设置 `FontFamily = "Microsoft YaHei"`，确保一致性
- **系统要求**：Windows 10 / 11 均预装微软雅黑字体，无需额外安装

## 编译与运行

### 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（8.0.0 或更高版本）
- Windows 10 / 11 x64

### 开发调试

```bash
# 克隆项目后进入目录
dotnet run
```

### 发布独立 exe

```bash
dotnet publish -c Release -o publish
```

生成的文件位于项目根目录下的 **`publish/`** 文件夹中：
- `publish/PomodoroWPF.exe` — 自包含的单文件可执行程序

内嵌完整 .NET 运行时，目标机器无需安装任何运行时环境。

> 提示：首次运行会自动在 `publish/` 同级目录生成配置文件和数据文件。

### 数据存储

首次运行会自动在 exe 同级目录生成：
- `sounds/` — 程序合成的提示音 wav 文件（tick.wav、chime.wav）
- `settings.json` — 应用设置（主题、时长等，关闭应用后自动保存）
- `stats.json` — 每日统计数据
- `stats_history.json` — 历史统计记录
- `tasks.json` — 任务列表
- `achievements.json` — 成就进度与解锁状态

## 快捷键

| 快捷键 | 功能 | 作用域 |
|--------|------|--------|
| `Ctrl+Shift+P` | 开始 / 暂停倒计时 | 全局（系统级注册，仅倒计时页面响应） |
| `Ctrl+Shift+R` | 重置倒计时 | 全局（系统级注册，仅倒计时页面响应） |
| `F11` / `Escape` | 切换全屏模式 | 应用内 |
| `Ctrl+Q` | 退出应用 | 应用内 |

> 全局热键通过 Win32 `RegisterHotKey` API 注册，可在任意应用程序中触发，但仅在倒计时页面激活时执行操作。

## 设计亮点

- **零依赖**：无需任何第三方 NuGet 包，完全基于 .NET BCL 和 Win32 API
- **现代 UI**：环形进度条、热力图、多主题，极简设计语言
- **开箱即用**：单文件发布，无需安装 .NET 运行时，便携可移动
- **数据持久化**：所有数据本地 JSON 存储，隐私安全，方便备份
- **中文优化**：使用微软雅黑字体，App.xaml 全局样式 + 逐元素显式指定双重保障，确保中英文显示效果

## 许可证

MIT License

---

**享受专注，提升效率！**
