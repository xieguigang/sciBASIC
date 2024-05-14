#Region "Microsoft.VisualBasic::68a7f9f29425dbed9adb4c9a67fa4a6e, gr\Microsoft.VisualBasic.Imaging\Drivers\Exif\JpegMetadata.vb"

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

    '   Total Lines: 15
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 401 B


    '     Class JpegMetadata
    ' 
    '         Properties: Author, Comments, Keywords, Rating, Subject
    '                     Title
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Driver

    Public Class JpegMetadata

        Public Property Title As String
        Public Property Subject As String
        Public Property Author As List(Of String)
        Public Property Rating As Integer
        Public Property Keywords As List(Of String)
        Public Property Comments As String

        Friend Sub New()
        End Sub
    End Class
End Namespace
