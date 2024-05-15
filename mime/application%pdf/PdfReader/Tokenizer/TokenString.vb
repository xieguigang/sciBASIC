#Region "Microsoft.VisualBasic::71c02a2b550222daa2bac71e65b48dd8, mime\application%pdf\PdfReader\Tokenizer\TokenString.vb"

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

    '   Total Lines: 58
    '    Code Lines: 44
    ' Comment Lines: 4
    '   Blank Lines: 10
    '     File Size: 2.00 KB


    '     Class TokenString
    ' 
    '         Properties: Raw
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EncodedBytesToString, GetStringLiteralUTF16
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace PdfReader
    Public MustInherit Class TokenString
        Inherits TokenObject

        Private _Raw As String

        Public Sub New(raw As String)
            Me.Raw = raw
        End Sub

        Public Property Raw As String
            Get
                Return _Raw
            End Get
            Private Set(value As String)
                _Raw = value
            End Set
        End Property

        Public MustOverride ReadOnly Property Resolved As String
        Public MustOverride ReadOnly Property ResolvedAsBytes As Byte()
        Public MustOverride Function BytesToString(raw As Byte()) As String

        Protected Function EncodedBytesToString(bytes As Byte()) As String
            ' Check for the UTF16 Byte Order Mark (little endian or big endian versions)
            If bytes.Length > 2 AndAlso bytes(0) = &HFE AndAlso bytes(1) = &HFF Then
                Return GetStringLiteralUTF16(bytes, True)
            ElseIf bytes.Length > 2 AndAlso bytes(0) = &HFF AndAlso bytes(1) = &HFE Then
                Return GetStringLiteralUTF16(bytes, False)
            Else
                ' Not unicode, so treat as ASCII
                Return Encoding.ASCII.GetString(bytes)
            End If
        End Function

        Private Function GetStringLiteralUTF16(bytes As Byte(), bigEndian As Boolean) As String
            Dim index = 0
            Dim last = bytes.Length - 1

            If bigEndian Then
                ' Swap byte ordering
                Dim temp As Byte

                While index < last
                    ' Switch byte order of each character pair
                    temp = bytes(index)
                    bytes(index) = bytes(index + 1)
                    bytes(index + 1) = temp
                    index += 2
                End While
            End If

            Return Encoding.Unicode.GetString(bytes)
        End Function
    End Class
End Namespace
