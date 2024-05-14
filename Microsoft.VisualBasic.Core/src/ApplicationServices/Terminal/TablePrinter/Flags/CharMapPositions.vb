#Region "Microsoft.VisualBasic::d2558d37f971711556c19c24ffaed47e, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\TablePrinter\Flags\CharMapPositions.vb"

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

    '   Total Lines: 22
    '    Code Lines: 19
    ' Comment Lines: 3
    '   Blank Lines: 0
    '     File Size: 582 B


    '     Enum CharMapPositions
    ' 
    '         BorderBottom, BorderLeft, BorderRight, BorderTop, BottomCenter
    '         BottomLeft, BottomRight, DividerX, DividerY, MiddleCenter
    '         MiddleLeft, MiddleRight, TopCenter, TopLeft, TopRight
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.TablePrinter.Flags
    ''' <summary>
    ''' Check map here https://raw.githubusercontent.com/minhhungit/ConsoleTableExt/master/wiki/Images/CharMapPositions.png
    ''' </summary>
    Public Enum CharMapPositions
        TopLeft
        TopCenter
        TopRight
        MiddleLeft
        MiddleCenter
        MiddleRight
        BottomLeft
        BottomCenter
        BottomRight
        BorderTop
        BorderRight
        BorderBottom
        BorderLeft
        DividerX
        DividerY
    End Enum
End Namespace
