#Region "Microsoft.VisualBasic::6734a8870378a74e0658638690091cb8, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\TablePrinter\CharMapDefinition.vb"

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

    '   Total Lines: 40
    '    Code Lines: 39
    ' Comment Lines: 0
    '   Blank Lines: 1
    '     File Size: 1.97 KB


    '     Class CharMapDefinition
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags

Namespace ApplicationServices.Terminal.TablePrinter
    Public Class CharMapDefinition
        Public Shared FramePipDefinition As New Dictionary(Of CharMapPositions, Char) From {
            {CharMapPositions.TopLeft, "┌"c},
            {CharMapPositions.TopCenter, "┬"c},
            {CharMapPositions.TopRight, "┐"c},
            {CharMapPositions.MiddleLeft, "├"c},
            {CharMapPositions.MiddleCenter, "┼"c},
            {CharMapPositions.MiddleRight, "┤"c},
            {CharMapPositions.BottomLeft, "└"c},
            {CharMapPositions.BottomCenter, "┴"c},
            {CharMapPositions.BottomRight, "┘"c},
            {CharMapPositions.BorderLeft, "│"c},
            {CharMapPositions.BorderRight, "│"c},
            {CharMapPositions.BorderTop, "─"c},
            {CharMapPositions.BorderBottom, "─"c},
            {CharMapPositions.DividerY, "│"c},
            {CharMapPositions.DividerX, "─"c}
        }
        Public Shared FrameDoublePipDefinition As New Dictionary(Of CharMapPositions, Char) From {
            {CharMapPositions.TopLeft, "╔"c},
            {CharMapPositions.TopCenter, "╤"c},
            {CharMapPositions.TopRight, "╗"c},
            {CharMapPositions.MiddleLeft, "╟"c},
            {CharMapPositions.MiddleCenter, "┼"c},
            {CharMapPositions.MiddleRight, "╢"c},
            {CharMapPositions.BottomLeft, "╚"c},
            {CharMapPositions.BottomCenter, "╧"c},
            {CharMapPositions.BottomRight, "╝"c},
            {CharMapPositions.BorderLeft, "║"c},
            {CharMapPositions.BorderRight, "║"c},
            {CharMapPositions.BorderTop, "═"c},
            {CharMapPositions.BorderBottom, "═"c},
            {CharMapPositions.DividerY, "│"c},
            {CharMapPositions.DividerX, "─"c}
        }
    End Class
End Namespace
