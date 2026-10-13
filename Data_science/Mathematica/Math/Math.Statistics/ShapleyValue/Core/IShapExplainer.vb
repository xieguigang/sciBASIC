#Region "Microsoft.VisualBasic::a7a7ef49c5f526e7fe4eaa17cf7d1d17, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\Core\IShapExplainer.vb"

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

    '   Total Lines: 44
    '    Code Lines: 8 (18.18%)
    ' Comment Lines: 29 (65.91%)
    '    - Xml Docs: 89.66%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 1.62 KB


    '     Interface IShapExplainer
    ' 
    '         Properties: Baseline, FeatureNames, Size
    ' 
    '         Function: Explain
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue

    ''' <summary>
    ''' The unified contract of a SHAP explainer.
    ''' 
    ''' 所有可解释模型的 SHAP 解释器都实现这个接口，从而使得
    ''' TreeSHAP（基于树结构的精确算法）与解析 SHAP（线性模型）可以
    ''' 通过同一套下游代码进行汇总、导出与可视化。
    ''' </summary>
    Public Interface IShapExplainer

        ''' <summary>
        ''' 模型特征的名字。其长度必须和 <see cref="Explain"/> 所返回的贡献向量的长度一致。
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property FeatureNames As String()

        ''' <summary>
        ''' 模型输出的期望值（基线值 / expected value）。
        ''' 
        ''' 对于任意样本 x，均有：``sum(Explain(x)) + Baseline = 模型输出``。
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Baseline As Double

        ''' <summary>
        ''' 模型的特征数量。
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Size As Integer

        ''' <summary>
        ''' 计算指定样本的逐特征 SHAP 贡献值。
        ''' 
        ''' 返回的向量长度与 <see cref="FeatureNames"/> 一致，并且**不包含**
        ''' 基线值 <see cref="Baseline"/>。
        ''' </summary>
        ''' <param name="x">特征向量，长度必须为 <see cref="Size"/></param>
        ''' <returns></returns>
        Function Explain(x As Double()) As Double()

    End Interface

End Namespace
