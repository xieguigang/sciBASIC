#Region "Microsoft.VisualBasic::8a64dedab7fbde77e5f010010e973a02, Microsoft.VisualBasic.Core\src\Language\Core\Strings.vb"

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

    '   Total Lines: 57
    '    Code Lines: 44 (77.19%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (22.81%)
    '     File Size: 1.40 KB


    '     Module Strings
    ' 
    '         Function: InStr, LCase, Len, Mid, Trim
    '                   UCase, Val
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

#If NETSTANDARD Then
Namespace Language

    Public Module Strings

        Public Function LCase(s As String) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.ToLower
            End If
        End Function

        Public Function UCase(s As String) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.ToUpper
            End If
        End Function

        Public Function Len(s As String) As Integer
            If s Is Nothing Then
                Return 0
            Else
                Return s.Length
            End If
        End Function

        Public Function Trim(s As String) As String
            If s Is Nothing Then
                Return ""
            Else
                Return s.Trim(ASCII.CR, ASCII.LF, ASCII.TAB, " "c)
            End If
        End Function

        Public Function Val(x As Object) As Double
            If x Is Nothing Then
                Return 0
            End If


        End Function

        Public Function InStr(s As String, find As String) As Integer

        End Function

        Public Function Mid(s As String, start As Integer, Optional len As Integer = -1) As String

        End Function
    End Module
End Namespace
#End If
