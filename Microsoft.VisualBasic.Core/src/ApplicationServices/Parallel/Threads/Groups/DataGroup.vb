#Region "Microsoft.VisualBasic::077af45831c994379f79d089e5016e46, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\Groups\DataGroup.vb"

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

'   Total Lines: 73
'    Code Lines: 54 (73.97%)
' Comment Lines: 5 (6.85%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 14 (19.18%)
'     File Size: 2.22 KB


'     Class TaggedGroupData
' 
'         Properties: Tag
' 
'         Function: ToString
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

Imports Microsoft.VisualBasic.Linq

Namespace Parallel

    Public MustInherit Class TaggedGroupData(Of T)
        Public Overridable Property Tag As T

        Public Overrides Function ToString() As String
            Return Tag.ToString
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
