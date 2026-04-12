# BFramework

基于 [HybridCLR](https://github.com/focus-creative-games/hybridclr) 的 Unity 轻量热更新框架

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue)](https://unity.com/)
[![HybridCLR](https://img.shields.io/badge/HybridCLR-热更新-orange)](https://github.com/focus-creative-games/hybridclr)
[![License](https://img.shields.io/badge/license-MIT-lightgrey)](LICENSE)

> 📦 本仓库为基础版框架，功能完整的扩展版请访问 **[BFramework-Ex](https://github.com/ToxicStar8/BFramework-Ex)**（包含 UI 框架、红点系统、网络通信、定时器、状态机等十余个生产级模块）。

---

## 简介

`BFramework` 是一个面向 Unity 的轻量级热更新基础框架，核心功能包括：

- 🔥 基于 [HybridCLR](https://github.com/focus-creative-games/hybridclr) 的原生 C# 热更新
- 📦 AssetBundle 打包与增量热更新（MD5 比对 + 断点重试）
- 🎮 游戏入口管理、UI 层级管理
- 🛠️ Editor 一键打包工具链
- 📝 分级日志系统（仅编辑器生效，零运行时开销）

---

## 目录结构

```
Assets/
├── Editor/          # 编辑器工具（AB包打包、ABConfig、热更说明）
├── GameData/        # 游戏数据（预制体、热更业务脚本）
├── MainPackage/     # 主包（游戏入口、下载管理器、工具类）
└── Scenes/          # 场景文件
```

---

## 环境要求

- Unity **2021.3 LTS** 或更高版本
- Git（HybridCLR 安装依赖）
- C++ 编译环境（IL2CPP 打包依赖）

---

## 快速开始

### 1. 安装 HybridCLR

1. 确保已安装 **Git** 和 **C++ 编译环境**（Visual Studio Build Tools / Xcode 等）
2. 打开 Unity 项目后，执行菜单 `HybridCLR / Installer` 完成安装
3. 在 `HybridCLR / Settings / Hot Update Assemblies` 中添加 `Assembly-CSharp`，保存

### 2. 编辑器模式本地开发

在场景中找到挂载了 `GameEntry` 的 GameObject，在 Inspector 中：

| 字段 | 说明 |
|------|------|
| `IsEditorMode` | 勾选后跳过热更下载，直接读取本地 `AssetBundle/` 目录 |
| `IsRunABPackage` | 编辑器模式下是否使用 AB 包运行（需配合 `IsEditorMode`） |
| `IsEnableTimeScale` | 是否启用时间倍率调试（仅 Editor） |

### 3. 编写热更业务代码

在 `Assets/GameData/Scripts/Hotfix/` 目录下编写热更脚本，入口为 `HotUpdateMain.cs`：

```csharp
namespace GameData
{
    public class HotUpdateMain : MonoBehaviour
    {
        void Start()
        {
            // 在此初始化游戏逻辑
        }
    }
}
```

---

## 热更打包流程

```
1. HybridCLR / Generate / All        # 生成主包体依赖文件
2. File / Build Settings / Build      # 打包并生成 AotDll
3. HybridCLR / CompileDll / ActiveBuildTarget   # 编译热更 DLL
4. BFramework / Build AssetBundles    # 打 AB 包（含 MD5 校验文件）
5. 将 AssetBundle/ 文件夹上传至 CDN 服务器
6. 发布包体
```

> 详细说明请参考 `Assets/Editor/热更说明.txt`。
>
> **编辑器模式**下跳过下载流程，直接读取项目根目录下的 `AssetBundle/` 文件夹。

---

## 主要模块说明

### GameEntry — 游戏入口

`Assets/MainPackage/GameEntry.cs`

负责整个游戏的启动流程：初始化 UI 根节点与各层级、启动 `DowloadManager` 下载 AB 包、加载热更 DLL 并补充 AOT 元数据，最终实例化热更预制体 `HotUpdatePrefab.prefab`。

**UI 层级（E_UILevel）：**

| 层级 | 说明 |
|------|------|
| `Background` | 背景层 |
| `Common` | 通用 UI 层 |
| `Pop` | 弹窗层 |
| `Loading` | 加载层 |
| `Tips` | 提示层 |

---

### DowloadManager — 下载管理器

`Assets/MainPackage/Dowload/DowloadManager.cs`

原生实现的 AB 包增量下载器，无第三方依赖。

**工作流程：**

```
未开始 → 检查更新（下载 fileUpdateInfo.json）→ 逐包下载 → 下载完毕
```

**关键特性：**

- 通过 **MD5 + 文件大小** 双重比对跳过未变化的包，实现增量更新
- 每个 AB 包下载失败后自动**重试最多 3 次**
- 下载地址为空时自动回落到 `StreamingAssets` 目录（适合离线包内置场景）
- 通过 `IsDowloadEnd` 属性通知 `GameEntry` 下载完成

**配置：**

```csharp
// 设置 CDN 地址，留空则使用 StreamingAssets
public string DownloadUrl = "http://your-cdn.com/app/CDN";
```

---

### 日志系统（E_Log）

所有 `Log` 调用均通过 `[Conditional("UNITY_EDITOR")]` 修饰，**正式包零开销**。

| 类型 | 颜色 | 用途 |
|------|------|------|
| `Log` | 白色 | 普通日志 |
| `Framework` | 品红 | 框架内部日志 |
| `Proto` | 青色 | 网络协议日志 |
| `Error` | 红色 | 错误日志 |
| `Warring` | 黄色 | 警告日志 |
| `Custom` | 自定义 | 自定义颜色日志 |

---

### Editor 打包工具

通过菜单栏 `BFramework` 执行：

| 功能 | 说明 |
|------|------|
| **Build AssetBundles** | 一键打包 AB 包，自动处理热更 DLL、AOT DLL 复制、依赖关系 JSON 生成、MD5 校验文件生成 |

AB 包输出到项目根目录的 `AssetBundle/` 文件夹，同时生成 `fileUpdateInfo.json` 用于客户端增量校验。

---

## 依赖

| 依赖 | 用途 |
|------|------|
| [HybridCLR](https://github.com/focus-creative-games/hybridclr) | C# 热更新运行时 |
| [LitJson](https://github.com/LitJSON/litjson) | JSON 序列化（AB 包 MD5 信息） |

---

## 扩展版

如需完整的生产级框架（UI 框架、红点系统、网络通信、定时器、状态机、Excel 表格、对象池等），请访问：

👉 **[BFramework-Ex](https://github.com/ToxicStar8/BFramework-Ex)**

---

## License

MIT License