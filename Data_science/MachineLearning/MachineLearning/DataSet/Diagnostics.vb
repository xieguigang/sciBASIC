Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace StoreProcedure

    Public Module Diagnostics

        <Extension>
        Public Iterator Function CheckDataSet(data As DataSet) As IEnumerable(Of LogEntry)
            Dim nSamples = data.DataSamples.size
            Dim size As Size = data.Size
            Dim outputSize As Integer = data.OutputSize

            ' check for sample datas
            For Each sample As Sample In data.DataSamples.AsEnumerable
                If sample.vector.Length <> size.Width Then
                    Yield New LogEntry With {
                        .message = $"sample vector size is not equals to {size.Width}!",
                        .[object] = sample.ID,
                        .time = Now,
                        .level = MSG_TYPES.ERR
                    }
                End If
                If sample.target.Length <> outputSize Then
                    Yield New LogEntry With {
                        .message = $"output vector size is not equals to {outputSize}!",
                        .[object] = sample.ID,
                        .time = Now,
                        .level = MSG_TYPES.ERR
                    }
                End If
            Next

            Dim i As i32 = Scan0

            For Each [property] In data.NormalizeMatrix.matrix.AsEnumerable
                If [property].size <> nSamples Then
                    Yield New LogEntry With {
                        .message = "sample size is not equals to the normalized samples",
                        .level = MSG_TYPES.WRN,
                        .[object] = data.NormalizeMatrix.names.ElementAtOrDefault(++i, $"unknown_{CInt(i) - 1}"),
                        .time = Now
                    }
                End If
            Next
        End Function
    End Module
End Namespace