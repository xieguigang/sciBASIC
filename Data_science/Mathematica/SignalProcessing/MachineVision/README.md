# 机器视觉工具：形状、连通域、OCR 与跟踪

## 引言

二维图像分析可以拆成一条常见链路：

```text
图像 ─► 分割（找区域）─► 描述（形状特征）─► 匹配 / 识别 ─► 跟踪（跨帧关联）
```

本包提供这条链路上各环节的实现，并且**复用信号处理数学库**（变换、滤波、峰值检测），不重复造轮子。

## 核心能力

| 环节 | 方法 | 命名空间 |
|---|---|---|
| **形状匹配** | Procrustes 对齐、Frechet 距离 | 根命名空间 |
| **点集配准** | RANSAC（容忍离群点） | 根命名空间 |
| **分割** | 连通域标记（Connected Component Labelling） | `CCL` |
| **圆检测** | Hough 圆变换 | `HoughCircles` |
| **识别** | 文字 OCR（简单字形提取） | 根命名空间 |
| **跟踪** | 多目标跟踪 | 根命名空间 |

## 关键方法说明

- **Procrustes 分析**：在允许平移、旋转、缩放的前提下，把两个形状对齐并度量其残差，从而得到「形状相似度」——与位置 / 朝向 / 尺度无关。
- **Frechet 距离**：度量两条曲线之间的「最小同步行走距离」，比 Hausdorff 距离更能反映曲线顺序上的差异，适合轮廓比较。
- **Hough 圆变换**：把「找圆」转化为参数空间中的峰值检测；即使圆有部分遮挡或缺损，仍能检测出来。
- **连通域标记**：把二值图像中相邻的前景像素归为同一区域，是区域计数与形状分析的第一步。
- **RANSAC 配准**：随机采样最小点集估计变换，再用一致性检验筛选——因此对错误匹配（离群点）有很强的抵抗力。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.MachineVision
Imports Microsoft.VisualBasic.Math.MachineVision.CCL
Imports Microsoft.VisualBasic.Math.MachineVision.HoughCircles

' 1. 连通域标记
Dim labels = CCL.Label(binaryImage)

' 2. Hough 圆检测
Dim circles = HoughCircles.Detect(edgeImage, minRadius:=10, maxRadius:=50)

' 3. 形状匹配（Procrustes / Frechet）
Dim score = ShapeMatching.ProcrustesDistance(shapeA, shapeB)

' 4. 含离群点的点集配准
Dim transform = RANSAC.Align(sourcePoints, targetPoints)
```

## 实现要点

- **为什么形状匹配需要「对齐后再比较」**：直接比较坐标会把位置差异当成形状差异。Procrustes 先做最优刚体 / 相似变换，剩下的残差才是真正的形状差异。
- **Hough 变换的代价**：参数空间是三 / 四维的（圆心 x、y 与半径），空间划分越细、内存与时间开销越大；实践上通常结合边缘检测与梯度方向信息来收缩搜索范围。
- **OCR 的适用范围**：本包提供的是**简单字形提取**（适合规整的图标 / 仪表读数），而非通用文档识别；复杂排版应交给专业 OCR 引擎。
- **跟踪的本质**：跨帧把「同一目标」关联起来，核心是**匹配代价 + 运动预测**；本包实现的是多目标关联框架。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.MachineVision`
- TargetFramework：`net10.0`
- Tags：`scibasic;machine-vision;shape-matching;procrustes;frechet-distance;hough-transform;connected-component;ransac;ocr;tracking`
- 许可：GPL-3.0-or-later
