#Region "Microsoft.VisualBasic::514a58086187095268bda501562bb7ec, sciBASIC#\Data\BinaryData\BinaryData\BufferRegion.vb"

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

    '   Total Lines: 24
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 487 B


    ' Class BufferRegion
    ' 
    '     Properties: nextBlock, position, size
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Class BufferRegion

    Public Property position As Long
    Public Property size As Integer

    Public ReadOnly Property nextBlock As Long
        Get
            Return position + size
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(scan0 As Long, size As Integer)
        Me.position = scan0
        Me.size = size
    End Sub

    Public Overrides Function ToString() As String
        Return $"&{position} [{size} bytes]"
    End Function

End Class
