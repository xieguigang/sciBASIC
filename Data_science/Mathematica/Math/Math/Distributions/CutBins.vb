Imports Microsoft.VisualBasic.Language

Namespace Distributions

    ''' <summary>
    ''' 进行数据分箱操作
    ''' </summary>
    Public Module CutBins

        ''' <summary>
        ''' ### 数据等宽分箱
        ''' 
        ''' 将变量的取值范围分为<paramref name="k"/>个等宽的区间，每个区间当作一个分箱。
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Function FixedWidthBins(data As IEnumerable(Of Double), k%) As IEnumerable(Of DataBinBox)
            ' 升序排序方便进行快速计算
            Dim v = data.OrderBy(Function(d) d).ToArray
            Dim min# = v.First
            Dim max# = v.Last
            Dim width# = (max - min) / k

            Return FixedWidthBins(v, width)
        End Function

        Public Iterator Function FixedWidthBins(v#(), width#) As IEnumerable(Of DataBinBox)
            Dim x As New Value(Of Double)
            Dim len% = v.Length
            Dim min# = v.First
            Dim max = v.Last
            Dim i As int = 0
            Dim lowerbound# = min
            Dim upbound#
            Dim list As New List(Of Double)

            Do While lowerbound < max
                upbound = lowerbound + width

                ' 因为数据已经是经过排序了的，所以在这里可以直接进行区间计数
                Do While i < len AndAlso (x = v(++i)) >= lowerbound AndAlso x < upbound
                    list += x
                Loop

                Yield New DataBinBox(list)

                If i.Value = len Then
                    Exit Do
                Else
                    list *= 0
                End If
            Loop
        End Function
    End Module
End Namespace