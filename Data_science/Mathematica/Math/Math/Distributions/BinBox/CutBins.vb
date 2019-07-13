#Region "Microsoft.VisualBasic::eb97e142020036bc0744d3550cf6766a, Data_science\Mathematica\Math\Math\Distributions\BinBox\CutBins.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module CutBins
    ' 
    '         Function: EqualFrequencyBins, (+2 Overloads) FixedWidthBins
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Distributions.BinBox

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
        Public Function FixedWidthBins(Of T)(data As IEnumerable(Of T), k%, eval As Evaluate(Of T)) As IEnumerable(Of DataBinBox(Of T))
            ' 升序排序方便进行快速计算
            Dim v = data.OrderBy(Function(d) eval(d)).ToArray
            Dim min# = eval(v.First)
            Dim max# = eval(v.Last)
            Dim width# = (max - min) / k

            Return FixedWidthBins(v, width, eval)
        End Function

        ''' <summary>
        ''' ### 数据等宽分箱
        ''' 
        ''' 将变量按照给定的值域宽度分为多个区间
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="v"></param>
        ''' <param name="width">所给定的区间宽度</param>
        ''' <param name="eval"></param>
        ''' <returns></returns>
        Public Iterator Function FixedWidthBins(Of T)(v As T(), width#, eval As Evaluate(Of T)) As IEnumerable(Of DataBinBox(Of T))
            Dim x As New Value(Of Double)
            Dim len% = v.Length
            Dim min# = eval(v.First)
            Dim max# = eval(v.Last)
            Dim i As VBInteger = 0
            Dim lowerbound# = min
            Dim upbound#
            Dim list As New List(Of T)

            Do While lowerbound < max
                upbound = lowerbound + width

                ' 因为数据已经是经过排序了的，所以在这里可以直接进行区间计数
                Do While i < len AndAlso (x = eval(v(i))) >= lowerbound AndAlso x < upbound
                    list += v(++i)
                Loop

                Yield New DataBinBox(Of T)(list, eval)

                If i.Value = len Then
                    Exit Do
                Else
                    list *= 0
                End If
            Loop
        End Function

        ''' <summary>
        ''' 等频分箱，每一个bin之中的元素数目相等
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="k">得到K个bin</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function EqualFrequencyBins(Of T)(data As IEnumerable(Of T), k%, eval As Evaluate(Of T)) As IEnumerable(Of DataBinBox(Of T))
            Dim v As T() = data _
                .OrderBy(Function(x) eval(x)) _
                .ToArray
            ' 计算出每一个bin之中的元素数目
            Dim size% = v.Length / k

            Return v _
                .SplitIterator(size) _
                .Select(Function(block)
                            Return New DataBinBox(Of T)(block, eval)
                        End Function)
        End Function
    End Module
End Namespace
