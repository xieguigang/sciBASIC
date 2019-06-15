#Region "Microsoft.VisualBasic::07bb13fb4b1929e273c991396589819c, Microsoft.VisualBasic.Core\My\JavaScript\Linq.vb"

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

    '     Module Linq
    ' 
    '         Function: (+2 Overloads) match, parseInt, Reduce, (+2 Overloads) Sort, test
    ' 
    '         Sub: match, (+2 Overloads) splice
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports r = System.Text.RegularExpressions.Regex
Imports System.Text.RegularExpressions


Namespace My.JavaScript

    Public Module Linq

        <Extension> Public Function test(pattern$, target$) As Boolean
            Return r.Match(target, pattern).Success
        End Function

        <Extension> Public Sub match(text$, pattern$, ByRef a$, ByRef b$, Optional ByRef c$ = Nothing)
            Dim parts = text.match(pattern)

            a = parts.ElementAtOrNull(0)
            b = parts.ElementAtOrNull(1)
            c = parts.ElementAtOrNull(2)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function parseInt(s As String, Optional radix% = 10) As Integer
            Return Convert.ToInt32(s, radix)
        End Function

        <Extension> Public Function match(text$, pattern$) As String()
            Return r.Match(text, pattern).Captures.AsQueryable.Cast(Of String).ToArray
        End Function

        <Extension> Public Function match(text$, pattern As r) As String()
            Return pattern.Match(text).Captures.AsQueryable.Cast(Of String).ToArray
        End Function

        <Extension>
        Public Sub splice(Of T)(array As T(), index As Integer, howmany As Integer, ParamArray items As T())
            Throw New NotImplementedException
        End Sub

        <Extension>
        Public Sub splice(Of T)(list As List(Of T), index As Integer, howmany As Integer, ParamArray items As T())
            Throw New NotImplementedException
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T">序列的类型</typeparam>
        ''' <typeparam name="V">序列进行降维之后的结果类型</typeparam>
        ''' <param name="seq"></param>
        ''' <param name="produce"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Reduce(Of T, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, V), init As V) As V
            For Each x As T In seq
                init = produce(init, x)
            Next

            Return init
        End Function

        <Extension>
        Public Function Sort(Of T)(seq As IEnumerable(Of T), comparer As Comparison(Of T)) As IEnumerable(Of T)
            With New List(Of T)(seq)
                Call .Sort(comparer)
                Return .AsEnumerable
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sort(Of T)(seq As IEnumerable(Of T), comparer As Func(Of T, T, Double)) As IEnumerable(Of T)
            Return seq.Sort(Function(x, y)
                                Dim d As Double = comparer(x, y)

                                If d > 0 Then
                                    Return 1
                                ElseIf d < 0 Then
                                    Return -1
                                Else
                                    Return 0
                                End If
                            End Function)
        End Function
    End Module
End Namespace
