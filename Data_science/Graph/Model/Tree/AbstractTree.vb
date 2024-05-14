#Region "Microsoft.VisualBasic::7020f89d381d6cb7cd1e28817b1e572e, Data_science\Graph\Model\Tree\AbstractTree.vb"

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

    '   Total Lines: 166
    '    Code Lines: 102
    ' Comment Lines: 42
    '   Blank Lines: 22
    '     File Size: 4.73 KB


    ' Class AbstractTree
    ' 
    '     Properties: Childs, Count, IsLeaf, IsRoot, Parent
    '                 QualifyName
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: (+2 Overloads) CountLeafs, EnumerateChilds, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

<DataContract>
Public Class AbstractTree(Of T As AbstractTree(Of T, K), K) : Inherits Vertex

    ''' <summary>
    ''' Childs table, key is the property <see cref="Vertex.Label"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property Childs As Dictionary(Of K, T)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 在序列化之中会需要忽略掉这个属性，否则会产生无限递归
    ''' </remarks>
    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public Property Parent As T

    Dim qualDeli$ = "."

    Default Public ReadOnly Property Child(key As K) As T
        Get
            Return Childs(key)
        End Get
    End Property

    ''' <summary>
    ''' Not null child count in this tree node.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 请注意，这个属性并不是返回的<see cref="Childs"/>的元素数量，
    ''' 而是返回当前树节点下的所有的子节点的数量
    ''' </remarks>
    ''' 
    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public ReadOnly Property Count As Integer
        Get
            Dim childs = Me.EnumerateChilds _
                .SafeQuery _
                .Where(Function(c) Not c Is Nothing) _
                .ToArray

            If childs.IsNullOrEmpty Then
                ' 自己算一个节点，所以数量总是1的
                Return 1
            Else
                Dim n% = childs.Length

                For Each node In childs
                    ' 如果节点没有childs，则会返回1，因为他自身就是一个节点
                    n += node.Count
                Next

                Return n
            End If
        End Get
    End Property

    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public Overridable ReadOnly Property QualifyName As String
        Get
            If Not Parent Is Nothing Then
                Return Parent.QualifyName & qualDeli & label
            Else
                Return label
            End If
        End Get
    End Property

    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public ReadOnly Property IsRoot As Boolean
        Get
            Return Parent Is Nothing
        End Get
    End Property

    <XmlIgnore>
    <DataIgnored>
    <IgnoreDataMember>
    Public ReadOnly Property IsLeaf As Boolean
        Get
            Return Childs.IsNullOrEmpty
        End Get
    End Property

    Sub New(Optional qualDeli$ = ".")
        Me.qualDeli = qualDeli
    End Sub

    Sub New()
        Call Me.New(".")
    End Sub

    ''' <summary>
    ''' Returns the values of <see cref="Childs"/>
    ''' </summary>
    ''' <param name="popAll">
    ''' just populate the childs in current tree node by default, 
    ''' and set this parameter to value true for populate all 
    ''' child nodes inside current tree node
    ''' </param>
    ''' <returns></returns>
    Public Function EnumerateChilds(Optional popAll As Boolean = False) As IEnumerable(Of T)
        If Not popAll Then
            Return Childs?.Values
        ElseIf Childs.IsNullOrEmpty Then
            Return New T() {Me}
        Else
            Dim all As New List(Of T) From {Me}

            For Each child As T In Childs.Values
                all.AddRange(child.EnumerateChilds(popAll:=True))
            Next

            Return all.Distinct
        End If
    End Function

    Public Overrides Function ToString() As String
        Return QualifyName
    End Function

    ''' <summary>
    ''' 计算出所有的叶节点的总数，包括自己的child的叶节点
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CountLeafs() As Integer
        Return CountLeafs(Me, 0)
    End Function

    ''' <summary>
    ''' 对某一个节点的所有的叶节点进行计数
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="count"></param>
    ''' <returns></returns>
    Public Shared Function CountLeafs(node As T, count As Integer) As Integer
        If node.IsLeaf Then
            count += 1
        End If

        For Each child As T In node.EnumerateChilds.SafeQuery
            count += child.CountLeafs()
        Next

        Return count
    End Function
End Class
