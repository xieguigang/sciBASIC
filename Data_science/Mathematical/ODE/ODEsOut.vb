Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ODEs output
''' </summary>
Public Class ODEsOut

    Public Property x As Double()
    Public Property y As Dictionary(Of NamedValue(Of Double()))
    Public Property y0 As Dictionary(Of String, Double)
    Public Property params As Dictionary(Of String, Double)

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

        Dim skips As Integer = ly.Length + 2

        For Each v In y0.SafeQuery.JoinAsIterator(params).SeqIterator
            Dim row As RowObject = file(v.i)
            Dim var = v.obj

            row(skips) = var.Key
            row(skips + 1) = var.Value
        Next

        Return file
    End Function

    Public Shared Function LoadFromDataFrame(csv As String) As ODEsOut
        Dim df As File = File.Load(csv)
        Dim cols = df.Columns().ToArray
        Dim X = cols(Scan0)
        Dim y = cols.Skip(1).Take(cols.Length - 1 - 3).ToArray
        Dim params = cols.Skip(cols.Length - 2).ToArray
        Dim yData As NamedValue(Of Double())() =
            LinqAPI.Exec(Of NamedValue(Of Double())) <= From s As String()
                                                        In y
                                                        Let name As String = s(Scan0)
                                                        Let values As Double() = s.Skip(1).ToArray(AddressOf Val)
                                                        Select New NamedValue(Of Double()) With {
                                                            .Name = name,
                                                            .x = values
                                                        }
        Return New ODEsOut With {
            .x = X.Skip(1).ToArray(AddressOf Val),
            .y = yData.ToDictionary,
            .params = __getArgs(params(Scan0), params(1))  ' 由于没有信息可以了解哪些变量是y0初始值，所以在这里都把这些数据放在变量参数列表里面
        }
    End Function

    Private Shared Function __getArgs(names As String(), values As String()) As Dictionary(Of String, Double)
        Dim params As New Dictionary(Of String, Double)

        For Each var As SeqValue(Of String) In names.SeqIterator
            If String.IsNullOrEmpty(+var) Then
                Exit For
            Else
                params(+var) = Val(values(var.i))
            End If
        Next

        Return params
    End Function
End Class
