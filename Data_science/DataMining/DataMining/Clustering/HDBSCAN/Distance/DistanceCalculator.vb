#Region "Microsoft.VisualBasic::cf1bbbd2e50b8fac32f35700e0990e2d, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Distance\DistanceCalculator.vb"

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
    '    Code Lines: 5
    ' Comment Lines: 13
    '   Blank Lines: 0
    '     File Size: 1005 B


    '     Interface IDistanceCalculator
    ' 
    '         Function: ComputeDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' An interface for classes which compute the distance between two points (where points are
    ''' represented as arrays of doubles).
    ''' </summary>
    Public Interface IDistanceCalculator(Of T)
        ''' <summary>
        ''' Computes the distance between two points.
        ''' Note that larger values indicate that the two points are farther apart.
        ''' </summary>
        ''' <param name="indexOne">The index of the first attribute</param>
        ''' <param name="indexTwo">The index of the second attribute</param>
        ''' <param name="attributesOne">The attributes of the first point</param>
        ''' <param name="attributesTwo">The attributes of the second point</param>
        ''' <returns>A double for the distance between the two points</returns>
        Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As T, attributesTwo As T) As Double
    End Interface
End Namespace
