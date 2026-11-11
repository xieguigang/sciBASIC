Imports System
Imports System.Globalization

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 模型包的文本格式约定：数字、时间戳与区域设置。
    ''' </summary>
    ''' <remarks>
    ''' 模型文件必须在任何机器上往返一致，因此：
    ''' <list type="bullet">
    ''' <item>一律使用 <see cref="CultureInfo.InvariantCulture"/>，避免
    '''       「小数点为逗号」的区域设置破坏 CSV 的分隔语义；</item>
    ''' <item>浮点数使用 <c>"R"</c> 往返格式，保证<br/>
    '''       <c>Parse(Format(x)) = x</c> 逐位成立；</item>
    ''' <item>NaN / Infinity 显式写成 <c>NaN</c> / <c>Infinity</c>
    '''       （.NET 的 invariant 解析器可识别），而不是静默变成 0。</item>
    ''' </list>
    ''' </remarks>
    Public Module ModelFormat

        ''' <summary>浮点数的往返格式（<c>"R"</c> + invariant culture）</summary>
        Public Function Number(value As Double) As String
            Return value.ToString("R", CultureInfo.InvariantCulture)
        End Function

        ''' <summary>把文本解析为浮点数，失败时抛出带上下文的异常</summary>
        ''' <param name="text">待解析的文本</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        Public Function ParseNumber(text As String, context As String) As Double
            Dim value As Double

            If Not Double.TryParse(If(text, "").Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                Throw New FormatException($"{context}: '{text}' is not a valid number!")
            End If

            Return value
        End Function

        ''' <summary>把文本解析为整数，失败时抛出带上下文的异常</summary>
        ''' <param name="text">待解析的文本</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        Public Function ParseInteger(text As String, context As String) As Integer
            Dim value As Integer

            If Not Integer.TryParse(If(text, "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, value) Then
                Throw New FormatException($"{context}: '{text}' is not a valid integer!")
            End If

            Return value
        End Function

        ''' <summary>模型包创建时间（ISO 8601，UTC）</summary>
        Public Function Timestamp() As String
            Return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
        End Function

    End Module

End Namespace
