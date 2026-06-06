# PomodoroWPF

一个基于 WPF 的番茄钟桌面应用，采用 .NET 8 + MVVM 架构，零外部依赖。

## 功能特性

### 计时器
- **番茄倒计时** — 可配置工作时长（默认 25 分钟），环形进度条可视化，最后 10 秒滴答提示音
- **自动休息** — 短休息（5 分钟）和长休息（15 分钟）自动切换，每 N 个番茄后触发长休息
- **正计时模式** — 独立的秒表功能，HH:MM:SS 格式显示

### 任务管理
- 添加 / 编辑 / 删除任务，支持优先级（高 / 中 / 低）和预估番茄数
- 设定当前任务，番茄完成后自动累计实际番茄数
- 任务完成标记，进度一目了然

### 数据统计
- 今日 / 本周 / 本月 / 累计番茄数和专注时长
- 连续打卡天数和最长连续记录
- GitHub 风格 16 周热力图可视化
- 数据导出为 CSV 或 JSON

### 成就系统
- 5 个预定义成就：初窥门架（首个番茄）、日进四茄、一周坚持、月不间断、百茄达成
- 解锁时系统托盘通知提醒

### 白噪音
- 三种程序生成的环境音：雨声、风声、咖啡厅
- 基于 Win32 WaveOut API 实时合成，无需外部音频文件
- 工作时段自动播放，休息时自动停止

### 系统集成
- 系统托盘驻留，支持最小化到托盘
- 全局热键：`Ctrl+Shift+P` 开始/暂停，`Ctrl+Shift+R` 重置
- F11 全屏模式切换
- 4 套主题：黑金（默认）、深蓝、森绿、浅色

## 技术栈

- **运行时**：.NET 8.0 (`net8.0-windows`)
- **UI 框架**：WPF + Windows Forms（仅用于 NotifyIcon 托盘支持）
- **架构**：MVVM（手动 Composition Root，无 DI 容器）
- **外部依赖**：零 NuGet 包，全部使用 BCL 和 Win32 P/Invoke
- **序列化**：System.Text.Json
- **音频**：winmm.dll WaveOut API（白噪音）、SoundPlayer（提示音）

## 项目结构

```
PomodoroWPF/
├── App.xaml / App.xaml.cs          # 应用入口，Composition Root
├── MainWindow.xaml / .xaml.cs      # 主窗口 XAML + ProgressRing 桥接
├── PomodoroWPF.csproj
│
├── Infrastructure/                 # MVVM 基础设施
│   ├── ViewModelBase.cs            #   INotifyPropertyChanged 基类
│   ├── RelayCommand.cs             #   ICommand 实现
│   └── ServiceLocator.cs           #   静态服务定位器
│
├── Models/                         # 数据模型
│   ├── AppSettings.cs              #   应用设置
│   ├── DailyStats.cs               #   每日统计
│   ├── PomodoroTask.cs             #   任务模型（含优先级、番茄计数）
│   └── Achievement.cs              #   成就模型
│
├── Services/                       # 业务服务层
│   ├── TimerService.cs             #   DispatcherTimer 封装
│   ├── PersistenceService.cs       #   JSON 文件持久化
│   ├── TaskService.cs              #   任务管理（CRUD + 当前任务）
│   ├── StatsService.cs             #   统计聚合（日/周/月/年 + 热力图）
│   ├── AchievementService.cs       #   成就检测与解锁
│   ├── AmbientSoundService.cs      #   白噪音合成（WaveOut P/Invoke）
│   ├── HotkeyService.cs            #   全局热键（RegisterHotKey P/Invoke）
│   └── DataExportService.cs        #   CSV / JSON 导出
│
├── ViewModels/                     # 视图模型层
│   ├── MainViewModel.cs            #   导航协调 + 番茄完成流程
│   ├── HomeViewModel.cs            #   首页（时钟、统计摘要、每日目标）
│   ├── CountdownViewModel.cs       #   倒计时（工作/休息循环）
│   ├── StopwatchViewModel.cs       #   正计时
│   ├── TaskListViewModel.cs        #   任务列表
│   ├── StatsViewModel.cs           #   统计面板
│   └── SettingsViewModel.cs        #   设置面板
│
├── Views/                          # 视图层（C# 代码构建，无 XAML）
│   ├── TaskListWindow.cs           #   任务列表窗口
│   ├── AddTaskWindow.cs            #   添加/编辑任务对话框
│   ├── SettingsWindow.cs           #   设置窗口
│   ├── StatsWindow.cs              #   统计窗口（含热力图 + 成就）
│   └── InfoDialogWindow.cs         #   通用信息对话框
│
├── Controls/
│   └── HeatmapControl.cs           # GitHub 风格热力图自定义控件
│
├── Converters/
│   └── BoolToVisibilityConverter.cs
│
├── ProgressRing.cs                 # 环形进度条控件
├── RoundButton.cs                  # 圆角按钮控件
├── SoundManager.cs                 # 提示音生成与播放
├── ThemeManager.cs                 # 多主题管理
└── TrayManager.cs                  # 系统托盘集成
```

### 架构说明

采用轻量级 MVVM 模式，核心设计：

- **Composition Root**：`App.xaml.cs` 的 `OnStartup` 按顺序创建所有服务 → 注册到 ServiceLocator → 创建 ViewModel（构造函数注入）→ 创建窗口 → 连线事件
- **页面导航**：`MainViewModel` 通过 `PageType` 枚举和 `BoolToVisibility` 绑定切换三个页面（首页 / 倒计时 / 正计时）
- **ViewModel 通信**：子 ViewModel 通过 C# 事件向上传递（如 `PomodoroCompleted`、`BreakCompleted`、`TasksChanged`），`MainViewModel` 订阅并协调跨 ViewModel 更新
- **数据绑定**：主窗口使用 XAML `{Binding}`，所有子窗口（设置、统计、任务列表等）通过 C# 代码构建 UI
- **ProgressRing 桥接**：code-behind 订阅 ViewModel 的 `PropertyChanged` 事件，调用命令式 `Set()` 方法更新环形进度条

## 编译与运行

### 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（8.0.0 或更高版本）
- Windows 10/11 x64

### 开发调试

```bash
dotnet run
```

### 发布独立 exe

```bash
dotnet publish -c Release -o publish
```

生成的 `publish/PomodoroWPF.exe` 是自包含的单文件可执行程序（约 155MB），内嵌完整 .NET 运行时，目标机器无需安装任何运行时环境。

首次运行会自动在 exe 同级目录生成：
- `sounds/` — 程序合成的提示音 wav 文件
- `settings.json` — 应用设置（关闭应用后保存）
- `stats.json` / `stats_history.json` — 统计数据
- `tasks.json` — 任务列表
- `achievements.json` — 成就进度

## 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Shift+P` | 开始 / 暂停倒计时（全局） |
| `Ctrl+Shift+R` | 重置倒计时（全局） |
| `F11` | 切换全屏 |
| `Ctrl+Q` | 退出应用 |

## 许可证

MIT License
