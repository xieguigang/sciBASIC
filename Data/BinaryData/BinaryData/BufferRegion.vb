#Region "Microsoft.VisualBasic::a1ab85a868f9dcf64f908275104e6b9c, Data\BinaryData\BinaryData\BufferRegion.vb"

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

    ' Class BufferRegion
    ' 
    '     Properties: nextBlock, position, size
    ' 
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

    Public Overrides Function ToString() As String
        Return $"&{position} [{size} bytes]"
    End Function

End Class

