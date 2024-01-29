#Region "Microsoft.VisualBasic::500aabdcd638365bf0d783c918c7a26e, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\CompleteLinkage\Point.vb"

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

'   Total Lines: 53
'    Code Lines: 39
' Comment Lines: 0
'   Blank Lines: 14
'     File Size: 1.69 KB


'     Class Point
' 
'         Properties: CompleteLinkageResultCluster, LloydsResultCluster
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: distanceToOtherPoint, fromStringArray
' 
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
