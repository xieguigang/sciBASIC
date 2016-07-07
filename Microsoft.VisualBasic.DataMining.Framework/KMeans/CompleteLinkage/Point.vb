Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.Framework.ComponentModel
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