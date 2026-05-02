Namespace Scripting.Runtime

    ''' <summary>
    ''' 变量槽位：封装变量的类型、值和元数据
    ''' </summary>
    Public Class ScriptSlot : Implements IDisposable

        Private disposedValue As Boolean

        ' — 元数据 —
        Public ReadOnly Property VarType As TypeCode = TypeCode.Empty
        Public Property IsReadOnly As Boolean = False
        Public Property IsConst As Boolean = False

        ' --- 强类型值存储 (模拟 Union，避免装箱) ---
        ' 只有与 VarType 对应的字段才是有效数据
        Public ReadOnly Property BoolValue As Boolean
        Public ReadOnly Property IntValue As Integer
        Public ReadOnly Property DblValue As Double
        Public ReadOnly Property StrValue As String
        Public ReadOnly Property ObjValue As Object
        Public ReadOnly Property SngValue As Single
        Public ReadOnly Property LngValue As Long
        Public ReadOnly Property DateValue As Date

        ' --- 强类型 Set 方法 (无装箱) ---

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="value"></param>
        Public Sub SetBoolean(value As Boolean)
            Call ClearValues()
            _VarType = TypeCode.Boolean
            _BoolValue = value
        End Sub

        Public Sub SetInteger(value As Integer)
            Call ClearValues()
            _VarType = TypeCode.Int32
            _IntValue = value
        End Sub

        Public Sub SetDouble(value As Double)
            Call ClearValues()
            _VarType = TypeCode.Double
            _DblValue = value
        End Sub

        Public Sub SetString(value As String)
            Call ClearValues()
            _VarType = TypeCode.String
            _StrValue = value
        End Sub

        Public Sub SetObject(value As Object)
            Call ClearValues()
            _VarType = TypeCode.Object
            _ObjValue = value
        End Sub

        Public Sub SetLong(value As Long)
            Call ClearValues()
            _VarType = TypeCode.Int64
            _LngValue = value
        End Sub

        Public Sub SetSingle(value As Single)
            Call ClearValues()
            _VarType = TypeCode.Single
            _SngValue = value
        End Sub

        Public Sub SetDate(value As Date)
            Call ClearValues()
            _VarType = TypeCode.DateTime
            _DateValue = value
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
                Case TypeCode.Single : Return SngValue
                Case TypeCode.Int64 : Return LngValue
                Case TypeCode.DateTime : Return DateValue

                Case Else
                    Return Nothing
            End Select
        End Function

        ' 辅助方法：切换类型时，清空旧值（特别是引用类型，防止内存泄漏）
        Private Sub ClearValues()
            _BoolValue = False
            _IntValue = 0
            _DblValue = 0.0
            _SngValue = 0
            _LngValue = 0
            _DateValue = Nothing
            _StrValue = Nothing
            _ObjValue = Nothing ' 释放旧引用
            _VarType = TypeCode.Empty
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call ClearValues()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace