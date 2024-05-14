#Region "Microsoft.VisualBasic::17ba708ca2e75dbfadd1b4261da10f55, Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\Metric.vb"

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

    '   Total Lines: 20
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 597 B


    '     Class Metric
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: DistanceTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Correlations

Namespace KMeans

    Public Class Metric

        ReadOnly propertyNames As String()

        Sub New(propertyNames As IEnumerable(Of String))
            Me.propertyNames = propertyNames.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DistanceTo(x As EntityClusterModel, y As EntityClusterModel) As Double
            Return x(propertyNames).EuclideanDistance(y(propertyNames))
        End Function

    End Class
End Namespace
