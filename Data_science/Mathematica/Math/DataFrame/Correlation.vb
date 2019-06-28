Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.Correlations

Public Module Correlation

    ''' <summary>
    ''' 这个函数是计算列之间的相关度的
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function CorrelationMatrix(data As IEnumerable(Of DataSet), Optional doCor As ICorrelation = Nothing) As IEnumerable(Of DataSet)
        Dim dataset As DataSet() = data.ToArray
        Dim columns = dataset.PropertyNames _
            .Select(Function(colName)
                        Return New NamedValue(Of Double()) With {
                            .Name = colName,
                            .Value = dataset _
                                .Select(Function(d) d(colName)) _
                                .ToArray
                        }
                    End Function) _
            .ToArray

        Return Correlations.CorrelationMatrix(columns, doCor) _
            .Select(Function(r)
                        Return New DataSet With {
                            .ID = r.Name,
                            .Properties = r.Value
                        }
                    End Function)
    End Function

    ''' <summary>
    ''' 这个函数处理的是没有经过归一化处理的原始数据(这个函数是计算行之间的相关度的)
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="doCor">假若这个参数为空，则默认使用<see cref="Correlations.GetPearson(Double(), Double())"/></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function CorrelatesNormalized(data As IEnumerable(Of DataSet), Optional doCor As ICorrelation = Nothing) As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))
        Dim dataset As DataSet() = data.ToArray
        Dim keys$() = dataset(Scan0) _
            .Properties _
            .Keys _
            .ToArray
        Dim b As Double()

        doCor = doCor Or PearsonDefault

        For Each x As DataSet In dataset
            Dim out As New Dictionary(Of String, Double)
            Dim array#() = keys _
                .Select(Of Double)(x) _
                .ToArray

            For Each y As DataSet In dataset
                b = keys.Select(Of Double)(y).ToArray
                out(y.ID) = doCor(X:=array, Y:=b)
            Next

            Yield New NamedValue(Of Dictionary(Of String, Double)) With {
                .Name = x.ID,
                .Value = out
            }
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Pearson(data As IEnumerable(Of DataSet), Optional visit As MatrixVisit = MatrixVisit.ByRow) As IEnumerable(Of DataSet)
        If visit = MatrixVisit.ByRow Then
            Return data.CorrelatesNormalized(AddressOf GetPearson) _
                .Select(Function(r)
                            Return New DataSet With {
                                .ID = r.Name,
                                .Properties = r.Value
                            }
                        End Function)
        Else
            Return data.CorrelationMatrix(AddressOf GetPearson)
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Spearman(data As IEnumerable(Of DataSet), Optional visit As MatrixVisit = MatrixVisit.ByRow) As IEnumerable(Of DataSet)
        If visit = MatrixVisit.ByRow Then
            Return data.CorrelatesNormalized(AddressOf Correlations.Spearman) _
                .Select(Function(r)
                            Return New DataSet With {
                                .ID = r.Name,
                                .Properties = r.Value
                            }
                        End Function)
        Else
            Return data.CorrelationMatrix(AddressOf Correlations.Spearman)
        End If
    End Function
End Module
