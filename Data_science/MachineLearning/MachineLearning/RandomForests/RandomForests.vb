Imports System.Runtime.CompilerServices

Namespace RandomForests

    Public Module RandomForests

        ''' <summary>
        ''' 基尼系数的选择的标准就是每个子节点达到最高的纯度，即落在子节点中的所有观察都属于同一个分类，
        ''' 此时基尼系数最小，纯度最高，不确定度最小。
        ''' 
        ''' 基尼指数越大，说明不确定性就越大；基尼系数越小，不确定性越小，数据分割越彻底，越干净。
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Gini(p As IEnumerable(Of Double)) As Double
            Return 1 - (Aggregate pk As Double In p Into Sum(pk ^ 2))
        End Function
    End Module
End Namespace