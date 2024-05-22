#Region "Microsoft.VisualBasic::e048141e62f5bb74d0dc52649cb4ea9a, Data\BinaryData\BinaryData\Stream\Helpers.vb"

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

    '   Total Lines: 49
    '    Code Lines: 36 (73.47%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (26.53%)
    '     File Size: 1.14 KB


    ' Interface IReaderDebugAccess
    ' 
    '     Properties: Length, Position
    ' 
    '     Function: ReadBytes
    ' 
    ' Class Helpers
    ' 
    '     Function: getDebugView
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text

Public Interface IReaderDebugAccess

    ReadOnly Property Length As Long
    Property Position As Long

    Function ReadBytes(nsize As Integer) As Byte()

End Interface

Public Class Helpers

    Public Shared Function getDebugView(bin As IReaderDebugAccess, bufSize As Integer) As String
        Dim start As Long
        Dim nsize As Integer

        If bin.Position < bufSize \ 2 Then
            start = 0
        Else
            start = bin.Position - (bufSize \ 2)
        End If

        If start + bufSize > bin.Length Then
            nsize = bin.Length - start
        Else
            nsize = bufSize
        End If

        Dim chars As New List(Of Char)
        Dim c As Char

        For Each b As Byte In bin.ReadBytes(nsize)
            If ASCII.IsNonPrinting(b) Then
                c = "*"c
            Else
                c = ChrW(b)
            End If

            If c = vbNullChar Then
                c = "*"
            End If

            chars.Add(c)
        Next

        Return chars.CharString
    End Function
End Class
