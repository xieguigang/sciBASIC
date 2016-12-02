Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Language

Public Module Extensions

    ''' <summary>
    ''' 使用PCC来了解各个变量之间的相关度
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    <Extension> Public Function Pcc(df As ODEsOut) As DataSet()
        Dim out As New List(Of DataSet)
        Dim vars$() = df.y.Keys.ToArray

        For Each var As NamedValue(Of Double()) In df
            Dim x As New DataSet With {
                .Identifier = var.Name,
                .Properties = New Dictionary(Of String, Double)
            }

            For Each name$ In vars
                Dim __pcc# = Correlations _
                    .GetPearson(var.Value, df.y(name).Value)
                x.Properties(name$) = __pcc
            Next

            out += x
        Next

        Return out
    End Function

    ''' <summary>
    ''' 使用sPCC来了解各个变量之间的相关度
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    <Extension> Public Function SPcc(df As ODEsOut) As DataSet()
        Dim out As New List(Of DataSet)
        Dim vars$() = df.y.Keys.ToArray

        For Each var As NamedValue(Of Double()) In df
            Dim x As New DataSet With {
                .Identifier = var.Name,
                .Properties = New Dictionary(Of String, Double)
            }

            For Each name$ In vars
                Dim __spcc# = Correlations _
                    .Spearman(var.Value, df.y(name).Value)
                x.Properties(name$) = __spcc
            Next

            out += x
        Next

        Return out
    End Function
End Module
