---
name: llm-vbproj-rootnamespace-nuget-metadata
overview: 为 llm\llm.vbproj 确立 RootNamespace（Microsoft.VisualBasic.DeepLearning）并补齐 NuGet 描述性元数据与打包配置，使其与工作区内同族深度学习项目保持一致。
todos:
  - id: set-rootnamespace
    content: 在 llm.vbproj 中设置 RootNamespace=Microsoft.VisualBasic.DeepLearning、AssemblyName/AssemblyTitle=Microsoft.VisualBasic.DeepLearning.LLM 与 net10.0 目标框架
    status: completed
  - id: fill-nuget-metadata
    content: 补齐 NuGet 描述性元数据与包信息：Title、Description、PackageTags、PackageReleaseNotes、Authors、Company、Copyright、Version、License、Repository、PackageIcon、Readme 等
    status: completed
    dependencies:
      - set-rootnamespace
  - id: packaging-config
    content: 添加打包与输出配置：README.md 与 ..\vs_solutions\logo-knot.png 打包项、GeneratePackageOnBuild、IncludeSymbols/snupkg、nuget_release|x64 输出 ../.nuget/
    status: completed
    dependencies:
      - fill-nuget-metadata
  - id: add-project-references
    content: 补齐 ProjectReference：TensorFlow.vbproj 与 DeepLearning.NET6.vbproj，确保 Transformer.TensorOps 等类型可解析
    status: completed
    dependencies:
      - fill-nuget-metadata
  - id: verify-build-package
    content: 以 nuget_release|x64 执行 restore/build，校验类型全名、依赖解析及 nupkg 内含 README 与图标
    status: completed
    dependencies:
      - packaging-config
      - add-project-references
---

## Product Overview

为工作区中的 `llm\llm.vbproj`（使用 VB.NET 实现的纯托管 LLM 引擎）确立正确的 `RootNamespace`，并完整补齐其在 NuGet 包中的描述性元数据与打包配置，使其成为一个命名规范、可被构建与发布的独立类库包。

## Core Features

- 确立 `RootNamespace` 为 `Microsoft.VisualBasic.DeepLearning`，使全部源码中的 `Namespace LLM` 编译为 `Microsoft.VisualBasic.DeepLearning.LLM.*`，与同族项目（TensorFlow / DeepLearning）的命名约定保持一致。
- 设置 `AssemblyName` 与 `AssemblyTitle` 为 `Microsoft.VisualBasic.DeepLearning.LLM`。
- 补齐描述性元数据：`Title`、`Description`、`PackageTags`、`PackageReleaseNotes`、`PackageReadmeFile`。
- 补齐包信息：`Authors`、`Company`、`Copyright`、`Version`/`AssemblyVersion`、`PackageLicenseExpression`、`RepositoryUrl`/`RepositoryType`、`PackageProjectUrl`、`PackageIcon`、`PackageRequireLicenseAcceptance`、`Platforms`、`Configurations`。
- 添加打包项：将 `README.md` 与仓库图标 `logo-knot.png` 一并打入 NuGet 包。
- 添加 `GeneratePackageOnBuild`、`GenerateDocumentationFile`、`IncludeSymbols`/`SymbolPackageFormat`，并配置 `nuget_release` 输出目录为仓库根 `.nuget\`。
- 补齐对 `TensorFlow.vbproj` 与 `DeepLearning.NET6.vbproj` 的项目引用，保证 `Transformer.TensorOps` 等类型可解析。

## 边界与约束

- 仅修改 `llm\llm.vbproj` 一个文件；不修改任何 `.vb` 源码（源码中 `Namespace LLM` 保持不变）。
- 所有取值、字段命名与打包写法均对齐工作区既有 `*.vbproj` 约定，不引入新约定。

## 技术栈

- 语言/构建：VB.NET（VB.NET 源文件 + SDK 风格 `Microsoft.NET.Sdk` 项目文件）
- 目标框架：`net10.0`
- 包管理：NuGet（`PackageReference` 风格、`GeneratePackageOnBuild`、`snupkg` 符号包）
- 依赖项目：`Microsoft.VisualBasic.MachineLearning.TensorFlow`、`Microsoft.VisualBasic.DeepLearning`（提供 `Transformer.TensorOps`）

## 实现方案

### 命名空间决策

`llm\` 下 24 个源文件全部声明 `Namespace LLM`，而 `RootNamespace` 会被 VB 编译器前置到源码命名空间之前。按用户确认，取：

```
RootNamespace : Microsoft.VisualBasic.DeepLearning
源码声明       : Namespace LLM
最终类型全名   : Microsoft.VisualBasic.DeepLearning.LLM.<Type>
```

该取值与同族项目一致（`TensorFlow.vbproj` 用 `Microsoft.VisualBasic.MachineLearning.TensorFlow` 作为 RootNamespace+AssemblyName；`SNN.vbproj` 用 `Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork`），且源码无需任何改动。

### 项目引用决策（必须补齐）

源码存在跨项目类型依赖，缺少引用将无法编译：

- 全部文件依赖 `Microsoft.VisualBasic.MachineLearning.TensorFlow` → 需引用 `Data_science\MachineLearning\TensorFlow\TensorFlow.vbproj`。
- `SwiGLUFeedForward.vb`、`MoELayer.vb`、`LLMTensorOps.vb`、`CausalSelfAttention.vb` 通过 `Imports Microsoft.VisualBasic.MachineLearning` 调用 `Transformer.TensorOps.BatchedMatMul / BatchedMatMulBackward / HeNormalInit`（共 13 处）→ 需引用 `Data_science\MachineLearning\DeepLearning\DeepLearning.NET6.vbproj`。

依据：`llm\obj\project.assets.json` 历史 restore 图（第 471-482、537-541 行）与 `llm\bin\x64\nuget_release\net10.0\` 产物（含 `Microsoft.VisualBasic.DeepLearning.dll`、`Microsoft.VisualBasic.MachineLearning.TensorFlow.dll`、`Microsoft.VisualBasic.MachineLearning.dll` 等）证明这些引用此前存在，当前 vbproj 被清空后需恢复。

### 元数据与打包决策

严格复用同族项目（以 `TensorFlow.vbproj` 与 `word2vec.vbproj` 为主要模板）的字段命名、许可证与打包结构：

- 许可证：`GPL-3.0-or-later`（与 `llm\README.md` 及同族项目一致）。
- 图标：`logo-knot.png`，源文件位于 `vs_solutions\logo-knot.png`，从 `llm\` 出发相对路径为 `..\vs_solutions\logo-knot.png`。
- README：`README.md`（已存在，位于 `llm\`，本次仅打包，不改内容）。
- 输出目录：`llm\` 位于仓库根下一级，`nuget_release `配置的 `OutputPath` 应为 `../.nuget/`（`word2vec` 在两级子目录用 `../../.nuget/`，可据此推导）。
- 版本号：采用与同族项目当前构建批次一致的 `1.0.9758.20045`（`Version` 与 `AssemblyVersion`），保持同批次一致性。

### 方案完整文件内容（唯一改动目标 `llm\llm.vbproj`）

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>Microsoft.VisualBasic.DeepLearning</RootNamespace>
    <AssemblyName>Microsoft.VisualBasic.DeepLearning.LLM</AssemblyName>
    <TargetFrameworks>net10.0</TargetFrameworks>

    <Version>1.0.9758.20045</Version>
    <AssemblyVersion>1.0.9758.20045</AssemblyVersion>

    <Authors>xieguigang &lt;I@xieguigang.me&gt;</Authors>
    <Company>sciBASIC.NET Foundation</Company>
    <Copyright>Copyright (c) sciBASIC.NET Foundation</Copyright>

    <PackageProjectUrl>http://scibasic.net/</PackageProjectUrl>
    <RepositoryUrl>https://github.com/xieguigang/sciBASIC</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
    <PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>

    <Platforms>AnyCPU;x64</Platforms>
    <Configurations>Debug;Release;nuget_release</Configurations>

    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    <IncludeSymbols>True</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    <PackageIcon>logo-knot.png</PackageIcon>
    <PackageReadmeFile>README.md</PackageReadmeFile>

    <Title>Decoder-only LLM Engine: DeepSeekMoE, KV Cache and Function Calling</Title>
    <AssemblyTitle>Microsoft.VisualBasic.DeepLearning.LLM</AssemblyTitle>
    <Description>Pure managed decoder-only language model engine for sciBASIC#: DeepSeek-style mixture-of-experts with isolated shared experts and node-limited routing, prefill/incremental decoding KV cache with GQA/MQA, RoPE, RMSNorm and SwiGLU decoder blocks, AdamW training with masked cross-entropy, and schema-constrained function calling driven by a multi-turn agent loop.</Description>
    <PackageTags>scibasic;llm;decoder-only;transformer;mixture-of-experts;deepseek-moe;kv-cache;rope;rmsnorm;swiglu;gqa;mqa;function-calling;constrained-decoding;tool-calling;agent-loop;adamw;language-model</PackageTags>
    <PackageReleaseNotes>The decoder-only language model engine of sciBASIC#. It complements the existing encoder-decoder Transformer namespace and deliberately keeps the same engineering conventions: every component exposes a LastCache/Cache snapshot, backward passes are hand written against the cached forward values, gradients accumulate in place into a same-shaped tensor, and MakeTrainingStep applies the update and clears gradients. The model stack is a token embedding, a stack of decoder blocks each running RMSNorm, causal self attention and either a dense SwiGLU feed forward or a mixture of experts, a final RMSNorm and a weight tied output projection. RoPE supplies rotary position encoding. Mixture of experts follows DeepSeekMoE: fine grained expert segmentation with isolated shared experts, sigmoid Top-K routing with renormalised gate weights, auxiliary-loss-free load balancing through a bias update, and node limited routing. The KV cache supports prefill plus incremental decoding with GQA/MQA through nKvHeads, cutting the per-step attention cost from O(t^2) to O(t); an uncached full-sequence path is kept as a correctness reference. Training is provided by LMTrainer and AdamW with decoupled weight decay, masked cross entropy that excludes tool-result tokens during SFT, global L2 gradient clipping through LLMTensorOps, and parameter persistence through Save/Load. Function calling is covered end to end by JsonSchema, a ConstrainedDecoder that compiles the schema into a finite state machine and masks illegal logits with -inf, ToolCallProtocol, ToolRegistry and an AgentLoop that drives the multi-turn call, execute and refill cycle. TokenizerVocabulary, TokenStream and TextCodec handle tokenisation, and the runtime is the pure managed Microsoft.VisualBasic.MachineLearning.TensorFlow engine, whose CUDA backend can be registered at runtime for acceleration.</PackageReleaseNotes>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='nuget_release|x64'">
    <PlatformTarget>x64</PlatformTarget>
    <RemoveIntegerChecks>true</RemoveIntegerChecks>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <OutputPath>../.nuget/</OutputPath>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md">
      <Pack>True</Pack>
      <PackagePath>\</PackagePath>
    </None>
    <None Include="..\vs_solutions\logo-knot.png">
      <Pack>True</Pack>
      <PackagePath>\</PackagePath>
    </None>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Data_science\MachineLearning\TensorFlow\TensorFlow.vbproj" />
    <ProjectReference Include="..\Data_science\MachineLearning\DeepLearning\DeepLearning.NET6.vbproj" />
  </ItemGroup>

</Project>
```

## 实现说明（执行要点）

- **单文件改动**：本次仅落盘 `llm\llm.vbproj`，`.vb` 源码与 `README.md` 内容均不改动，改动半径极小。
- **不破坏既有构建**：`RootNamespace` 改变只影响编译期类型全名；源码内部使用未限定的 `LLM.*` 与 `Transformer.TensorOps`（经 `Imports Microsoft.VisualBasic.MachineLearning` 解析），因此不受影响。原先 `bin\x64\nuget_release` 下的旧产物与新 `OutputPath`（`../.nuget/`）不冲突。
- **跨平台/平台配置**：`nuget_release|x64` 条件依赖 `<Platforms>` 含 `x64`；`Debug`/`Release` 走 `AnyCPU`。`nuget.slnx` 已将多个解决方案配置映射到本项目的 Release/Debug，无需新增项目级配置项。
- **性能/资源**：纯配置文件改动，无运行时开销；唯一需要考虑的是恢复 `DeepLearning.NET6.vbproj` 引用会带动其传递依赖（DataMining、Math、Core 等）参与构建，与历史 restore 图一致，属预期行为。
- **验证策略**：改后执行一次带 `nuget_release|x64` 的 restore/build，确认依赖解析成功、生成的包名为 `Microsoft.VisualBasic.DeepLearning.LLM.<version>.nupkg`、包内含 `README.md` 与 `logo-knot.png`、且生成 `Microsoft.VisualBasic.DeepLearning.LLM.xml` 文档。

## 目录结构（受影响的文件）

```
sciBASIC#/
├── llm/
│   ├── llm.vbproj   # [MODIFY] 唯一改动文件。确立 RootNamespace=Microsoft.VisualBasic.DeepLearning；新增 AssemblyName/AssemblyTitle；补齐 NuGet 描述性元数据（Title/Description/PackageTags/PackageReleaseNotes/PackageReadmeFile）与包信息（Authors/Company/Copyright/Version/PackageLicenseExpression/RepositoryUrl/PackageProjectUrl/PackageIcon 等）；新增 README.md 与 logo-knot.png 打包项；新增 nuget_release|x64 输出配置（../.nuget/）；新增对 TensorFlow.vbproj 与 DeepLearning.NET6.vbproj 的 ProjectReference。源码中的 Namespace LLM 保持不变，最终类型全名为 Microsoft.VisualBasic.DeepLearning.LLM.*。
│   └── README.md    # [REFERENCE] 已存在，仅被打包，不修改内容
└── vs_solutions/
    └── logo-knot.png  # [REFERENCE] 已存在，仅被打包，不修改
```