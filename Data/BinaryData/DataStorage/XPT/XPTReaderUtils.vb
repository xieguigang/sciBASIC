#Region "Microsoft.VisualBasic::8c73037cfa58814205f5bf65cce889ea, Data\BinaryData\DataStorage\XPT\XPTReaderUtils.vb"

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

    '   Total Lines: 91
    '    Code Lines: 72 (79.12%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (20.88%)
    '     File Size: 3.48 KB


    '     Class SimpleDateFormat
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: format
    ' 
    '     Class XPTReaderUtils
    ' 
    '         Function: convertSASDate9ToString, getInteger, getPrimitiveInteger, getPrimitiveShort, getShort
    '                   getString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text

Namespace Xpt

    Public Class SimpleDateFormat

        Private formatStr As String

        Public Sub New(format As String)
            formatStr = format
        End Sub

        Public Function format(dt As Date) As String
            Return dt.ToString(formatStr, CultureInfo.InvariantCulture)
        End Function
    End Class


    Public Class XPTReaderUtils

        Public Shared DATE5_FORMAT As SimpleDateFormat = New SimpleDateFormat("ddMMM ")
        Public Shared DATE6_FORMAT As SimpleDateFormat = New SimpleDateFormat(" ddMMM ")
        Public Shared DATE7_FORMAT As SimpleDateFormat = New SimpleDateFormat("ddMMMYY ")
        Public Shared DATE8_FORMAT As SimpleDateFormat = New SimpleDateFormat(" ddMMMYY ")
        Public Shared DATE9_FORMAT As SimpleDateFormat = New SimpleDateFormat("ddMMMYYYY")
        Public Shared DATE11_FORMAT As SimpleDateFormat = New SimpleDateFormat("dd-MMM-YYYY")

        Public Shared Function getString(line As Byte(), offset As Integer, len As Integer) As String

            Dim data = New Byte(len - 1) {}
            For i = 0 To len - 1
                data(i) = line(offset + i)
            Next
            Return Encoding.UTF8.GetString(data).Trim()
        End Function

        Public Shared Function getInteger(line As Byte(), offset As Integer, len As Integer) As Integer
            Dim val = getString(line, offset, len)
            If val.Length <= 0 Then
                Return 0
            End If
            Return Integer.Parse(val)
        End Function

        Public Shared Function getShort(line As Byte(), offset As Integer, len As Integer) As Short
            Dim val = getString(line, offset, 2)
            If val.Length <= 0 Then
                Return 0
            End If
            Return Short.Parse(val)
        End Function

        Public Shared Function getPrimitiveInteger(buffer As Byte(), offset As Integer) As Integer

            Dim val = (buffer(offset + 0) And &HFF) << 24 Or (buffer(offset + 1) And &HFF) << 16 Or (buffer(offset + 2) And &HFF) << 8 Or (buffer(offset + 3) And &HFF) << 0
            Return val
        End Function

        Public Shared Function getPrimitiveShort(buffer As Byte(), offset As Integer) As Short
            Dim val As Short = (buffer(offset + 0) And &HFF) << 8 Or (buffer(offset + 1) And &HFF) << 0
            Return val
        End Function

        Public Shared Function convertSASDate9ToString(dtformat As String, [date] As Double) As String
            Dim num As Integer = [date]
            Dim format = DATE9_FORMAT
            If "date5".Equals(dtformat) Then
                format = DATE5_FORMAT
            ElseIf "date6".Equals(dtformat) Then
                format = DATE6_FORMAT
            ElseIf "date7".Equals(dtformat) Then
                format = DATE7_FORMAT
            ElseIf "date8".Equals(dtformat) Then
                format = DATE8_FORMAT
            ElseIf "date9".Equals(dtformat) Then
                format = DATE9_FORMAT
            ElseIf "date11".Equals(dtformat) Then
                format = DATE11_FORMAT
            End If

            Dim cal As Date = New DateTime()
            cal = New DateTime(1960, 0, 1)
            cal.AddDays(num)

            Dim formatted = format.format(cal)
            Return formatted
        End Function
    End Class

End Namespace

