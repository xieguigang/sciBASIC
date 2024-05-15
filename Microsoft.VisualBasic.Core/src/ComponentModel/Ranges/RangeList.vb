#Region "Microsoft.VisualBasic::db823fc685023f06205b5c43d4ba9f77, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeList.vb"

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

    '   Total Lines: 95
    '    Code Lines: 78
    ' Comment Lines: 1
    '   Blank Lines: 16
    '     File Size: 3.35 KB


    '     Class RangeList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Select], GetEnumerator, IEnumerable_GetEnumerator, (+2 Overloads) SelectValue
    ' 
    '         Sub: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges

    Public Class RangeList(Of T As IComparable, V) : Implements IEnumerable(Of RangeTagValue(Of T, V))

        Dim buffer As New List(Of RangeTagValue(Of T, V))

        Public ReadOnly Iterator Property Values As IEnumerable(Of V)
            Get
                For Each x As RangeTagValue(Of T, V) In Me
                    Yield x.Value
                Next
            End Get
        End Property

        Public ReadOnly Iterator Property Keys As IEnumerable(Of Range(Of T))
            Get
                For Each x As RangeTagValue(Of T, V) In Me
                    Yield x
                Next
            End Get
        End Property

        Sub New(Optional capacity% = 128)
            If capacity > 0 Then
                buffer = New List(Of RangeTagValue(Of T, V))(capacity)
            Else
                ' do nothing, internal used only
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(range As RangeTagValue(Of T, V))
            Call buffer.Add(range)
        End Sub

        Public Function [Select](x As T) As RangeTagValue(Of T, V)
            Dim LQuery = LinqAPI.DefaultFirst(Of RangeTagValue(Of T, V)) _
 _
                () <= From r As RangeTagValue(Of T, V)
                      In Me.AsQueryable
                      Where r.IsInside(x)
                      Select r

            Return LQuery
        End Function

        Public Function SelectValue(x As T, Optional [throw] As Boolean = True, Optional ByRef success As Boolean = False) As V
            Dim n As RangeTagValue(Of T, V) = [Select](x)

            If n Is Nothing Then
                If [throw] Then
                    Throw New DataException($"{x.GetJson} is not in any ranges!")
                Else
                    Return Nothing
                End If
            Else
                success = True
                Return n.Value
            End If
        End Function

        Public Function SelectValue(x As T, [default] As Func(Of T, V)) As V
            Dim success As Boolean = False
            Dim v As V = SelectValue(x, [throw]:=False, success:=success)

            If success Then
                Return v
            Else
                Return [default](x)
            End If
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of RangeTagValue(Of T, V)) Implements IEnumerable(Of RangeTagValue(Of T, V)).GetEnumerator
            For Each x In buffer
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(buffer As RangeTagValue(Of T, V)()) As RangeList(Of T, V)
            Return New RangeList(Of T, V)(-1) With {
                .buffer = buffer.AsList
            }
        End Operator
    End Class
End Namespace
