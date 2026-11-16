' ------------------------------------------------------------------------
' HRESULT 判定与 DirectML 异常
' ------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>
    ''' HRESULT 的成功判定、文本描述与统一抛出。
    ''' </summary>
    Public Module HResult

        ''' <summary>S_OK</summary>
        Public Const Ok As Integer = 0
        ''' <summary>S_FALSE</summary>
        Public Const FalseResult As Integer = 1
        ''' <summary>DXGI_ERROR_NOT_FOUND</summary>
        Public Const DxgiNotFound As Integer = -2005270526
        ''' <summary>DXGI_ERROR_UNSUPPORTED</summary>
        Public Const DxgiUnsupported As Integer = -2005270524
        ''' <summary>DXGI_ERROR_NOT_CURRENTLY_AVAILABLE</summary>
        Public Const DxgiNotCurrentlyAvailable As Integer = -2005270494

        ''' <summary>判断 HRESULT 是否表示成功（最高位为 0）。</summary>
        Public Function Succeeded(hr As Integer) As Boolean
            Return hr >= 0
        End Function

        ''' <summary>判断 HRESULT 是否表示失败（最高位为 1）。</summary>
        Public Function Failed(hr As Integer) As Boolean
            Return hr < 0
        End Function

        ''' <summary>
        ''' 生成 HRESULT 的可读描述（优先使用系统提供的 COM 异常文本）。
        ''' </summary>
        ''' <param name="hr">HRESULT 值。</param>
        ''' <returns>形如 <c>0x887A0005 (The GPU device instance has been suspended)</c> 的文本。</returns>
        Public Function Describe(hr As Integer) As String
            Dim text As String = Nothing

            If hr = DxgiNotFound Then
                Return $"0x{hr:X8} (DXGI_ERROR_NOT_FOUND)"
            ElseIf hr = DxgiUnsupported Then
                Return $"0x{hr:X8} (DXGI_ERROR_UNSUPPORTED)"
            ElseIf hr = DxgiNotCurrentlyAvailable Then
                Return $"0x{hr:X8} (DXGI_ERROR_NOT_CURRENTLY_AVAILABLE)"
            End If

            Try
                Dim ex As Exception = Marshal.GetExceptionForHR(hr)

                If ex IsNot Nothing Then
                    text = ex.Message
                End If
            Catch
                text = Nothing
            End Try

            If String.IsNullOrEmpty(text) Then
                Return $"0x{hr:X8}"
            End If

            Return $"0x{hr:X8} ({text})"
        End Function

        ''' <summary>
        ''' HRESULT 失败时抛出 <see cref="DmlException"/>。
        ''' </summary>
        ''' <param name="hr">HRESULT 值。</param>
        ''' <param name="apiName">出错的 API 名称（由编译器自动填充）。</param>
        ''' <returns>原 HRESULT（成功时）。</returns>
        Public Function ThrowIfFailed(hr As Integer, <CallerMemberName> Optional apiName As String = Nothing) As Integer
            If Failed(hr) Then
                Throw New DmlException(hr, apiName)
            End If

            Return hr
        End Function

        ''' <summary>
        ''' 检查 BOOL 返回值（Win32 风格 API），为 0 时抛出异常。
        ''' </summary>
        ''' <param name="value">BOOL 值。</param>
        ''' <param name="apiName">出错的 API 名称。</param>
        Public Sub ThrowIfFalse(value As Boolean, <CallerMemberName> Optional apiName As String = Nothing)
            If Not value Then
                Throw New DmlException(-2147467259, apiName, "原生调用返回 FALSE")
            End If
        End Sub

    End Module

    ''' <summary>
    ''' DirectML / D3D12 调用失败时抛出的异常。
    ''' </summary>
    Public Class DmlException
        Inherits Exception

        ''' <summary>原始 HRESULT。</summary>
        Public ReadOnly Property ErrorCode As Integer

        ''' <summary>出错的 API 名称。</summary>
        Public ReadOnly Property ApiName As String

        ''' <summary>创建异常。</summary>
        ''' <param name="hr">HRESULT。</param>
        ''' <param name="apiName">API 名称。</param>
        ''' <param name="detail">补充说明。</param>
        Public Sub New(hr As Integer, Optional apiName As String = Nothing, Optional detail As String = Nothing)
            MyBase.New(BuildMessage(hr, apiName, detail))
            Me.ErrorCode = hr
            Me.ApiName = apiName
        End Sub

        Private Shared Function BuildMessage(hr As Integer, apiName As String, detail As String) As String
            Dim name As String = If(String.IsNullOrEmpty(apiName), "<native call>", apiName)
            Dim text As String = $"{name} 调用失败：{Interop.HResult.Describe(hr)}"

            If Not String.IsNullOrEmpty(detail) Then
                text &= "；" & detail
            End If

            Return text
        End Function

        Public Overrides Function ToString() As String
            Return Message
        End Function

    End Class

End Namespace
