Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Namespace GMM

    Public Class Datum

        Private m_val As  ClusterEntity
        Private m_probs As Double()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(value As Double, components As Integer)
            Call Me.New({value}, components)
        End Sub

        Sub New(value As IEnumerable(Of Double), components As Integer)
            Call Me.New(New ClusterEntity With {.entityVector = value.ToArray}, components)
        End Sub

        Sub New(value As ClusterEntity, components As Integer)
            m_val = value
            m_probs = New Double(components - 1) {}

            For i = 0 To m_probs.Length - 1
                m_probs(i) = 0.0
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function val() As Double
            Return m_val.entityVector.EuclideanDistance
        End Function

        Public Overridable Sub setProb(i As Integer, val As Double)
            m_probs(i) = val
        End Sub

        Public Overridable Function getProb(i As Integer) As Double
            Return m_probs(i)
        End Function
    End Class
End Namespace