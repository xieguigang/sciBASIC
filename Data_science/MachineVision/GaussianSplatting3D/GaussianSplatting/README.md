# 3D Gaussian Splatting — VB.NET 实现

基于 **3D Gaussian Splatting** 算法的多视图三维重建项目，使用 VB.NET 语言实现，核心数学运算基于自定义 `Tensor` 类。

## 项目结构

```
GaussianSplatting/
├── Tensor.vb               # 张量运算类（用户提供）
├── Camera.vb               # 相机模型（内参 + 外参 + JSON 加载）
├── GaussianModel.vb        # 3D 高斯参数化表示（位置/缩放/旋转/颜色/不透明度）
├── SplatRenderer.vb        # 可微光栅化渲染器（前向投影 + Alpha 混合）
├── GradientCalculator.vb   # 梯度计算（有限差分法）+ 自适应密度控制
├── Optimizer.vb            # Adam 优化器 + 余弦退火学习率调度
├── ImageLoader.vb          # PNG 图像加载/保存 + PLY 点云 I/O
├── Program.vb              # 主入口（训练流程 + 渲染 + 评估）
└── GaussianSplatting.vbproj
```

## 算法原理

### 3D 高斯溅射（3D Gaussian Splatting）

每个 3D 高斯由以下参数定义：

| 参数 | 维度 | 说明 |
|------|------|------|
| 位置 μ | 3 | 高斯中心在世界坐标系的位置 |
| 缩放 s | 3 | 各向异性缩放（log 空间，确保为正） |
| 旋转 q | 4 | 四元数（单位四元数） |
| 颜色 c | 3 | RGB（0-1） |
| 不透明度 α | 1 | logit 空间，通过 sigmoid 映射到 [0,1] |

**协方差矩阵**通过缩放和旋转参数化：
```
Σ = R(q) · diag(s)² · R(q)ᵀ
```
这种参数化保证 Σ 始终半正定。

### 渲染流程

1. **世界→相机变换**：`p_cam = R · p_world + t`
2. **相机→屏幕投影**：`u = fx·x/z + cx, v = fy·y/z + cy`
3. **协方差投影**：`Σ' = J · W · Σ · Wᵀ · Jᵀ`（W 为世界到相机变换，J 为投影雅可比）
4. **Alpha 混合**（按深度排序）：
   ```
   C(pixel) = Σᵢ cᵢ · αᵢ · Πⱼ<ᵢ(1 - αⱼ)
   αᵢ = oᵢ · exp(-0.5 · dᵀ · Σ'⁻¹ · d)
   ```

### 优化

- **损失函数**：MSE（渲染图像 vs 目标图像）
- **优化器**：Adam（β₁=0.9, β₂=0.999, ε=1e-8）
- **学习率调度**：余弦退火（0.015 → 0.001）
- **梯度计算**：中心差分法（解析反向传播的简化替代）
- **自适应密度控制**：
  - **克隆**：小高斯且梯度大 → 复制
  - **分裂**：大高斯且梯度大 → 分裂为两个
  - **剪枝**：不透明度过低 → 删除

## 使用方法

### 1. 生成测试数据（Python）

```bash
cd /home/z/my-project
python scripts/generate_scene.py
python scripts/make_contact_sheet.py
```

生成内容（位于 `download/test_data/`）：
- `view_00.png` ~ `view_11.png`：12 个视角的渲染图像（320×240）
- `cameras.json`：相机内参 + 外参
- `ground_truth.ply`：真值 3D 点云（7368 点）
- `scene_info.json`：场景元数据
- `contact_sheet.png`：所有视角的拼接预览图

### 2. 编译并运行（VB.NET）

```bash
# 使用 .NET SDK 编译
cd download/GaussianSplatting
dotnet build -c Release
dotnet run -- train

# 或使用 Mono 编译
vbnc -out:GaussianSplatting.exe *.vb -r:System.Drawing.dll
mono GaussianSplatting.exe train
```

### 3. 命令行参数

```
GaussianSplatting train    ' 训练 3D 高斯模型（默认）
GaussianSplatting render   ' 仅渲染已训练的模型
GaussianSplatting info     ' 显示测试数据和输出信息
GaussianSplatting help     ' 显示帮助
```

## 输出文件

训练完成后，输出位于 `download/output/`：

| 文件 | 说明 |
|------|------|
| `model_final.ply` | 最终 3D 高斯点云（PLY 格式） |
| `rendered_view_NN.png` | 每个视角的渲染结果 |
| `training_log.csv` | 训练日志（迭代/损失/PSNR/高斯数） |
| `metrics.json` | 最终评估指标 |

## 测试场景

合成场景包含以下物体（用于验证重建精度）：

1. **棋盘格地面**（10×10 单位）
2. **红色立方体**（中心，边长 1.5）
3. **蓝色球体**（立方体上方，半径 0.8）
4. **黄色圆环**（立方体右侧，外径 0.7）
5. **绿色圆锥**（立方体左侧，高 1.2）

相机在半径 6、高度 2 的圆周上均匀分布 12 个视角，FOV 50°。

## 技术说明

### 简化与权衡

本实现是 3D Gaussian Splatting 的**教学/验证版本**，与原始论文（Kerbl et al., 2023）有以下区别：

| 方面 | 原始论文 | 本实现 |
|------|----------|--------|
| 梯度计算 | 解析反向传播（自定义 autograd） | 有限差分法（数值梯度） |
| 球谐函数 | 3阶球谐（48维） | 直接 RGB（3维） |
| 光栅化 | CUDA 自定义内核 | 纯 VB.NET CPU 实现 |
| 速度 | 实时渲染 | 演示级速度 |

### 性能考虑

- 有限差分法的计算量为 O(N_params) 次前向渲染，仅适用于小规模验证
- 建议初始高斯数 ≤ 2000，迭代次数 ≤ 500
- 渲染分辨率建议 ≤ 320×240

## 依赖

- .NET 6.0+ 或 Mono
- NuGet 包：`System.Drawing.Common`、`System.Text.Json`

## 参考文献

1. Kerbl, B., Kopanas, G., Leimkühler, T., & Drettakis, G. (2023). *3D Gaussian Splatting for Real-Time Radiance Field Rendering*. ACM TOG.
2. Kingma, D. P., & Ba, J. (2014). *Adam: A Method for Stochastic Optimization*. ICLR.
