Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module StreamExtension

    ''' <summary>
    ''' Generates datafram and then can makes the result save data into a csv file.
    ''' </summary>
    ''' <param name="xDisp"></param>
    ''' <param name="fix%">Formats output by using <see cref="Math.Round"/></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function DataFrame(df As ODEsOut, Optional xDisp As String = "X", Optional fix% = -1) As DocumentStream.File
        Dim ly = df.y.Values.ToArray
        Dim file As New DocumentStream.File
        Dim head As New RowObject(xDisp + ly.ToList(Function(s) s.Name))
        Dim round As Func(Of Double, String)

        If fix <= 0 Then
            round = Function(n) CStr(n)
        Else
            round = Function(n) CStr(Math.Round(n, fix))
        End If

        file += head

        For Each x As SeqValue(Of Double) In df.x.SeqIterator
            file += (round(x.value) + ly.ToList(Function(n) round(n.Value(x.i))))
        Next

        Dim skips As Integer = ly.Length + 2

        For Each v In df.y0.SafeQuery.JoinIterates(df.params).SeqIterator
            Dim row As RowObject = file(v.i)
            Dim var As KeyValuePair(Of String, Double) = v.value

            row(skips) = var.Key
            row(skips + 1) = var.Value
        Next

        Return file
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="csv$"></param>
    ''' <param name="noVars">ODEs Parameter value is not exists in the data file?</param>
    ''' <returns></returns>
    Public Function LoadFromDataFrame(csv$, Optional noVars As Boolean = False) As ODEsOut
        Dim df As File = File.Load(csv)
        Dim cols = df.Columns().ToArray
        Dim X = cols(Scan0)
        Dim y$()() = If(noVars,
            cols.Skip(1).ToArray,
            cols.Skip(1).Take(cols.Length - 1 - 3).ToArray)
        Dim params$()() = cols.Skip(cols.Length - 2).ToArray
        Dim args As Dictionary(Of String, Double) = If(
            noVars,
            New Dictionary(Of String, Double),
            __getArgs(params(Scan0), params(1))) ' 由于没有信息可以了解哪些变量是y0初始值，所以在这里都把这些数据放在变量参数列表里面
        Dim yData As NamedValue(Of Double())() =
            LinqAPI.Exec(Of NamedValue(Of Double())) <= From s As String()
                                                        In y
                                                        Let name As String = s(Scan0)
                                                        Let values As Double() = s.Skip(1).ToArray(AddressOf Val)
                                                        Select New NamedValue(Of Double()) With {
                                                            .Name = name,
                                                            .Value = values
                                                        }
        Return New ODEsOut With {
            .params = args,
            .x = X.Skip(1).ToArray(AddressOf Val),
            .y = yData.ToDictionary
        }
    End Function

    Private Function __getArgs(names As String(), values As String()) As Dictionary(Of String, Double)
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
End Module
