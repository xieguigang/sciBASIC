Imports Microsoft.VisualBasic.Language.C
Imports stdNum = System.Math

Public Module StringFormats

    ''' <summary>
    ''' 对bytes数值进行格式自动优化显示
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <returns>经过自动格式优化过后的大小显示字符串</returns>
    Public Function Lanudry(bytes As Integer) As String
        If bytes <= 0 Then
            Return "0 B"
        End If

        Dim symbols = {"B", "KB", "MB", "GB", "TB"}
        Dim exp = stdNum.Floor(stdNum.Log(bytes) / stdNum.Log(1000))
        Dim symbol = symbols(exp)
        Dim val = (bytes / (1000 ^ stdNum.Floor(exp)))

        Return sprintf($"%.2f %s", val, symbol)
    End Function
End Module
