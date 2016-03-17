Public Module ValueMapping

    ''' <summary>
    ''' Gets the modal number of the ranking mapping data set.(求取众数)
    ''' </summary>
    ''' <param name="data">The ranked mapping encoding value.(经过Rank Mapping处理过后的编码值)</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 当不存在相同的分组元素数目的时候，会直接取第一个元素的值作为众数
    ''' 当存在相同的分组元素数目的时候，会取最大的元素值作为众数
    ''' </remarks>
    Public Function ModalNumber(data As Integer()) As Integer
        Dim Avg As Double = data.Average
        Dim Min = (From n In data Where n < Avg Select n).ToArray
        Dim Max = (From n In data Where n >= Avg Select n).ToArray
        Dim Mdn As Integer

        If Min.Length > Max.Length Then
            Mdn = Min.Average
        Else
            Mdn = Max.Average
        End If

        Return Mdn
    End Function
End Module
