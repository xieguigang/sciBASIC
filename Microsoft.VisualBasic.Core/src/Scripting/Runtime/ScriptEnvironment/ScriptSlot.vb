Namespace Scripting.Runtime

    ''' <summary>
    ''' 变量槽位：封装变量的类型、值和元数据
    ''' </summary>
    Public Class ScriptSlot

        ' — 元数据 —
        Public Property VarType As TypeCode = TypeCode.Empty
        Public Property IsReadOnly As Boolean = False
        Public Property IsConst As Boolean = False

        ' --- 强类型值存储 (模拟 Union，避免装箱) ---
        ' 只有与 VarType 对应的字段才是有效数据
        Public BoolValue As Boolean
        Public IntValue As Integer
        Public DblValue As Double
        Public StrValue As String
        Public ObjValue As Object

        ' --- 强类型 Set 方法 (无装箱) ---
        Public Sub SetBoolean(value As Boolean)
            Me.ClearValues() : VarType = TypeCode.Boolean : BoolValue = value
        End Sub

        Public Sub SetInteger(value As Integer)
            Me.ClearValues() : VarType = TypeCode.Int32 : IntValue = value
        End Sub

        Public Sub SetDouble(value As Double)
            Me.ClearValues() : VarType = TypeCode.Double : DblValue = value
        End Sub

        Public Sub SetString(value As String)
            Me.ClearValues() : VarType = TypeCode.String : StrValue = value
        End Sub

        Public Sub SetObject(value As Object)
            Me.ClearValues() : VarType = TypeCode.Object : ObjValue = value
        End Sub

        ' --- 通用 Set 方法 (可能会发生一次拆箱，用于外部 Object 传入时) ---
        Public Sub SetValue(value As Object)
            If value Is Nothing Then
                ClearValues()
            Else
                Dim t = value.GetType()

                If t Is GetType(Integer) Then
                    SetInteger(DirectCast(value, Integer))
                ElseIf t Is GetType(Double) Then
                    SetDouble(DirectCast(value, Double))
                ElseIf t Is GetType(Boolean) Then
                    SetBoolean(DirectCast(value, Boolean))
                ElseIf t Is GetType(String) Then
                    SetString(DirectCast(value, String))
                Else
                    ' 其他类型统一当 Object 处理
                    SetObject(value)
                End If
            End If
        End Sub

        ' --- 通用 Get 方法 (返回 Object，读取基础类型时会发生装箱，应尽量避免在引擎内部核心循环使用) ---
        Public Function GetValue() As Object
            Select Case VarType
                Case TypeCode.Empty, TypeCode.DBNull : Return Nothing
                Case TypeCode.Boolean : Return BoolValue
                Case TypeCode.Int32 : Return IntValue
                Case TypeCode.Double : Return DblValue
                Case TypeCode.String : Return StrValue
                Case TypeCode.Object : Return ObjValue

                Case Else
                    Return Nothing
            End Select
        End Function

        ' 辅助方法：切换类型时，清空旧值（特别是引用类型，防止内存泄漏）
        Private Sub ClearValues()
            BoolValue = False
            IntValue = 0
            DblValue = 0.0
            StrValue = Nothing
            ObjValue = Nothing ' 释放旧引用
            VarType = TypeCode.Empty
        End Sub
    End Class
End Namespace