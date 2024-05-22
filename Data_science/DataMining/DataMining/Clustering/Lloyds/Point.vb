#Region "Microsoft.VisualBasic::cd95f41845164efcb27008c04354e89e, Data_science\DataMining\DataMining\Clustering\Lloyds\Point.vb"

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

    '   Total Lines: 33
    '    Code Lines: 25 (75.76%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (24.24%)
    '     File Size: 964 B


    '     Class Point
    ' 
    '         Properties: CompleteLinkageResultCluster
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: CompleteLinkageCluster, SetKMeansCluster
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans

Namespace Lloyds

    Public Class Point : Inherits ClusterEntity

        Dim mResultantClusterCompleteLinkage As Integer = -1

        Public ReadOnly Property CompleteLinkageResultCluster As Integer
            Get
                Return mResultantClusterCompleteLinkage
            End Get
        End Property

        Public Sub New(units As Double())
            entityVector = units
        End Sub

        Public Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub CompleteLinkageCluster(cluster As Integer)
            mResultantClusterCompleteLinkage = cluster
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetKMeansCluster(kMeansCluster As Integer)
            cluster = kMeansCluster
        End Sub
    End Class
End Namespace
