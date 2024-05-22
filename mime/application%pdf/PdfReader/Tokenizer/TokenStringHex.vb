#Region "Microsoft.VisualBasic::ffcb6265eb577b45fa2968c21ab4348f, mime\application%pdf\PdfReader\Tokenizer\TokenStringHex.vb"

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

    '   Total Lines: 41
    '    Code Lines: 29 (70.73%)
    ' Comment Lines: 3 (7.32%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (21.95%)
    '     File Size: 1.50 KB


    '     Class TokenStringHex
    ' 
    '         Properties: Resolved, ResolvedAsBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BytesToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace PdfReader
    Public Class TokenStringHex
        Inherits TokenString

        Public Sub New(raw As String)
            MyBase.New(raw)
        End Sub

        Public Overrides ReadOnly Property Resolved As String
            Get
                Return BytesToString(ResolvedAsBytes)
            End Get
        End Property

        Public Overrides ReadOnly Property ResolvedAsBytes As Byte()
            Get
                ' Remove all whitespace from the hex string
                Dim sections = MyBase.Raw.Split(New Char() {Microsoft.VisualBasic.Strings.ChrW(0), Microsoft.VisualBasic.Strings.ChrW(9), Microsoft.VisualBasic.Strings.ChrW(10), Microsoft.VisualBasic.Strings.ChrW(12), Microsoft.VisualBasic.Strings.ChrW(13), " "c})
                Dim hex = String.Join(String.Empty, sections)

                ' If a missing character from last hex pair, then default to 0, as per the spec
                If hex.Length Mod 2 = 1 Then hex += "0"

                ' Convert from hex to bytes
                Dim raw = New Byte(hex.Length / 2 - 1) {}

                For i = 0 To raw.Length - 1
                    raw(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
                Next

                Return raw
            End Get
        End Property

        Public Overrides Function BytesToString(bytes As Byte()) As String
            Return EncodedBytesToString(bytes)
        End Function
    End Class
End Namespace
