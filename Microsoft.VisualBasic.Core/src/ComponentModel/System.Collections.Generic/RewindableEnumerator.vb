#Region "Microsoft.VisualBasic::0491aaecf2db63ea83f9be8e90c6f1db, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\RewindableEnumerator.vb"

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

    '   Total Lines: 80
    '    Code Lines: 58 (72.50%)
    ' Comment Lines: 8 (10.00%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 14 (17.50%)
    '     File Size: 2.71 KB


    '     Class RewindableEnumerator
    ' 
    '         Properties: Current, Current_NonGeneric
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: MoveNext
    ' 
    '         Sub: Dispose, Previous, Reset
    ' 
    '     Module EnumeratorExtensions
    ' 
    '         Function: AsRewindable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection

    Public Class RewindableEnumerator(Of T)
        Implements IEnumerator(Of T)

        Private ReadOnly _enumerator As IEnumerator(Of T)
        Private _current As T
        Private _previousItem As T
        Private _hasPreviousItem As Boolean

        Public Sub New(enumerator As IEnumerator(Of T))
            If enumerator Is Nothing Then
                Throw New ArgumentNullException(NameOf(enumerator))
            End If
            _enumerator = enumerator
        End Sub

        Public ReadOnly Property Current As T Implements IEnumerator(Of T).Current
            Get
                Return If(_hasPreviousItem, _previousItem, _current)
            End Get
        End Property

        Private ReadOnly Property Current_NonGeneric As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            ' 如果之前调用了 Previous()，我们实际上处于“回退”状态
            ' 下一次 MoveNext 应该再次返回那个被回退的元素
            If _hasPreviousItem Then
                _hasPreviousItem = False
                Return True
            End If

            ' 否则，正常从底层枚举器获取下一个
            If _enumerator.MoveNext() Then
                _current = _enumerator.Current
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' 将枚举器回退一步，使得下一次 MoveNext() 再次返回当前元素。
        ''' </summary>
        Public Sub Previous()
            If _hasPreviousItem Then
                Throw New InvalidOperationException("只能回退一步。")
            End If

            ' 将当前元素缓存到 _previousItem
            ' 标记为有回退项
            _previousItem = _current
            _hasPreviousItem = True
        End Sub

        Public Sub Reset() Implements IEnumerator.Reset
            _enumerator.Reset()
            _hasPreviousItem = False
            _current = Nothing
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            _enumerator.Dispose()
        End Sub
    End Class

    Public Module EnumeratorExtensions

        <Extension()>
        Public Function AsRewindable(Of T)(enumerator As IEnumerator(Of T)) As RewindableEnumerator(Of T)
            Return New RewindableEnumerator(Of T)(enumerator)
        End Function
    End Module
End Namespace
