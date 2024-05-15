#Region "Microsoft.VisualBasic::e064611db458c46a361a03134ec8554a, Data_science\Graph\Model\GridGraph\IPoint2D.vb"

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

    '   Total Lines: 18
    '    Code Lines: 7
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 405 B


    '     Interface IPoint2D
    ' 
    '         Properties: X, Y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging

Namespace GridGraph

    ''' <summary>
    ''' 2d point data model in readonly
    ''' </summary>
    ''' <remarks>
    ''' <see cref="RasterPixel"/> is not a readonly 2d point data model
    ''' </remarks>
    Public Interface IPoint2D

        ReadOnly Property X As Integer
        ReadOnly Property Y As Integer

    End Interface

End Namespace
