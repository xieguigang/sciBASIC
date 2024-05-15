#Region "Microsoft.VisualBasic::99fc674cdee403b48fc7cc46fb6ea0ae, mime\application%pdf\PdfReader\Tokenizer\TokenStringLiteral.vb"

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

    '   Total Lines: 92
    '    Code Lines: 73
    ' Comment Lines: 5
    '   Blank Lines: 14
    '     File Size: 3.80 KB


    '     Class TokenStringLiteral
    ' 
    '         Properties: Resolved, ResolvedAsBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BytesToString, RawStringToResolved
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace PdfReader
    Public Class TokenStringLiteral
        Inherits TokenString

        Public Sub New(raw As String)
            MyBase.New(raw)
        End Sub

        Public Overrides ReadOnly Property Resolved As String
            Get
                Return RawStringToResolved(Raw)
            End Get
        End Property

        Public Overrides ReadOnly Property ResolvedAsBytes As Byte()
            Get
                Return Encoding.ASCII.GetBytes(Raw)
            End Get
        End Property

        Public Overrides Function BytesToString(bytes As Byte()) As String
            Return RawStringToResolved(Encoding.ASCII.GetString(bytes))
        End Function

        Private Function RawStringToResolved(raw As String) As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim last = raw.Length
            Dim first = 0

            For i = 0 To last - 1
                ' If we encounter an escape '\' that is not in the last character position
                If raw(i) = "\"c AndAlso i < last - 1 Then
                    Select Case raw(i + 1)
                        Case "n"c, "r"c, "t"c, "b"c, "f"c
                            ' Convert from two characters to actual escaped character
                            sb.Append(raw.Substring(first, i - first))

                            Select Case raw(i + 1)
                                Case "n"c
                                    sb.Append(Microsoft.VisualBasic.Constants.vbLf)
                                Case "r"c
                                    sb.Append(Microsoft.VisualBasic.Constants.vbCr)
                                Case "t"c
                                    sb.Append(Microsoft.VisualBasic.Constants.vbTab)
                                Case "b"c
                                    sb.Append(Microsoft.VisualBasic.Constants.vbBack)
                                Case "f"c
                                    sb.Append(Microsoft.VisualBasic.Constants.vbFormFeed)
                            End Select

                            i += 1
                            first = i + 1
                        Case "("c, ")"c, "\"c
                            ' Ignore the escape '\' and then add the escaped character onwards
                            sb.Append(raw.Substring(first, i - first))
                            i += 1
                            first = i
                        Case "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c
                            ' Add preceding characters not already appended
                            If first < i Then
                                sb.Append(raw.Substring(first, i - first))
                                first = i
                            End If

                            ' Find all the octal digits
                            Dim octal As Byte = 0

                            For j = i + 1 To last - 1
                                Dim c = raw(j)

                                If c >= "0"c AndAlso c <= "7"c Then
                                    octal *= 8
                                    octal += CByte(AscW(c) - Asc("0"c))
                                    i += 1
                                    first = i + 1
                                Else
                                    Exit For
                                End If
                            Next

                            sb.Append(Microsoft.VisualBasic.ChrW(octal))
                    End Select
                End If
            Next

            sb.Append(raw.Substring(first, last - first))
            Return sb.ToString()
        End Function
    End Class
End Namespace
