' ============================================================================
' CompatHelpers.vb — LPPSolution 所需扩展的最小独立实现（纯 BCL）。
' 若宿主工程（如 sciBASIC）已有同名扩展，删除本文件避免二义性。
' ============================================================================

Imports System
Imports System.Runtime.CompilerServices

Public Module CompatHelpers

    ''' <summary>数组为 Nothing 或长度 0</summary>
    <Extension>
    Public Function IsNullOrEmpty(arr As String()) As Boolean
        Return arr Is Nothing OrElse arr.Length = 0
    End Function

    ''' <summary>
    ''' 字符串空判定。whitespaceAsEmpty=True 时纯空白亦视为空
    ''' （对应 SolverError 中的 StringEmpty(, True) 调用）。
    ''' </summary>
    <Extension>
    Public Function StringEmpty(s As String,
                                Optional trim As Boolean = False,
                                Optional whitespaceAsEmpty As Boolean = False) As Boolean
        If s Is Nothing OrElse s.Length = 0 Then Return True
        If trim Then s = s.Trim()
        If whitespaceAsEmpty Then Return String.IsNullOrWhiteSpace(s)
        Return s.Length = 0
    End Function

End Module

''' <summary>名称-值对（GetSolution() 枚举用）</summary>
Public Class NamedValue(Of T)

    Public Property Name As String
    Public Property Value As T

    Public Sub New(name As String, value As T)
        Me.Name = name
        Me.Value = value
    End Sub

    Public Overrides Function ToString() As String
        Return Name & "=" & Value?.ToString()
    End Function

End Class
