Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.DataStorage.Parquet.test
'  Data\BinaryData\Parquet\test\TestSupport.vb
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.Framework

''' <summary>
''' Parquet demo 测试的断言与辅助函数，统一走 <see cref="DebugAudit"/> 记录结果。
''' </summary>
Friend Module TestSupport

    Private _checks As Integer

    ''' <summary>
    ''' 已执行的断言总数。
    ''' </summary>
    Friend ReadOnly Property Checks As Integer
        Get
            Return _checks
        End Get
    End Property

    Friend Sub ResetChecks()
        _checks = 0
    End Sub

    Private Sub Count()
        _checks += 1
    End Sub

    ''' <summary>
    ''' 断言两个值相等。
    ''' </summary>
    Friend Sub AssertEqual(Of T)(expected As T, actual As T, message As String)
        Call Count()

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
        Call Count()
        Call DebugAudit.Test(condition, message)
    End Sub

    ''' <summary>
    ''' 断言条件不成立。
    ''' </summary>
    Friend Sub AssertFalse(condition As Boolean, message As String)
        Call Count()
        Call DebugAudit.Test(Not condition, message)
    End Sub

    ''' <summary>
    ''' 断言执行指定的代码块会抛出指定异常。
    ''' </summary>
    Friend Sub AssertThrows(Of TException As Exception)(code As Action, message As String)
        Call Count()

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
    ''' 断言两个数组逐元素相等（<c>NaN</c> 视为相等）。
    ''' </summary>
    Friend Sub AssertArray(expected As Array, actual As Array, message As String)
        Call Count()

        If expected Is Nothing OrElse actual Is Nothing Then
            Call DebugAudit.Test(expected Is actual, $"{message}：数组为空")
            Return
        End If

        If expected.Length <> actual.Length Then
            Call DebugAudit.Test(False, $"{message}：长度不一致，期望 {expected.Length}，实际 {actual.Length}")
            Return
        End If

        For i As Integer = 0 To expected.Length - 1
            Dim a As Object = expected.GetValue(i)
            Dim b As Object = actual.GetValue(i)

            If Not ValueEquals(a, b) Then
                Call DebugAudit.Test(False, $"{message}：第 {i} 个元素不一致，期望 {DebugAudit.ValueText(a)}，实际 {DebugAudit.ValueText(b)}")
                Return
            End If
        Next

        Call DebugAudit.Test(True, message)
    End Sub

    Private Function ValueEquals(a As Object, b As Object) As Boolean
        If a Is Nothing OrElse b Is Nothing Then
            Return a Is b
        End If

        If TypeOf a Is Double AndAlso TypeOf b Is Double Then
            Dim x As Double = CDbl(a)
            Dim y As Double = CDbl(b)

            If Double.IsNaN(x) AndAlso Double.IsNaN(y) Then
                Return True
            End If
        End If

        If TypeOf a Is Single AndAlso TypeOf b Is Single Then
            Dim x As Single = CSng(a)
            Dim y As Single = CSng(b)

            If Single.IsNaN(x) AndAlso Single.IsNaN(y) Then
                Return True
            End If
        End If

        Return Object.Equals(a, b)
    End Function

    ''' <summary>
    ''' 创建一个唯一的临时目录。
    ''' </summary>
    Friend Function TempDir() As String
        Dim dirPath As String = System.IO.Path.Combine(Path.GetTempPath(), "parquet-demo-" & Guid.NewGuid().ToString("N"))

        Call Directory.CreateDirectory(dirPath)

        Return dirPath
    End Function

    ''' <summary>
    ''' 读取特征列底层数组。
    ''' </summary>
    Friend Function ColumnArray(df As DataFrame, name As String) As Array
        If Not df.features.ContainsKey(name) Then
            Return Nothing
        End If

        Return df.features(name).vector
    End Function

End Module
