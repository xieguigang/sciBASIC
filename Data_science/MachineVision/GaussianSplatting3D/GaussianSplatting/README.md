# 3D 高斯泼溅：场景重建与渲染

## 引言

传统的 3D 重建走「网格」路线：从多视角图像重建三角形网格，再贴图渲染。它的问题是——**细小结构（毛发、树叶、半透明物体）几乎无法网格化**。

**3D Gaussian Splatting** 换了一条路：不重建网格，而是用**一堆各向异性的高斯椭球**表示场景：

- 每个高斯有自己的位置、尺度、旋转、不透明度和颜色；
- 渲染就是把所有高斯按投影「泼溅」到图像平面（splatting）；
- 训练就是通过图像重建误差反向优化每个高斯的参数。

好处是：**渲染极快**（可实时）、**细节保留好**（粒子级表达），代价是表示形式不是传统网格。

## 核心能力与关键类型

全部类型位于根命名空间 `Microsoft.VisualBasic.MachineVision.GaussianSplatting3D`：

| 类型 | 职责 |
|---|---|
| `GaussianModel` | 场景表示：一组各向异性高斯（位置、尺度、旋转、不透明度、颜色） |
| `ImageLoader` | 读取多视角输入图像 |
| `Camera` | 定义光栅化使用的投影模型 |
| `GradientCalculator` | 计算图像损失对每个高斯参数的梯度 |
| `Optimizer` | 用 Adam 更新高斯参数 |
| `SplatRenderer` | 把高斯光栅化成图像 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D

' 1. 载入多视角图像并建立相机
Dim views = ImageLoader.LoadDirectory("./captures")

' 2. 初始化高斯场景
Dim model As New GaussianModel()
Call model.InitializeFrom(views)

' 3. 训练：图像损失 → 梯度 → Adam 更新
For i As Integer = 1 To iterations
    Dim loss = SplatRenderer.RenderLoss(model, views, camera:=Camera.Default)

    Call Optimizer.Step(model, GradientCalculator.Compute(model, loss))
Next

' 4. 渲染新视角
Dim image = SplatRenderer.Render(model, newCamera)
```

## 实现要点

- **为什么用椭球而不是球**：真实表面在不同方向上曲率不同；各向异性（可旋转的椭球）能贴合平面与细长结构，而球形只能表示"一团"。这是高斯泼溅能表达细节的根本原因。
- **可微光栅化**：渲染过程必须对参数可微，否则无法反传。`SplatRenderer` 与 `GradientCalculator` 的分工正体现这一点——渲染负责前向可见性与混合，梯度计算负责把图像误差映射回每个高斯。
- **Adam 优化的必要性**：高斯的位置、尺度、旋转与不透明度量纲差异巨大，普通 SGD 很难同时收敛；Adam 的逐参数自适应学习率在这里几乎是必需的。
- **与 `Landscape` 的分工**：`Landscape` 处理**网格与体素**（STL/OBJ/glTF → 体素 / SDF），本包处理**基于粒子的场景表示**；两者面向不同的下游任务，互为补充。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineVision.GaussianSplatting3D`
- TargetFramework：`net10.0`
- Tags：`scibasic;gaussian-splatting;3d-reconstruction;neural-rendering;novel-view-synthesis;computer-vision;adam-optimizer`
- 许可：GPL-3.0-or-later
