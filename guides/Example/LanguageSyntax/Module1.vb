#Region "Microsoft.VisualBasic::6ef887e10f951ecee6311703fe89f5a3, ..\visualbasic_App\guides\Example\LanguageSyntax\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Module Module1

    Sub Main()
        Dim i As int = 0

        Call i.RangeDescription.__DEBUG_ECHO
        Call (++i).RangeDescription.__DEBUG_ECHO
        Call (i = 150).RangeDescription.__DEBUG_ECHO
        Call i.RangeDescription.__DEBUG_ECHO
        Call (i = 250).RangeDescription.__DEBUG_ECHO
        Call i.RangeDescription.__DEBUG_ECHO

        Pause()
    End Sub

    Private Iterator Function allLines() As IEnumerable(Of String)
        Yield "adasd"
        Yield "2342sdas"
        Yield "zdaasda"
        Yield Nothing
    End Function

    <Extension>
    Public Function RangeDescription(x As Integer) As String
        Return New int(x).RangeDescription
    End Function

    <Extension>
    Public Function RangeDescription(x As int) As String
        If 0 < x <= 100 Then
            Return "0-100"
        ElseIf 100 < x <= 200 Then
            Return "100-200"
        ElseIf 200 < x <= 300 Then
            Return "200-300"
        Else
            Return "undefined"
        End If
    End Function
End Module
