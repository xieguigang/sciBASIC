Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace Heatmap

    Public Module DataScaler

        ''' <summary>
        ''' 主要是通过这个方法将样本值转换为索引编号，即将值映射到颜色数组中的索引，即可以使用颜色来表示相对应的样本数值
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="scaleMethod"></param>
        ''' <param name="levels%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DoDataScale(data As IEnumerable(Of DataSet), scaleMethod As DrawElements, levels#) As Dictionary(Of String, DataSet)
            Select Case scaleMethod
                Case DrawElements.Cols
                    Return data _
                        .ScaleByCol(levels) _
                        .ToDictionary(Function(x) x.ID)
                Case DrawElements.Rows
                    Return data _
                        .ScaleByRow(levels) _
                        .ToDictionary(Function(x) x.ID)
                Case Else
                    Return data _
                        .ScaleByALL(levels) _
                        .ToDictionary(Function(x) x.ID)
            End Select
        End Function

        ''' <summary>
        ''' 返回来的都是0-1之间的数，乘以颜色数组长度之后即可庸作为颜色的index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleByRow(data As IEnumerable(Of DataSet), levels#) As IEnumerable(Of DataSet)
            Dim levelRange As DoubleRange = {0R, levels}

            Return data _
                .Select(Function(x)
                            Dim range As DoubleRange = x.Properties.Values.Range
                            Dim values As Dictionary(Of String, Double)

                            If range.Length = 0 Then
                                values = x.Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key) levels)
                            Else
                                values = x _
                                    .Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key)
                                                      Return range.ScaleMapping(x(key), levelRange)
                                                  End Function)
                            End If

                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = values
                            }
                        End Function)
        End Function

        ''' <summary>
        ''' 返回来的都是0-1之间的数，乘以颜色数组长度之后即可庸作为颜色的index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleByCol(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim list = data.ToArray
            Dim keys = list.PropertyNames
            Dim ranges As Dictionary(Of String, DoubleRange) = keys _
                .ToDictionary(Function(key) key,
                              Function(key)
                                  Return list _
                                    .Select(Function(x) x(key)) _
                                    .Range
                              End Function)
            Dim levelRange As DoubleRange = {0R, levels}

            Return list _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key)
                                                      Return ranges(key).ScaleMapping(x(key), levelRange)
                                                  End Function)
                            }
                        End Function)
        End Function

        <Extension>
        Public Function ScaleByALL(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim list = data.ToArray
            Dim keys = list.PropertyNames
            Dim range As DoubleRange = list _
                .Select(Function(x) x.Properties.Values) _
                .IteratesALL _
                .Range
            Dim levelRange As DoubleRange = {0R, levels}

            Return data _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = x _
                                    .Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key)
                                                      Return range.ScaleMapping(x(key), levelRange)
                                                  End Function)
                            }
                        End Function)
        End Function
    End Module
End Namespace