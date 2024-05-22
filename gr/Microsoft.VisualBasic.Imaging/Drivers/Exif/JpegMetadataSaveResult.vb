#Region "Microsoft.VisualBasic::e18d848709af6737a7bbf3b6de22c74f, gr\Microsoft.VisualBasic.Imaging\Drivers\Exif\JpegMetadataSaveResult.vb"

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

    '   Total Lines: 14
    '    Code Lines: 11 (78.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (21.43%)
    '     File Size: 389 B


    '     Class JpegMetadataSaveResult
    ' 
    '         Properties: FilePath, IsSuccess, Metadata
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Driver

    Friend Class JpegMetadataSaveResult

        Public Property IsSuccess() As Boolean
        Public Property FilePath() As String
        Public Property Metadata() As JpegMetadata

        Public Sub New(filePath$, metadata As JpegMetadata)
            Me.FilePath = filePath
            Me.Metadata = metadata
        End Sub
    End Class
End Namespace
