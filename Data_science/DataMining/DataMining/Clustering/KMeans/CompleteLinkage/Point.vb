#Region "Microsoft.VisualBasic::3b6b87574e95b1920db255cf1111aa2f, Data_science\DataMining\DataMining\Clustering\KMeans\CompleteLinkage\Point.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
            entityVector = units
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
