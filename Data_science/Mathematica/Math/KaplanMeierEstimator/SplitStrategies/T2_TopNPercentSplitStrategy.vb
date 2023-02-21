Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models

Namespace SplitStrategies
    Public Class T2_TopNPercentSplitStrategy
        Implements ISplitStrategy
        Private ReadOnly m_percent As Integer
        Private ReadOnly m_patients As IDictionary(Of Integer, Patient)

        Public Sub New(ByVal percent As Integer, ByVal patients As IDictionary(Of Integer, Patient))
            If percent <= 0 OrElse percent > 50 Then
                Throw New ArgumentOutOfRangeException("percent")
            End If

            If patients Is Nothing Then
                Throw New ArgumentNullException("patients")
            End If

            m_percent = percent
            m_patients = patients
        End Sub

        Public Sub DoSplit(ByVal genes As IEnumerable(Of GeneExpression), <Out> ByRef groupA As IEnumerable(Of Patient), <Out> ByRef groupB As IEnumerable(Of Patient)) Implements ISplitStrategy.DoSplit
            Dim relevantGenes = genes.Where(Function(gene) Not Double.IsNaN(gene.After) AndAlso m_patients.ContainsKey(gene.PatientId))
            Dim orderedGenes = relevantGenes.OrderBy(Function(gene) gene.After)

            Dim groupSize As Integer = CInt((orderedGenes.Count() * (m_percent / 100.0)))

            Dim groupAGenes = orderedGenes.Take(groupSize)
            Dim groupBGenes = orderedGenes.Skip(orderedGenes.Count() - groupSize)

            groupA = groupAGenes.[Select](Function(gene) m_patients(gene.PatientId)).ToList()
            groupB = groupBGenes.[Select](Function(gene) m_patients(gene.PatientId)).ToList()
        End Sub

        Public ReadOnly Property Name As String Implements ISplitStrategy.Name
            Get
                Return "T2_Top" & m_percent
            End Get
        End Property
    End Class
End Namespace
