#Region "Microsoft.VisualBasic::5d8e8f9f7616aa8fce94ff9e91cc56b4, Data_science\DataMining\DataMining\ComponentModel\Normalizer\Standardizer.vb"

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

    '   Total Lines: 76
    '    Code Lines: 43 (56.58%)
    ' Comment Lines: 19 (25.00%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 14 (18.42%)
    '     File Size: 2.20 KB


    ' Class Standardizer
    ' 
    '     Properties: mean, std
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Load, Transform
    ' 
    '     Sub: Fit, Save
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdf = System.Math

''' <summary>
''' 每维度特征的 z-score 标准化器。
''' 
''' 训练阶段按特征维计算均值/标准差，推理阶段复用同一套参数，
''' 解决各维度特征量纲差异大导致的梯度不稳定问题。
''' 零方差维度标准差置 1，避免除零。
''' </summary>
Public Class Standardizer

    Public Property mean As Double()
    Public Property std As Double()

    Sub New()
    End Sub

    ''' <summary>
    ''' 用一组训练样本拟合均值与标准差
    ''' </summary>
    Public Sub Fit(samples As IEnumerable(Of Double()))
        Dim list = samples.ToList()

        If list.Count = 0 Then
            mean = Nothing
            std = Nothing
            Return
        End If

        Dim dimSize = list(0).Length
        mean = New Double(dimSize - 1) {}
        std = New Double(dimSize - 1) {}

        For d As Integer = 0 To dimSize - 1
            Dim offset As Integer = d
            Dim m = list.Average(Function(v) v(offset))
            Dim variance = list.Average(Function(v) (v(offset) - m) * (v(offset) - m))
            mean(d) = m
            std(d) = If(variance <= 0, 1.0, stdf.Sqrt(variance))
        Next
    End Sub

    ''' <summary>
    ''' 对单个特征向量做标准化：(x - mean) / std
    ''' </summary>
    Public Function Transform(input As Double()) As Double()
        If mean Is Nothing OrElse std Is Nothing Then
            Return input
        End If

        Dim out = New Double(input.Length - 1) {}

        For i As Integer = 0 To input.Length - 1
            out(i) = (input(i) - mean(i)) / std(i)
        Next

        Return out
    End Function

    ''' <summary>
    ''' 保存标准化参数到 JSON 文件
    ''' </summary>
    Public Sub Save(path As String)
        Call File.WriteAllText(path, Me.GetJson)
    End Sub

    ''' <summary>
    ''' 从 JSON 文件加载标准化参数
    ''' </summary>
    Public Shared Function Load(path As String) As Standardizer
        Dim snap = File.ReadAllText(path).LoadJSON(Of Standardizer)
        Return snap
    End Function

End Class
