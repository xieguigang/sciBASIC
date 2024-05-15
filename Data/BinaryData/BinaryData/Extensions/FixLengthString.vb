#Region "Microsoft.VisualBasic::caee8921de8aef7e1ccec0d478fa11a3, Data\BinaryData\BinaryData\Extensions\FixLengthString.vb"

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
    '     File Size: 677 B


    ' Class FixLengthString
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetBytes, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Public Class FixLengthString

    ReadOnly encoding As Encoding

    Sub New(encoding As Encoding)
        Me.encoding = encoding
    End Sub

    Public Function GetBytes(text$, bytLen%) As Byte()
        Dim bytes As Byte() = encoding.GetBytes(text)

        If bytes.Length > bytLen Then
            Return bytes.Take(bytLen).ToArray
        ElseIf bytes.Length < bytLen Then
            ReDim Preserve bytes(bytLen - 1)
            Return bytes
        Else
            Return bytes
        End If
    End Function

    Public Overrides Function ToString() As String
        Return encoding.ToString
    End Function
End Class
