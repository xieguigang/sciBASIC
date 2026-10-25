#Region "Microsoft.VisualBasic::677919254431461e80e75332104cdfcd, Data_science\MachineVision\GaussianSplatting3D\GaussianSplatting\NamespaceDoc.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 6
    '    Code Lines: 2 (33.33%)
    ' Comment Lines: 4 (66.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 199 B


    ' Class NamespaceDoc
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 3D Gaussian splatting：高斯模型、相机、图像 I/O、优化器与可微光栅化。
'''
''' 管线分为四段（对应四组类型）：
'''   1. <see cref="SplatProjection"/> —— 逐高斯屏幕投影（相机变换 / 协方差 / conic / 包围盒）
'''   2. <see cref="SplatRasterizer"/> —— 可见性剔除 → 深度升序排序 → tile 分箱
'''   3. <see cref="ISplatRasterBackend"/> / <see cref="CpuTileRasterBackend"/> —— 前向合成与解析反向
'''   4. <see cref="SplatProjectionGradient"/> —— 屏幕空间中间梯度 → 参数梯度
'''
''' 训练侧由 <see cref="SplatTrainer"/> 组织（梯度 <see cref="GradientCalculator"/>、
''' Adam <see cref="AdamOptimizer"/>、密度控制 <see cref="DensityController"/>），
''' 分阶段耗时由 <see cref="PerfTrace"/> 记录。
''' </summary>
Friend Class NamespaceDoc
End Class
