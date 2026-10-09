' ---------------- environments ----------------

Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' Lexical scope: name → (value, isConst); undeclared writes go to the global scope.
''' </summary>
''' <remarks>
''' 复用 Core 库的 <see cref="NestedScriptEnvironment"/>（作用域链 + <see cref="ScriptSlot"/> 槽位），
''' 槽位内部以 <see cref="JsValue"/> 标签联合体承载值：
''' 解释器热路径（<see cref="TryGet(String, JsValue ByRef)"/>、<see cref="Define(String, JsValue, Boolean)"/>、
''' <see cref="Assign(String, JsValue)"/>、<see cref="LookupOrThrow"/>）全程零 box/unbox。
''' </remarks>
Public NotInheritable Class Environment
    Inherits NestedScriptEnvironment

    Public Sub New(parent As Environment)
        MyBase.New(parent)
    End Sub

    ' ========================================================
    ' 解释器热路径（JsValue，零装箱）
    ' ========================================================

    ''' <summary>沿作用域链查找变量并以零装箱方式返回值</summary>
    Public Function TryGet(name As String, ByRef value As JsValue) As Boolean
        Dim s = FindSlot(name, False)
        If s IsNot Nothing Then
            value = s.GetJsValue()
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>在当前作用域声明变量（JS var/函数提升语义：同作用域重声明为覆盖）</summary>
    Public Sub Define(name As String, value As JsValue, isConst As Boolean)
        Dim slot = DefineVariable(name, isReadOnly:=False, overwrite:=True)
        slot.SetJsValue(value)
        slot.IsConst = isConst
    End Sub

    ''' <summary>
    ''' 沿作用域链赋值；const 重赋值抛出 <see cref="JsRuntimeException.TypeError"/>；
    ''' 未声明变量按 JS 非严格模式语义落入全局根作用域。
    ''' </summary>
    Public Sub Assign(name As String, value As JsValue)
        Dim s = FindSlot(name, False)

        If s IsNot Nothing Then
            If s.IsConst Then
                Throw JsRuntimeException.TypeError("Assignment to constant variable '" & name & "'")
            End If
            s.SetJsValue(value)
        Else
            ' non-strict JS: implicit global
            Dim slot = Root().DefineVariable(name, isReadOnly:=False, overwrite:=True)
            slot.SetJsValue(value)
        End If
    End Sub

    ''' <summary>沿作用域链读取变量，未定义时抛出 JS ReferenceError（零装箱）</summary>
    Public Function LookupOrThrow(name As String) As JsValue
        Dim s = FindSlot(name, True)
        Return s.GetJsValue()
    End Function

    ' ========================================================
    ' 宿主边界兼容接口（Object，仅在边界执行一次装箱）
    ' ========================================================

    Public Function TryGet(name As String, ByRef value As Object) As Boolean
        Dim v As JsValue = Nothing
        If TryGet(name, v) Then
            value = v.AsObject()
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub Define(name As String, value As Object, isConst As Boolean)
        Define(name, JsRuntime.JsUnbox(value), isConst)
    End Sub

    Public Sub Assign(name As String, value As Object)
        Assign(name, JsRuntime.JsUnbox(value))
    End Sub

    ''' <summary>宿主边界读取：读取并装箱为 Object</summary>
    Public Function LookupObjectOrThrow(name As String) As Object
        Return LookupOrThrow(name).AsObject()
    End Function

    ' ========================================================
    ' 只读检查：抛出脚本引擎自己的异常类型，保证可以被 JS try/catch 捕获
    ' ========================================================

    Protected Overrides Sub CheckReadOnly(slot As ScriptSlot, name As String)
        If slot.IsConst Then
            Throw JsRuntimeException.TypeError("Assignment to constant variable '" & name & "'")
        ElseIf slot.IsReadOnly Then
            Throw JsRuntimeException.TypeError($"无法修改只读变量: '{name}'")
        End If
    End Sub
End Class
