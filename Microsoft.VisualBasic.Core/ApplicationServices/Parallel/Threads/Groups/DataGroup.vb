#Region "Microsoft.VisualBasic::30597085f169577662e128cc9d9bf8e7, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\Groups\DataGroup.vb"

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

    '     Class TaggedGroupData
    ' 
    '         Properties: Tag
    ' 
    '         Function: ToString
    ' 
    '     Class GroupListNode
    ' 
    '         Properties: Count, Group, InitReads
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '     Class GroupResult
    ' 
    '         Properties: Count, Group, Tag
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Parallel

    Public MustInherit Class TaggedGroupData(Of T_TAG)
        Public Overridable Property Tag As T_TAG

        Public Overrides Function ToString() As String
            Return Tag.ToString
        End Function
    End Class

    Public Class GroupListNode(Of T, T_TAG) : Inherits TaggedGroupData(Of T_TAG)
        Implements IEnumerable(Of T)

        Dim _Group As List(Of T)

        Public Property Group As List(Of T)
            Get
                Return _Group
            End Get
            Set(value As List(Of T))
                _Group = value
                If value.IsNullOrEmpty Then
                    _InitReads = 0
                Else
                    _InitReads = value.Count
                End If
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return Group.Count
            End Get
        End Property

        ''' <summary>
        ''' 由于<see cref="Group"/>在分组之后的后续的操作的过程之中元素会发生改变，
        ''' 所以在这个属性之中存储了在初始化<see cref="Group"/>列表的时候的原始的列表之中的元素的个数以满足一些其他的算法操作
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property InitReads As Integer

        Public Overrides Function ToString() As String
            Return MyBase.ToString & $" // {NameOf(InitReads)}:={InitReads},  current:={Count}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each obj In Group
                Yield obj
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    ''' <summary>
    ''' 分组操作的结果
    ''' </summary>
    ''' <typeparam name="T">Group的元素的类型</typeparam>
    ''' <typeparam name="Itag">Group的Key的类型</typeparam>
    Public Class GroupResult(Of T, Itag) : Inherits TaggedGroupData(Of Itag)
        Implements IEnumerable(Of T)
        Implements IGrouping(Of Itag, T)

        Public Overrides Property Tag As Itag Implements IGrouping(Of Itag, T).Key
        Public Property Group As T()
            Get
                Return __list.ToArray
            End Get
            Set(value As T())
                Call __list.Clear()

                If Not value.IsNullOrEmpty Then
                    Call __list.AddRange(value)
                End If
            End Set
        End Property

        ReadOnly __list As New List(Of T)

        Public ReadOnly Property Count As Integer
            Get
                Return Group.Length
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(tag As Itag, data As IEnumerable(Of T))
            Me.Tag = tag
            Me.Group = data.ToArray
        End Sub

        Public Sub Add(x As T)
            __list.Add(x)
        End Sub

        Public Sub Add(source As IEnumerable(Of T))
            __list.AddRange(source)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For i As Integer = 0 To Group.Length - 1
                Yield Group(i)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
