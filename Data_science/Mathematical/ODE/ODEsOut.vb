Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ODEs output
''' </summary>
Public Class out

    Public Property x As Double()
    Public Property y As Dictionary(Of NamedValue(Of Double()))

    ''' <summary>
    ''' Is there NAN value in the function value <see cref="y"/> ???
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property HaveNaN As Boolean
        Get
            For Each val As NamedValue(Of Double()) In y.Values
                For Each x As Double In val.x
                    If Double.IsNaN(x) OrElse
                        Double.IsInfinity(x) OrElse
                        Double.IsNegativeInfinity(x) OrElse
                        Double.IsPositiveInfinity(x) Then

                        Return True
                    End If
                Next
            Next

            Return False
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Function DataFrame(Optional xDisp As String = "X") As DocumentStream.File
        Dim ly = y.Values.ToArray
        Dim file As New DocumentStream.File
        Dim head As New RowObject(xDisp + ly.ToList(Function(s) s.Name))

        file += head

        For Each x As SeqValue(Of Double) In Me.x.SeqIterator
            file += (x.obj.ToString + ly.ToList(Function(n) n.x(x.i).ToString))
        Next

        Return file
    End Function
End Class
