#Region "Microsoft.VisualBasic::22e0d756b8ab969dcd7a8b9bb5f999c8, Microsoft.VisualBasic.Core\src\Scripting\Expressions\ArrayIndex.vb"

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

    '   Total Lines: 75
    '    Code Lines: 48
    ' Comment Lines: 19
    '   Blank Lines: 8
    '     File Size: 2.58 KB


    '     Module ArrayIndex
    ' 
    '         Function: AsVector, TranslateIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Scripting.Expressions

    ''' <summary>
    ''' Expression text helper for vector index
    ''' </summary>
    Public Module ArrayIndex

        ''' <summary>
        ''' 表达式字符串只允许：``1, 2, 3, 4, 5``
        ''' </summary>
        ''' <param name="exp$"></param>
        ''' <returns></returns>
        <Extension> Public Function AsVector(exp$) As Double()
            Return exp.Split(","c) _
                .Select(AddressOf Trim) _
                .Select(Function(s) Val(s)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Translate the vector expression into a intger index vector
        ''' </summary>
        ''' <param name="exp$">
        ''' + ``1``, index=1
        ''' + ``1:8``, index=1, count=8
        ''' + ``1->8``, index from 1 to 8
        ''' + ``8->1``, index from 8 to 1
        ''' + ``1,2,3,4``, index=1 or  2 or 3 or 4
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function TranslateIndex(exp$) As Integer()
            If exp.IsPattern("-?\d+") Then
                Return {
                    CInt(exp)
                }
            ElseIf exp.IndexOf(":"c) > -1 Then
                Dim t = exp.Split(":"c)
                Dim from = CInt(t(Scan0))
                Dim count = CInt(t(1))
                Dim indcies%() = New Integer(count - 1) {}

                For i As Integer = 0 To count - 1
                    indcies(i) = i + from
                Next

                Return indcies
            ElseIf exp.IndexOf(","c) > -1 Then
                Dim t As Integer() = exp _
                    .Split(","c) _
                    .Select(Function(i) CInt(Val(i.Trim))) _
                    .ToArray
                Return t
            ElseIf InStr(exp, "->") > 0 Then
                Dim t As Integer() = Strings.Split(exp, "->") _
                    .Select(Function(s) CInt(s.Trim)) _
                    .ToArray
                Dim from = t(Scan0)
                Dim to% = t(1)
                Dim delta = If(from < [to], 1, -1)
                Dim out As New List(Of Integer)

                For i As Integer = from To [to] Step delta
                    out += i
                Next

                Return out
            Else
                Throw New SyntaxErrorException($"'{exp}' expression syntax error!")
            End If
        End Function
    End Module
End Namespace
