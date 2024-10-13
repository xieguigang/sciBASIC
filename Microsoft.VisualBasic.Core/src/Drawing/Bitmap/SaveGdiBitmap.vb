#Region "Microsoft.VisualBasic::ad77adc5d9789cfc42788e15abdb646d, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\SaveGdiBitmap.vb"

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

    '   Total Lines: 23
    '    Code Lines: 14 (60.87%)
    ' Comment Lines: 5 (21.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (17.39%)
    '     File Size: 561 B


    '     Interface SaveGdiBitmap
    ' 
    '         Function: Save
    ' 
    '     Enum BitsPerPixel
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Namespace Imaging.BitmapImage

    Public Interface SaveGdiBitmap

        ''' <summary>
        ''' <see cref="Image.Save"/>
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <param name="format"></param>
        Function Save(stream As Stream, format As ImageFormats) As Boolean
    End Interface

    Public Enum BitsPerPixel As Int16
        One = 1
        Four = 4
        Eight = 8
        TwentyFour = 24
    End Enum
End Namespace
