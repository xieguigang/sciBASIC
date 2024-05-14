#Region "Microsoft.VisualBasic::8f8c6151177cfe095a53de8901e19930, Microsoft.VisualBasic.Core\src\Net\Tcp\IPTools\AddressEnumerator.vb"

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

    '   Total Lines: 71
    '    Code Lines: 48
    ' Comment Lines: 14
    '   Blank Lines: 9
    '     File Size: 2.14 KB


    '     Module AddressEnumerator
    ' 
    '         Function: (+3 Overloads) EnumerateAddress, Enumerates
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Net

    Public Module AddressEnumerator

        Public Iterator Function EnumerateAddress(a$, b$, c$) As IEnumerable(Of String)
            For i As Integer = 0 To 255
                Yield $"{a}.{b}.{c}.{i}"
            Next
        End Function

        Public Iterator Function EnumerateAddress(a$, b$) As IEnumerable(Of String)
            For i As Integer = 0 To 255
                For Each ip$ In EnumerateAddress(a, b, i)
                    Yield ip
                Next
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expression$">
        ''' Example as:
        ''' 
        ''' ```
        ''' 192.168.100.*
        ''' 192.168.*.233
        ''' 192.1-120.*.*
        ''' ```
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Iterator Function EnumerateAddress(expression$) As IEnumerable(Of String)
            Dim t$() = expression.Split("."c)

            For Each a$ In Enumerates(t(0))
                For Each b$ In Enumerates(t(1))
                    For Each c$ In Enumerates(t(2))
                        For Each d$ In Enumerates(t(3))
                            Yield $"{a}.{b}.{c}.{d}"
                        Next
                    Next
                Next
            Next
        End Function

        Private Iterator Function Enumerates(expression$) As IEnumerable(Of String)
            Dim a%, b%

            If expression = "*" Then
                a = 0
                b = 255
            ElseIf Regex.Match(expression, "\d+-\d+").Value = expression Then
                Dim t$() = expression.Split("-"c)
                a = CInt(Val(t(Scan0)))
                b = CInt(Val(t(1)))
            Else
                a = CInt(Val(expression))
                b = a
            End If

            For i As Integer = a To b
                Yield CStr(i)
            Next
        End Function
    End Module
End Namespace
