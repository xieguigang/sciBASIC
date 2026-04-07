#Region "Microsoft.VisualBasic::2ae0b7a797902656d883f8108429bc18, Data_science\Mathematica\Math\DataFrame\NamedSparseMatrix.vb"

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

    '   Total Lines: 88
    '    Code Lines: 55 (62.50%)
    ' Comment Lines: 21 (23.86%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 12 (13.64%)
    '     File Size: 2.64 KB


    ' Class NamedSparseMatrix
    ' 
    '     Properties: [Dim]
    ' 
    '     Function: CheckElement, ContainsNode, GetDirectedValue, ToString
    ' 
    '     Sub: SetValue
    ' 
    ' /********************************************************************************/

#End Region

Public Class NamedSparseMatrix

    ''' <summary>
    ''' 使用 Dictionary 存储稀疏数据
    ''' </summary>
    ReadOnly data As New Dictionary(Of String, Dictionary(Of String, Double))
    ''' <summary>
    ''' 维护 ID 到索引的映射列表
    ''' </summary>
    ReadOnly names As New List(Of String)
    ''' <summary>
    ''' 快速查找 ID 是否存在
    ''' </summary>
    ReadOnly hash As New HashSet(Of String)

    Default Public Property Value(i As String, j As String) As Double
        Get
            Return GetDirectedValue(i, j)
        End Get
        Set
            Call SetValue(i, j, Value)
        End Set
    End Property

    Default Public Property Value(i As Integer, j As Integer) As Double
        Get
            Return Me(names(i), names(j))
        End Get
        Set
            Me(names(i), names(j)) = Value
        End Set
    End Property

    Public ReadOnly Property [Dim] As Integer
        Get
            Return names.Count
        End Get
    End Property

    ''' <summary>
    ''' 检查指定的节点ID是否存在于矩阵维度中
    ''' </summary>
    Public Function ContainsNode(nodeId As String) As Boolean
        Return hash.Contains(nodeId)
    End Function

    ''' <summary>
    ''' Check of the edge connection is existsed inside this matrix?
    ''' </summary>
    ''' <param name="i"></param>
    ''' <param name="j"></param>
    ''' <returns>检查指定的边 (i -> j) 是否显式定义（非零值或在字典中存在键）</returns>
    Public Function CheckElement(i As String, j As String) As Boolean
        Return hash.Contains(i) AndAlso hash.Contains(j)
    End Function

    Public Function GetDirectedValue(i As String, j As String) As Double
        If data.ContainsKey(i) AndAlso data(i).ContainsKey(j) Then
            Return data(i)(j)
        End If
        Return 0.0
    End Function

    Public Sub SetValue(i As String, j As String, value As Double)
        ' 修复逻辑：基于 hash 判断是否为新节点，而不是基于 data
        If Not hash.Contains(i) Then
            names.Add(i)
            hash.Add(i)
        End If

        If Not hash.Contains(j) Then
            names.Add(j)
            hash.Add(j)
        End If

        ' 初始化行字典
        If Not data.ContainsKey(i) Then
            data.Add(i, New Dictionary(Of String, Double))
        End If

        ' 赋值
        data(i)(j) = value
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{[Dim]} x {[Dim]}]"
    End Function
End Class

