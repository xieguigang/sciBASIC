#Region "Microsoft.VisualBasic::c6783325811ad1a25cfe8133ee521d81, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\KMeans\CompleteLinkage\Point.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans.CompleteLinkage

    Public Class Point : Inherits EntityBase(Of Double)

        Private mResultantKMeansCluster As Integer = -1
        Private mResultantClusterCompleteLinkage As Integer = -1

        Public Sub New(units As Double())
            Properties = units
        End Sub

        Public Sub New()
        End Sub

        Public Shared Function fromStringArray(ParamArray unitsAsString As String()) As Point
            Dim ___dimension As Integer = unitsAsString.Length

            Dim unitsAsDouble As Double() = New Double(___dimension - 1) {}

            For i As Integer = 0 To ___dimension - 1
                unitsAsDouble(i) = Convert.ToDouble(unitsAsString(i))
            Next i

            Return New Point(unitsAsDouble)
        End Function

        Public Sub CompleteLinkageCluster(___cluster As Integer)
            mResultantClusterCompleteLinkage = ___cluster
        End Sub

        Public Function distanceToOtherPoint(otherPoint As Point) As Double
            Return DistanceBetweenPoints(otherPoint)
        End Function

        Public ReadOnly Property CompleteLinkageResultCluster As Integer
            Get
                Return mResultantClusterCompleteLinkage
            End Get
        End Property

        Public Sub SetKMeansCluster(kMeansCluster As Integer)
            mResultantKMeansCluster = kMeansCluster
        End Sub

        Public ReadOnly Property LloydsResultCluster As Integer
            Get
                Return mResultantKMeansCluster
            End Get
        End Property
    End Class
End Namespace
