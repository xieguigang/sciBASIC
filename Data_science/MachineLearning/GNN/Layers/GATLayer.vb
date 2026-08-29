#Region "Microsoft.VisualBasic::4de5333dc925707f63ae1f594f7ce021, Data_science\MachineLearning\GNN\Layers\GATLayer.vb"

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

    '   Total Lines: 171
    '    Code Lines: 108 (63.16%)
    ' Comment Lines: 33 (19.30%)
    '    - Xml Docs: 51.52%
    ' 
    '   Blank Lines: 30 (17.54%)
    '     File Size: 6.34 KB


    ' Class GATLayer
    ' 
    '     Properties: InFeatures, OutFeatures
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, (+2 Overloads) Forward, GetGradients, GetParameters, LeakyReLU
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 图注意力层 (Graph Attention Layer, GAT)
''' 使用注意力机制聚合邻居信息
''' 论文: Graph Attention Networks (Veličković et al., ICLR 2018)
''' 
''' 核心思想：不同的邻居节点对中心节点的重要性不同，
''' 通过学习注意力权重来自适应地聚合邻居信息
''' </summary>
Public Class GATLayer
    Inherits Layer
    Private ReadOnly _numHeads As Integer
    Private ReadOnly _outFeaturesPerHead As Integer
    Private ReadOnly _leakyReluSlope As Single

    ' 可学习参数
    Private _W As Tensor  ' 特征变换权重 [inFeatures, numHeads * outFeaturesPerHead]
    Private _a As Tensor  ' 注意力权重 [numHeads, 2 * outFeaturesPerHead]

    ' 梯度
    Private _wGrad As Tensor
    Private _aGrad As Tensor

    ' 中间结果
    Private _lastInput As Tensor
    Private _transformedFeatures As Tensor
    Private _attentionWeights As Tensor
    Private _lastGraph As Graph

    Public ReadOnly Property InFeatures As Integer
    Public ReadOnly Property OutFeatures As Integer
        Get
            Return _numHeads * _outFeaturesPerHead
        End Get
    End Property

    ''' <summary>
    ''' 创建图注意力层
    ''' </summary>
    ''' <param name="inFeatures">输入特征维度</param>
    ''' <param name="outFeatures">输出特征维度（每个头）</param>
    ''' <param name="numHeads">注意力头数量</param>
    ''' <param name="leakyReluSlope">LeakyReLU的负斜率</param>
    Public Sub New(inFeatures As Integer, outFeatures As Integer, Optional numHeads As Integer = 1, Optional leakyReluSlope As Single = 0.2F, Optional name As String = Nothing)
        Me.InFeatures = inFeatures
        _outFeaturesPerHead = outFeatures
        _numHeads = numHeads
        _leakyReluSlope = leakyReluSlope
        MyBase.Name = If(name, $"GAT_{inFeatures}_{outFeatures}_heads{numHeads}")

        ' 初始化权重
        _W = Tensor.XavierInit(inFeatures, numHeads * outFeatures)
        _a = Tensor.RandomNormal(New Integer() {numHeads, 2 * outFeatures}, 0, 0.1F)

        ' 初始化梯度
        _wGrad = New Tensor(inFeatures, numHeads * outFeatures)
        _aGrad = New Tensor(numHeads, 2 * outFeatures)
    End Sub

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public Overloads Function Forward(input As Tensor, graph As Graph) As Tensor
        _lastInput = input
        _lastGraph = graph

        Dim numNodes = input.Shape(0)

        ' 步骤1: 特征变换
        ' H' = H * W, shape: [numNodes, numHeads * outFeatures]
        _transformedFeatures = input.MatMul(_W)

        ' 步骤2: 计算注意力系数
        ' 对于每条边，计算注意力权重
        _attentionWeights = New Tensor(numNodes, numNodes, _numHeads)

        For h = 0 To _numHeads - 1
            Dim headOffset = h * _outFeaturesPerHead

            For Each edge In graph.Edges
                Dim i = edge.Source
                Dim j = edge.Target

                ' 计算注意力分数 e_ij = LeakyReLU(a^T [Wh_i || Wh_j])
                Dim score As Single = 0
                For k = 0 To _outFeaturesPerHead - 1
                    score += _a(h, k) * _transformedFeatures(i, headOffset + k)
                    score += _a(h, _outFeaturesPerHead + k) * _transformedFeatures(j, headOffset + k)
                Next
                score = LeakyReLU(score, _leakyReluSlope)

                _attentionWeights(i, j, h) = score
            Next
        Next

        ' 步骤3: Softmax归一化（对每个节点的所有入边）
        ' α_ij = softmax_j(e_ij)
        Dim normalizedAttention = New Tensor(numNodes, numNodes, _numHeads)

        For h = 0 To _numHeads - 1
            For i = 0 To numNodes - 1
                ' 找到所有邻居并计算softmax
                Dim neighbors = graph.GetInNeighbors(i)
                If neighbors.Count = 0 Then Continue For

                Dim maxScore = Single.MinValue
                For Each j In neighbors
                    If _attentionWeights(j, i, h) > maxScore Then maxScore = _attentionWeights(j, i, h)
                Next

                Dim sumExp As Single = 0
                For Each j In neighbors
                    sumExp += CSng(std.Exp(_attentionWeights(j, i, h) - maxScore))
                Next

                For Each j In neighbors
                    Dim expScore As Single = std.Exp(_attentionWeights(j, i, h) - maxScore)
                    normalizedAttention(j, i, h) = expScore / sumExp
                Next
            Next
        Next

        ' 步骤4: 加权聚合
        ' h'_i = Σ_j α_ij * Wh_j
        Dim output = New Tensor(numNodes, _numHeads * _outFeaturesPerHead)

        For h = 0 To _numHeads - 1
            Dim headOffset = h * _outFeaturesPerHead

            For i = 0 To numNodes - 1
                Dim neighbors = graph.GetInNeighbors(i)
                For Each j In neighbors
                    Dim alpha = normalizedAttention(j, i, h)
                    For k = 0 To _outFeaturesPerHead - 1
                        output(i, headOffset + k) += alpha * _transformedFeatures(j, headOffset + k)
                    Next
                Next
            Next
        Next

        Return output
    End Function

    Private Function LeakyReLU(x As Single, slope As Single) As Single
        Return If(x > 0, x, slope * x)
    End Function

    Public Overrides Function Forward(input As Tensor) As Tensor
        Throw New InvalidOperationException("GAT层需要图结构，请使用Forward(input, graph)方法")
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Throw New InvalidOperationException("GAT层需要图结构")
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor) From {
            _W,
            _a
        }
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor) From {
            _wGrad,
            _aGrad
        }
    End Function
End Class

