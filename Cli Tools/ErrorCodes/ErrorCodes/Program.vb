#Region "Microsoft.VisualBasic::fa6e05c20997d0230874b6f6ceec884f, ..\visualbasic_App\Cli Tools\ErrorCodes\ErrorCodes\Program.vb"

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

Imports System.Text
Imports System.Text.RegularExpressions

Module Program

    Const ErrorCode As String = "^-?\d+L?"
    Const ErrorName As String = "\S+(_\S+)+"

    Sub Main()

        Dim file As String = App.CommandLine.Name
        Dim out As String = file.TrimFileExt & ".vb"
        Dim lines As String() = file.ReadAllLines
        Dim vb As New StringBuilder("Public Enum ErrorCodes As Long" & vbCrLf)
        Dim tmp As New StringBuilder
        Dim __code As String = Nothing
        Dim __name As String = Nothing

        For Each line As String In lines.Skip(2)
            Dim code As String = Regex.Match(line, ErrorCode, RegexOptions.Multiline).Value

            If Not String.IsNullOrEmpty(code) Then
                ' 第一行

                If Not String.IsNullOrEmpty(__name) Then
                    Call vb.AppendLine("'''<summary>")
                    For Each c As String In tmp.ToString.lTokens
                        Call vb.AppendLine("''' " & c)
                    Next
                    Call vb.AppendLine("'''</summary>")
                    Call vb.AppendLine($"{__name} = {__code}")
                    Call vb.AppendLine()
                End If

                Call tmp.Clear()

                Dim name As String = Regex.Match(line, ErrorName).Value
                line = line.Replace(code, "")
                line = line.Replace(name, "")
                line = line.Trim

                __code = code
                __name = name
                Call tmp.AppendLine(line)

            Else
                Call tmp.AppendLine(line.Trim)
            End If
        Next

        If Not String.IsNullOrEmpty(__name) Then
            Call vb.AppendLine("'''<summary>")
            For Each c As String In tmp.ToString.lTokens
                Call vb.AppendLine("''' " & c)
            Next
            Call vb.AppendLine("'''</summary>")
            Call vb.AppendLine($"{__name} = {__code}")
            Call vb.AppendLine()
        End If

        Call vb.AppendLine("End Enum")

        Call vb.SaveTo(out)
    End Sub
End Module

