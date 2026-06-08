#Region "Microsoft.VisualBasic::d0fa3cc1606ad92fba0e98cb2cc31f02, Data\BinaryData\BinaryData\PickleSerializer\PythonSet.vb"

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

    '   Total Lines: 43
    '    Code Lines: 28 (65.12%)
    ' Comment Lines: 5 (11.63%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (23.26%)
    '     File Size: 1.45 KB


    '     Class PythonSet
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Contains, GetEnumerator, GetEnumerator1, ToString
    ' 
    '         Sub: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Pickle

    ''' <summary>
    ''' 表示 Python 的 set 类型。Python 的 set 是无序且元素唯一的集合，
    ''' 类似于 .NET 的 HashSet，但提供与 Python 互操作所需的类型标识。
    ''' 支持 frozenset 和 set 两种语义（此类型不区分可变/不可变）。
    ''' </summary>
    Public Class PythonSet
        Implements IEnumerable(Of Object)

        Private ReadOnly _items As New HashSet(Of Object)

        Public Sub New()
        End Sub

        Public Sub Add(item As Object)
            _items.Add(item)
        End Sub

        Public Function Contains(item As Object) As Boolean
            Return _items.Contains(item)
        End Function

        Public ReadOnly Property Count As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of Object) Implements IEnumerable(Of Object).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Overrides Function ToString() As String
            Return "{" & String.Join(", ", _items.Select(Function(o) If(o Is Nothing, "None", o.ToString()))) & "}"
        End Function
    End Class

End Namespace
