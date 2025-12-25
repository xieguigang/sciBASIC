#Region "Microsoft.VisualBasic::61c29c0d89064b9523a21ab96d3c2013, Data\BinaryData\BinaryData\BufferRegion.vb"

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

    '   Total Lines: 50
    '    Code Lines: 27 (54.00%)
    ' Comment Lines: 15 (30.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (16.00%)
    '     File Size: 1.21 KB


    ' Class BufferRegion
    ' 
    '     Properties: nextBlock, position, size, Zero
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' A binary data region in the binary raw data file
''' </summary>
Public Class BufferRegion

    ''' <summary>
    ''' the start position of the data region
    ''' </summary>
    ''' <returns></returns>
    Public Property position As Long
    ''' <summary>
    ''' the region size in bytes
    ''' </summary>
    ''' <returns></returns>
    Public Property size As Integer

    Public ReadOnly Property nextBlock As Long
        Get
            Return position + size
        End Get
    End Property

    ''' <summary>
    ''' start position is zero and there is no buffer size
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property Zero As BufferRegion
        Get
            Return New BufferRegion(0, -1)
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(scan0 As Long, size As Integer)
        Me.position = scan0
        Me.size = size
    End Sub

    Sub New(copy As BufferRegion)
        Me.size = copy.size
        Me.position = copy.position
    End Sub

    Public Overrides Function ToString() As String
        Return $"&{StringFormats.Lanudry(position)} [{StringFormats.Lanudry(size)}]"
    End Function

End Class
