#Region "Microsoft.VisualBasic::cc9bcb12570190a8e54c30e0e6778bf9, Data_science\Mathematica\Math\Math\Algebra\LP\Extensions.vb"

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

    '   Total Lines: 81
    '    Code Lines: 64 (79.01%)
    ' Comment Lines: 5 (6.17%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (14.81%)
    '     File Size: 2.70 KB


    '     Module Extensions
    ' 
    '         Function: intToBinary, ParseType, subscriptN, unicodeSubscript
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace LinearAlgebra.LinearProgramming

    Module Extensions

        Public Function intToBinary(binary As Long, digits As Integer) As List(Of Byte)
            Dim temp As String = Convert.ToString(binary, 2)
            Dim foundDigits As Integer = temp.Length
            Dim returner As New List(Of Byte)

            For i As Integer = foundDigits To digits - 1
                returner.Add(0)
            Next

            returner.AddRange(temp.Select(Function(c) If(c = "1"c, CByte(1), CByte(0))))

            Return returner
        End Function

        ''' <summary>
        ''' Convert an integer into a multi-character subscript.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension> Public Function subscriptN(n As Integer) As String
            If Not LPP.USE_SUBSCRIPT_UNICODE Then
                Return "_" & n
            Else
                Return n.unicodeSubscript
            End If
        End Function

        <Extension> Private Function unicodeSubscript(n As Integer) As String
            Dim index As String = n.ToString
            Dim subscript As Char() = New Char(index.Length - 1) {}
            Dim c As Char

            For i As Integer = 0 To index.Length - 1
                Select Case n
                    Case 0
                        c = ChrW(&H2080)
                    Case 1
                        c = ChrW(&H2081)
                    Case 2
                        c = ChrW(&H2082)
                    Case 3
                        c = ChrW(&H2083)
                    Case 4
                        c = ChrW(&H2084)
                    Case 5
                        c = ChrW(&H2085)
                    Case 6
                        c = ChrW(&H2086)
                    Case 7
                        c = ChrW(&H2087)
                    Case 8
                        c = ChrW(&H2088)
                    Case Else
                        c = ChrW(&H2089)
                End Select

                subscript(i) = c
            Next

            Return New String(subscript)
        End Function

        <Extension>
        Friend Function ParseType(type As String) As OptimizationType
            Select Case LCase(type)
                Case "max", "maximize"
                    Return OptimizationType.MAX
                Case "min", "minimize"
                    Return OptimizationType.MIN
                Case Else
                    Throw New NotImplementedException(type)
            End Select
        End Function
    End Module
End Namespace
