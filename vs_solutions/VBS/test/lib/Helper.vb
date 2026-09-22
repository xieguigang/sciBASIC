' =============================================================
' 被 #include 引入的脚本样例
'
' 限制(因为其代码会被直接复制到主脚本的顶层命名空间):
'   1. 不允许出现脚本引擎的魔法方法(ScriptDir/Here/Package/...);
'   2. 不允许顶层可执行语句;
'   3. 不允许顶层 Function/Sub(仅类型定义与 Imports);
'   4. 不允许 #package/#author/#title/#version 元数据定义指令。
' =============================================================
Imports System.Text

Public Class LibraryHelper

    Public Shared Function Greet(name As String) As String
        Return $"hello, {name}!"
    End Function

    Public Shared Function BuildReport(items As String()) As String
        Dim sb As New StringBuilder()

        For Each item As String In items
            Call sb.AppendLine("  - " & item)
        Next

        Return sb.ToString()
    End Function
End Class

Public Structure Vector2

    Public X As Double
    Public Y As Double

    Public Overrides Function ToString() As String
        Return $"({X}, {Y})"
    End Function
End Structure

Public Module LibraryAlgebra

    ''' <summary>被引入脚本之中的类型可以互相引用, 也可以引用被引入脚本自身的 Imports</summary>
    Public Function Scale(v As Vector2, factor As Double) As Vector2
        Return New Vector2 With {
            .X = v.X * factor,
            .Y = v.Y * factor
        }
    End Function
End Module
