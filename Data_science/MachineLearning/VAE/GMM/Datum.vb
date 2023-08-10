Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations

Namespace GMM

    Public Class Datum

        Private m_val As ClusterEntity
        Private m_probs As Double()

        Public ReadOnly Property dataId As String
            Get
                Return m_val.uid
            End Get
        End Property

        Public ReadOnly Property probs As Double()
            Get
                Return m_probs
            End Get
        End Property

        ''' <summary>
        ''' get the cluster id of current data point most likely assign to
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property max As Integer
            Get
                Return which.Max(m_probs) + 1
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(value As Double, components As Integer, index As Integer)
            Call Me.New({value}, components, index)
        End Sub

        Sub New(value As IEnumerable(Of Double), components As Integer, index As Integer)
            Call Me.New(New ClusterEntity With {.entityVector = value.ToArray, .uid = index}, components)
        End Sub

        Sub New(value As ClusterEntity, components As Integer)
            m_val = value
            m_probs = New Double(components - 1) {}

            For i = 0 To m_probs.Length - 1
                m_probs(i) = 0.0
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getVector() As Double()
            Return m_val.entityVector
        End Function

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