Imports System.Runtime.CompilerServices

Namespace Knapsack

    Public Class Item

        Public Property Name As String
        Public Property Value As Integer
        Public Property Weight As Integer

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(name As String, value As Integer, weight As Integer)
            Me.Name = name
            Me.Value = value
            Me.Weight = weight
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format(vbLf & "    {0} - weight: {1}, value: {2}", Name, Weight, Value)
        End Function
    End Class
End Namespace
