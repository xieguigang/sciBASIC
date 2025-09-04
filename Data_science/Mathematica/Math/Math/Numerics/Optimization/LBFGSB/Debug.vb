#Region "Microsoft.VisualBasic::6e0b243e81f4fb98d4ca6bdfcc835b5a, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\Debug.vb"

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
    '    Code Lines: 54 (72.00%)
    ' Comment Lines: 6 (8.00%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 15 (20.00%)
    '     File Size: 2.22 KB


    '     Class Debugger
    ' 
    '         Sub: (+6 Overloads) debug
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.C.CLangStringFormatProvider

Namespace Framework.Optimization.LBFGSB

    ''' <summary>
    ''' internal logger wrapper for echo debug message
    ''' </summary>
    Friend Class Debugger

        Public Shared flag As Boolean = False

        ' array and matrix cell formatting
        Private Const cellfmt As String = "%12s"
        ' number formatting
        Private Const numfmt As String = "%.6f"
        ' repeat character
        Private Const repeat As Integer = 10

        Public Shared Sub debug(c As Char, s As String)
            Dim b As String = New String(c, repeat)
            Console.WriteLine(b & " " & s & " " & b)
        End Sub

        Public Shared Sub debug(s As String)
            Console.WriteLine(s)
        End Sub

        Public Shared Sub debug(a As Double())
            debug(Nothing, a)
        End Sub

        Public Shared Sub debug(s As String, a As Double())
            If Not ReferenceEquals(s, Nothing) Then
                Console.Write(s)
            End If

            Console.Write("[")
            For Each v In a
                sprintf(cellfmt, sprintf(numfmt, v)).EchoLine
            Next
            Console.WriteLine("]")
        End Sub

        Public Shared Sub debug(m As Matrix)
            debug(Nothing, m)
        End Sub

        Public Shared Sub debug(s As String, m As Matrix)
            Dim shift As String

            If Not ReferenceEquals(s, Nothing) Then
                Console.Write(s & "[")
                shift = New String(" "c, s.Length + 1)
            Else
                Console.Write("[")
                shift = " "
            End If

            For row = 0 To m.rows - 1
                If row > 0 Then
                    Console.Write(shift)
                End If
                For col = 0 To m.cols - 1
                    sprintf(cellfmt, sprintf(numfmt, m.get(row, col))).EchoLine()
                Next
                If row = m.rows - 1 Then
                    Console.Write("]")
                End If
                Console.WriteLine()
            Next
        End Sub

    End Class

End Namespace
