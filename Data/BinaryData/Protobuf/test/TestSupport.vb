Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf.test
'  Data\BinaryData\Protobuf\test\TestSupport.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' 测试断言与辅助函数，统一走 <see cref="DebugAudit"/> 记录结果。
''' </summary>
Friend Module TestSupport

    ''' <summary>
    ''' 把字节数组格式化为 <c>0A 01 61</c> 形式的十六进制文本。
    ''' </summary>
    Friend Function ToHex(bytes As Byte()) As String
        If bytes Is Nothing Then
            Return "<null>"
        End If

        Dim parts As New List(Of String)

        For Each b As Byte In bytes
            Call parts.Add(b.ToString("X2"))
        Next

        Return String.Join(" ", parts)
    End Function

    ''' <summary>
    ''' 把 <c>0A 01 61</c> 形式的十六进制文本解析为字节数组。
    ''' </summary>
    Friend Function HexBytes(hex As String) As Byte()
        Dim parts As String() = hex.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim buffer(0 To parts.Length - 1) As Byte

        For i As Integer = 0 To parts.Length - 1
            buffer(i) = Convert.ToByte(parts(i), 16)
        Next

        Return buffer
    End Function

    ''' <summary>
    ''' 断言两个字节数组完全一致。
    ''' </summary>
    Friend Sub AssertBytesEqual(expected As Byte(), actual As Byte(), message As String)
        Dim equal As Boolean = expected IsNot Nothing AndAlso actual IsNot Nothing AndAlso
                               expected.Length = actual.Length

        If equal Then
            For i As Integer = 0 To expected.Length - 1
                If expected(i) <> actual(i) Then
                    equal = False
                    Exit For
                End If
            Next
        End If

        If equal Then
            Call DebugAudit.Test(True, message)
        Else
            Call DebugAudit.Test(False, $"{message}：期望 [{ToHex(expected)}]，实际 [{ToHex(actual)}]")
        End If
    End Sub

    ''' <summary>
    ''' 断言两个值相等。
    ''' </summary>
    Friend Sub AssertEqual(Of T)(expected As T, actual As T, message As String)
        If EqualityComparer(Of T).Default.Equals(expected, actual) Then
            Call DebugAudit.Test(True, message)
        Else
            Call DebugAudit.Test(False, $"{message}：期望 {DebugAudit.ValueText(expected)}，实际 {DebugAudit.ValueText(actual)}")
        End If
    End Sub

    ''' <summary>
    ''' 断言条件成立。
    ''' </summary>
    Friend Sub AssertTrue(condition As Boolean, message As String)
        Call DebugAudit.Test(condition, message)
    End Sub

    ''' <summary>
    ''' 断言条件不成立。
    ''' </summary>
    Friend Sub AssertFalse(condition As Boolean, message As String)
        Call DebugAudit.Test(Not condition, message)
    End Sub

    ''' <summary>
    ''' 断言文本包含指定片段。
    ''' </summary>
    Friend Sub AssertContains(text As String, fragment As String, message As String)
        If text IsNot Nothing AndAlso text.Contains(fragment) Then
            Call DebugAudit.Test(True, message)
        Else
            Call DebugAudit.Test(False, $"{message}：文本中没有找到 '{fragment}'")
        End If
    End Sub

    ''' <summary>
    ''' 断言执行指定的代码块会抛出 <typeparamref name="TException"/>。
    ''' </summary>
    Friend Sub AssertThrows(Of TException As Exception)(code As Action, message As String)
        Try
            Call code()
            Call DebugAudit.Test(False, $"{message}：期望抛出 {GetType(TException).Name}，但执行成功")
        Catch ex As TException
            Call DebugAudit.Test(True, message)
        Catch ex As Exception
            Call DebugAudit.Test(False, $"{message}：期望抛出 {GetType(TException).Name}，实际抛出 {ex.GetType().Name}")
        End Try
    End Sub

    ''' <summary>
    ''' 创建一个唯一的临时目录。
    ''' </summary>
    Friend Function TempDir() As String
        Dim dirPath As String = Path.Combine(Path.GetTempPath(), "protobuf-demo-" & Guid.NewGuid().ToString("N"))

        Call Directory.CreateDirectory(dirPath)

        Return dirPath
    End Function

End Module
