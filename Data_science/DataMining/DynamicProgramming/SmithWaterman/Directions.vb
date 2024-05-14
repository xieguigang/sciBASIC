#Region "Microsoft.VisualBasic::bf5738edfd5bc6c384d96883eee3c238, Data_science\DataMining\DynamicProgramming\SmithWaterman\Directions.vb"

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

    '   Total Lines: 28
    '    Code Lines: 8
    ' Comment Lines: 17
    '   Blank Lines: 3
    '     File Size: 723 B


    '     Class Directions
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SmithWaterman

    ''' <summary>
    ''' Constants of directions.
    ''' Multiple directions are stored by bits.
    ''' The zero direction is the starting point.
    ''' </summary>
    Public Class Directions

        ''' <summary>
        ''' 0001
        ''' </summary>
        Public Const DR_LEFT As Integer = 1
        ''' <summary>
        ''' 0010
        ''' </summary>        
        Public Const DR_UP As Integer = 2
        ''' <summary>
        ''' 0100
        ''' </summary>        
        Public Const DR_DIAG As Integer = 4
        ''' <summary>
        ''' 1000
        ''' </summary>        
        Public Const DR_ZERO As Integer = 8

    End Class
End Namespace
