#Region "Microsoft.VisualBasic::0b0d890a6dc37737de8f4ea7ff8501f8, Data\BinaryData\BinaryData\Stream\ReadonlyStream.vb"

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

    '   Total Lines: 35
    '    Code Lines: 17 (48.57%)
    ' Comment Lines: 13 (37.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (14.29%)
    '     File Size: 1.08 KB


    ' Class ReadonlyStream
    ' 
    '     Properties: CanWrite
    ' 
    '     Sub: Flush, SetLength, Write
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Public MustInherit Class ReadonlyStream : Inherits Stream

    Public Overrides ReadOnly Property CanWrite() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' do nothing at here 
    ''' </summary>
    Public Overrides Sub Flush()
        Call "readonly stream not supports of flash stream data".warning
    End Sub

    ''' <summary>
    ''' do nothing at here
    ''' </summary>
    ''' <param name="value"></param>
    Public Overrides Sub SetLength(value As Long)
        Call "readonly stream not supports of modify stream size, this action just be ignored".error
    End Sub

    ''' <summary>
    ''' do nothing at here
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="offset"></param>
    ''' <param name="count"></param>
    Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
        Call "readonly stream not supports of modify stream data, this action just be ignored".error
    End Sub
End Class
