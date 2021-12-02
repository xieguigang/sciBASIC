Namespace ApplicationServices.Terminal.TablePrinter

    Public Class ConsoleTableBaseData

        Public Property Column As List(Of Object)
        Public Property Rows As List(Of List(Of Object))

        Public Function AppendLine(line As IEnumerable) As ConsoleTableBaseData
            Rows.Add((From x In line Select x).ToList)
            Return Me
        End Function
    End Class
End Namespace