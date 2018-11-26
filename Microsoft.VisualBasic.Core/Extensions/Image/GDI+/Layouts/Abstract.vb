#Region "Microsoft.VisualBasic::cd51de795fa217e83ca60562489aaf77, Microsoft.VisualBasic.Core\Extensions\Image\GDI+\Layouts\LayoutedObject.vb"

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

'     Interface ILayoutedObject
' 
'         Properties: Location
' 
'     Interface ILayoutCoordinate
' 
'         Properties: ID, X, Y
' 
'     Class mxPoint
' 
'         Properties: Point, X, Y
' 
'         Constructor: (+4 Overloads) Sub New
'         Function: Clone, Equals, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Any typed object with a location layout value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface ILayoutedObject(Of T)
        Inherits Value(Of T).IValueOf

        Property Location As PointF
    End Interface

    Public Interface ILayoutCoordinate
        Property ID As String
        Property X As Double
        Property Y As Double
    End Interface

End Namespace
