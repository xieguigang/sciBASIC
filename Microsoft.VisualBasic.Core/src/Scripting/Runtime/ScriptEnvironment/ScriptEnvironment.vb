Namespace Scripting.Runtime

    ''' <summary>
    ''' 脚本运行环境（支持作用域嵌套）
    ''' </summary>
    Public Class ScriptEnvironment
        ' 当前作用域的变量字典
        Private ReadOnly _slots As New Dictionary(Of String, ScriptSlot)

        ' 父级作用域（用于实现闭包或块级作用域）
        Private ReadOnly _parent As ScriptEnvironment

        Public Sub New(Optional parent As ScriptEnvironment = Nothing)
            _parent = parent
        End Sub

        ' ========================================================
        ' 核心优化：强类型读写接口 (脚本引擎内部执行时优先使用)
        ' ========================================================

        Public Function GetInt(name As String) As Integer
            Dim slot = FindSlot(name, True)
            ' 简单的隐式转换支持 (如果脚本允许 Double 转 Integer)
            If slot.VarType = TypeCode.Int32 Then Return slot.IntValue
            If slot.VarType = TypeCode.Double Then Return CInt(slot.DblValue)
            Throw New Exception($"变量 '{name}' 不是数字类型")
        End Function

        Public Sub SetInt(name As String, value As Integer)
            Dim slot = FindSlot(name, False)
            If slot IsNot Nothing Then
                CheckReadOnly(slot, name)
                slot.SetInteger(value)
            Else
                ' 变量不存在，隐式声明
                DefineVariable(name).SetInteger(value)
            End If
        End Sub

        ' Double, Boolean, String 等以此类推...
        Public Sub SetDouble(name As String, value As Double)
            Dim slot = FindSlot(name, False)
            If slot IsNot Nothing Then
                CheckReadOnly(slot, name)
                slot.SetDouble(value)
            Else
                DefineVariable(name).SetDouble(value)
            End If
        End Sub


        ' ========================================================
        ' 通用读写接口 (用于与外部宿主程序交互，或动态调用)
        ' ========================================================

        Public Function GetValue(name As String) As Object
            Dim slot = FindSlot(name, True)
            Return slot.GetValue()
        End Function

        Public Sub SetValue(name As String, value As Object)
            Dim slot = FindSlot(name, False)
            If slot IsNot Nothing Then
                CheckReadOnly(slot, name)
                slot.SetValue(value)
            Else
                DefineVariable(name).SetValue(value)
            End If
        End Sub

        ' ========================================================
        ' 变量声明与作用域管理
        ' ========================================================

        ''' <summary>
        ''' 显式声明一个变量
        ''' </summary>
        Public Function DefineVariable(name As String, Optional isReadOnly As Boolean = False) As ScriptSlot
            If _slots.ContainsKey(name) Then
                Throw New Exception($"变量 '{name}' 已在当前作用域中定义")
            End If
            Dim slot = New ScriptSlot() With {.IsReadOnly = isReadOnly}
            _slots(name) = slot
            Return slot
        End Function

        ''' <summary>
        ''' 查找变量槽位，支持向父级作用域查找
        ''' </summary>
        Private Function FindSlot(name As String, throwIfNotFound As Boolean) As ScriptSlot
            Dim currentEnv = Me
            While currentEnv IsNot Nothing
                Dim slot As ScriptSlot = Nothing
                If currentEnv._slots.TryGetValue(name, slot) Then
                    Return slot
                End If
                currentEnv = currentEnv._parent
            End While

            If throwIfNotFound Then Throw New Exception($"未定义的变量: '{name}'")
            Return Nothing
        End Function

        Private Sub CheckReadOnly(slot As ScriptSlot, name As String)
            If slot.IsReadOnly OrElse slot.IsConst Then
                Throw New Exception($"无法修改只读变量: '{name}'")
            End If
        End Sub
    End Class
End Namespace