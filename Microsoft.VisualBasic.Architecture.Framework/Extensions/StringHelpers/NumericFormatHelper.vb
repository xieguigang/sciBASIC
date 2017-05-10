''' <summary>
''' ###### ``C``货币
'''
''' ```vbnet
''' 2.5.ToString("C")
''' ' ￥2.50
''' ```
'''
''' ###### ``D``十进制数
'''
''' ```vbnet
''' 25.ToString("D5")
''' ' 00025
''' ```
''' 
''' ###### ``E``科学型
'''
''' ```vbnet
''' 25000.ToString("E")
''' ' 2.500000E+005
''' ```
''' 
''' ###### ``F``固定点
'''
''' ```vbnet
''' 25.ToString("F2")
''' ' 25.00
''' ```
''' 
''' ###### ``G``常规
'''
''' ```vbnet
''' 2.5.ToString("G")
''' ' 2.5
''' ```
'''
''' ###### ``N``数字
'''
''' ```vbnet
''' 2500000.ToString("N")
''' ' 2,500,000.00
''' ```
''' 
''' ###### ``X``十六进制
'''
''' ```vbnet
''' 255.ToString("X")
''' ' FF
''' ```
''' </summary>
Public Module NumericFormatHelper

    ''' <summary>
    ''' ``D&lt;n>``
    ''' </summary>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Function [Decimal](n%) As String
        Return "D" & n
    End Function

    ''' <summary>
    ''' ``F&lt;n>``
    ''' </summary>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    Public Function Float(n%) As String
        Return "F" & n
    End Function
End Module
