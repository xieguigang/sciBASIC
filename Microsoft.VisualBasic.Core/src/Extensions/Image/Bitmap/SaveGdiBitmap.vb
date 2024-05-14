#Region "Microsoft.VisualBasic::1b371e321a291f68cf2a6122ea0811b7, Microsoft.VisualBasic.Core\src\Extensions\Image\Bitmap\SaveGdiBitmap.vb"

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
    '    Code Lines: 7
    ' Comment Lines: 5
    '   Blank Lines: 3
    '     File Size: 410 B


    '     Interface SaveGdiBitmap
    ' 
    '         Function: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging

Namespace Imaging.BitmapImage

    Public Interface SaveGdiBitmap

        ''' <summary>
        ''' <see cref="Image.Save"/>
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <param name="format"></param>
        Function Save(stream As IO.Stream, format As ImageFormat) As Boolean
    End Interface
End Namespace
