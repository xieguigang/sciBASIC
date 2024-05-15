#Region "Microsoft.VisualBasic::50f1abb95167c2281d6b1109e3b7bce3, mime\application%pdf\PdfReader\Document\PdfDecryptNone.vb"

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

    '   Total Lines: 27
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 933 B


    '     Class PdfDecryptNone
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: DecodeStream, DecodeStreamAsBytes, DecodeString, DecodeStringAsBytes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace PdfReader
    Public Class PdfDecryptNone
        Inherits PdfDecrypt

        Public Sub New(parent As PdfObject)
            MyBase.New(parent)
        End Sub

        Public Overrides Function DecodeString(obj As PdfString) As String
            Return obj.ParseString.Value
        End Function

        Public Overrides Function DecodeStringAsBytes(obj As PdfString) As Byte()
            Return obj.ParseString.ValueAsBytes
        End Function

        Public Overrides Function DecodeStream(stream As PdfStream) As String
            Return Encoding.ASCII.GetString(stream.ParseStream.DecodeBytes(stream.ParseStream.StreamBytes))
        End Function

        Public Overrides Function DecodeStreamAsBytes(stream As PdfStream) As Byte()
            Return stream.ParseStream.DecodeBytes(stream.ParseStream.StreamBytes)
        End Function
    End Class
End Namespace
