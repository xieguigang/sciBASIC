Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.Correlations

Public Module Correlation

    ''' <summary>
    ''' 这个函数处理的是没有经过归一化处理的原始数据
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="correlation">假若这个参数为空，则默认使用<see cref="Correlations.GetPearson(Double(), Double())"/></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function CorrelatesNormalized(data As IEnumerable(Of DataSet), Optional correlation As ICorrelation = Nothing) As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))
        Dim dataset As DataSet() = data.ToArray
        Dim keys$() = dataset(Scan0) _
            .Properties _
            .Keys _
            .ToArray

        correlation = correlation Or PearsonDefault

        For Each x As DataSet In dataset
            Dim out As New Dictionary(Of String, Double)
            Dim array#() = keys _
                .Select(Of Double)(x) _
                .ToArray

            For Each y As DataSet In dataset
                out(y.ID) = correlation(
                    X:=array,
                    Y:=keys _
                        .Select(Of Double)(y) _
                        .ToArray)
            Next

            Yield New NamedValue(Of Dictionary(Of String, Double)) With {
                .Name = x.ID,
                .Value = out
            }
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Pearson(data As IEnumerable(Of DataSet)) As IEnumerable(Of DataSet)
        Return data.CorrelatesNormalized(AddressOf GetPearson) _
            .Select(Function(r)
                        Return New DataSet With {
                            .ID = r.Name,
                            .Properties = r.Value
                        }
                    End Function)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Spearman(data As IEnumerable(Of DataSet)) As IEnumerable(Of DataSet)
        Return data.CorrelatesNormalized(AddressOf Correlations.Spearman) _
            .Select(Function(r)
                        Return New DataSet With {
                            .ID = r.Name,
                            .Properties = r.Value
                        }
                    End Function)
    End Function
End Module
