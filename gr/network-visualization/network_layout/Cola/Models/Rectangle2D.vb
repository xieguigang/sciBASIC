#Region "Microsoft.VisualBasic::995e0bf0f7ae4da90247832885b1f214, gr\network-visualization\network_layout\Cola\Models\Rectangle2D.vb"

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

    '   Total Lines: 17
    '    Code Lines: 12 (70.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (29.41%)
    '     File Size: 392 B


    '     Class Rectangle2D
    ' 
    '         Properties: space_left
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports RectangleF = Microsoft.VisualBasic.Imaging.LayoutModel.Rectangle2D

Namespace Cola

    Public Class Rectangle2D : Inherits RectangleF

        Public Property space_left As Double

        Sub New(x1#, x2#, y1#, y2#)
            Call MyBase.New(x1, x2, y1, y2)
        End Sub

        Sub New()
            Call MyBase.New
        End Sub
    End Class
End Namespace
