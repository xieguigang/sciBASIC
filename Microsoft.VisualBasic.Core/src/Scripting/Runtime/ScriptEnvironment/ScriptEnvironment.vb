#Region "Microsoft.VisualBasic::bcf03581386472bd0e7c140c36f66a4f, Microsoft.VisualBasic.Core\src\Scripting\Runtime\ScriptEnvironment\ScriptEnvironment.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 167
    '    Code Lines: 102 (61.08%)
    ' Comment Lines: 40 (23.95%)
    '    - Xml Docs: 45.00%
    ' 
    '   Blank Lines: 25 (14.97%)
    '     File Size: 6.39 KB


    '     Class ScriptEnvironment
    ' 
    '         Function: DefineVariable, FindSlot, GetInt, GetValue
    ' 
    '         Sub: CheckReadOnly, (+2 Overloads) Dispose, SetDouble, SetInt, SetValue
    ' 
    '     Class NestedScriptEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FindSlot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.Runtime

    ''' <summary>
    ''' 脚本运行环境
    ''' </summary>
    Public Class ScriptEnvironment : Implements IDisposable

        ''' <summary>
        ''' 当前作用域的变量字典
        ''' </summary>
        Protected ReadOnly _slots As New Dictionary(Of String, ScriptSlot)
        Private disposedValue As Boolean

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
            Dim slot = New ScriptSlot(is_readonly:=isReadOnly)
            _slots(name) = slot
            Return slot
        End Function

        ''' <summary>
        ''' 查找变量槽位，支持向父级作用域查找
        ''' </summary>
        Public Overridable Function FindSlot(name As String, throwIfNotFound As Boolean) As ScriptSlot
            Dim slot As ScriptSlot = Nothing

            If _slots.TryGetValue(name, slot) Then
                Return slot
            ElseIf throwIfNotFound Then
                Throw New Exception($"未定义的变量: '{name}'")
            Else
                Return Nothing
            End If
        End Function

        Private Sub CheckReadOnly(slot As ScriptSlot, name As String)
            If slot.IsReadOnly OrElse slot.IsConst Then
                Throw New Exception($"无法修改只读变量: '{name}'")
            End If
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    For Each slot As ScriptSlot In _slots.Values
                        Call slot.Dispose()
                    Next

                    Call _slots.Clear()
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

    ''' <summary>
    ''' 脚本运行环境（支持作用域嵌套）
    ''' </summary>
    Public Class NestedScriptEnvironment : Inherits ScriptEnvironment

        ''' <summary>
        ''' 父级作用域（用于实现闭包或块级作用域）
        ''' </summary>
        ReadOnly _parent As ScriptEnvironment

        Public Sub New(Optional parent As ScriptEnvironment = Nothing)
            _parent = parent
        End Sub

        Public Overrides Function FindSlot(name As String, throwIfNotFound As Boolean) As ScriptSlot
            Dim currentEnv = Me
            While currentEnv IsNot Nothing
                Dim slot As ScriptSlot = Nothing
                If currentEnv._slots.TryGetValue(name, slot) Then
                    Return slot
                End If
                currentEnv = currentEnv._parent
            End While

            If throwIfNotFound Then
                Throw New Exception($"未定义的变量: '{name}'")
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
