Imports System.Collections.Generic
Imports System.Data
Imports System.Linq

Namespace DecisionTree

    Public Class MyAttribute
        Public Sub New(name__1 As String, differentAttributenames__2 As List(Of String))
            Name = name__1
            DifferentAttributeNames = differentAttributenames__2
        End Sub

        Public ReadOnly Property Name() As String

        Public ReadOnly Property DifferentAttributeNames() As List(Of String)

        Public Property InformationGain() As Double
            Get
                Return m_InformationGain
            End Get
            Set
                m_InformationGain = Value
            End Set
        End Property
        Private m_InformationGain As Double

        Public Shared Function GetDifferentAttributeNamesOfColumn(data As DataTable, columnIndex As Integer) As List(Of String)
            Dim differentAttributes = New List(Of String)()

            For i As Integer = 0 To data.Rows.Count - 1
                Dim index = i
                Dim found = differentAttributes.Any(Function(t) t.ToUpper().Equals(data.Rows(index)(columnIndex).ToString().ToUpper()))

                If Not found Then
                    differentAttributes.Add(data.Rows(i)(columnIndex).ToString())
                End If
            Next

            Return differentAttributes
        End Function
    End Class
End Namespace