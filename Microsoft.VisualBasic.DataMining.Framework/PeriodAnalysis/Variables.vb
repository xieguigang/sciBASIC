Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.IEnumerations
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Serials.PeriodAnalysis

    Public Structure SerialsVarialble
        Implements sIdEnumerable

        Public Property Identifier As String Implements sIdEnumerable.Identifier
        Dim SerialsData As Double()

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        Public Shared Function Load(source As File) As SerialsVarialble()
            Dim LQuery = (From Line As RowObject
                          In source
                          Let Tag As String = Line.First
                          Let Data As Double() = (From col As String In Line.Skip(1) Select Val(col)).ToArray
                          Select New SerialsVarialble With {
                              .Identifier = Tag,
                              .SerialsData = Data}).ToArray
            Return LQuery
        End Function
    End Structure

    Public Class TimePoint
        Public Property Time As Integer
        Public Property Value As Double

        Public Overrides Function ToString() As String
            Return String.Format("{0} --> {1}", Time, Value)
        End Function

        Friend Shared Function GetData(source As IEnumerable(Of TimePoint), Time As Integer) As TimePoint
            Dim LQuery = (From p In source Where p.Time = Time Select p).First
            Return LQuery
        End Function

        Friend Shared Function GetData(Time As Integer, source As IEnumerable(Of TimePoint)) As Double
            Dim LQuery = (From p In source Where p.Time = Time Select p).ToArray
            If LQuery.IsNullOrEmpty Then
                Return 0
            Else
                Return LQuery.First.Value
            End If
        End Function

        Friend Shared Function CreateBufferObject(ChunkBuffer As Generic.IEnumerable(Of TimePoint)) As SortedDictionary(Of Integer, Double)
            Dim Temp As SortedDictionary(Of Integer, Double) = New SortedDictionary(Of Integer, Double)
            For Each p In ChunkBuffer
                Call Temp.Add(p.Time, p.Value)
            Next

            Return Temp
        End Function
    End Class

    Public Class SamplingData
        Public Property Peaks As List(Of TimePoint)
        Public Property Trough As List(Of TimePoint)
        Public Property FiltedData As List(Of TimePoint)
        Public Property TimePoints As Integer
        Public Property TSerials As TimePoint()
    End Class
End Namespace
