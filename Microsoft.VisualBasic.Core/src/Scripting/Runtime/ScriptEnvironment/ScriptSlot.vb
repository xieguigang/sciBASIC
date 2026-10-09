#Region "Microsoft.VisualBasic::f6986f2d7d945f82d9380eb2d3dc6586, Microsoft.VisualBasic.Core\src\Scripting\Runtime\ScriptEnvironment\ScriptSlot.vb"

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

    '   Total Lines: 188
    '    Code Lines: 128 (68.09%)
    ' Comment Lines: 33 (17.55%)
    '    - Xml Docs: 42.42%
    ' 
    '   Blank Lines: 25 (14.97%)
    '     File Size: 6.72 KB


    '     Class ScriptSlot
    ' 
    '         Properties: BoolValue, DateValue, DblValue, IntValue, IsConst
    '                     IsReadOnly, LngValue, ObjValue, SngValue, StrValue
    '                     VarType
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetJsValue, GetValue
    ' 
    '         Sub: ClearValues, (+2 Overloads) Dispose, SetBoolean, SetDate, SetDouble
    '              SetInteger, SetJsValue, SetLong, SetObject, SetSingle, SetString
    '              SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.Runtime

    ''' <summary>
    ''' 变量槽位：封装变量的类型、值和元数据
    ''' </summary>
    ''' <remarks>
    ''' 内部的值存储统一由 <see cref="JsValue"/> 标签联合体承载：
    ''' CLR 基础类型（Double/Int32/Boolean/...）在槽位内以强类型字段保存，
    ''' 读写均不发生装箱；只有在通过 <see cref="GetValue"/> 跨越宿主边界时
    ''' 才会执行一次装箱。
    ''' </remarks>
    Public Class ScriptSlot : Implements IDisposable

        Private disposedValue As Boolean

        ' — 值存储（JsValue 联合体） —
        Private _v As JsValue

        ' — 元数据 —
        ''' <summary>当前槽位值的数据类型标签</summary>
        Public ReadOnly Property VarType As TypeCode
            Get
                Return _v.VarType
            End Get
        End Property

        ''' <summary>
        ''' current symbol value is constant lock binding in the environment?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsReadOnly As Boolean
            Get
                Return _readonly
            End Get
        End Property

        Protected _readonly As Boolean = False

        Public Property IsConst As Boolean = False

        ' --- 强类型值读取 (无装箱) ---

        Public ReadOnly Property BoolValue As Boolean
            Get
                If _v.VarType = TypeCode.Boolean Then
                    Return _v.AsBoolean()
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property IntValue As Integer
            Get
                If _v.VarType = TypeCode.Int32 Then
                    Return _v.AsInt32()
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property DblValue As Double
            Get
                If _v.VarType = TypeCode.Double Then
                    Return _v.AsDouble()
                Else
                    Return 0.0
                End If
            End Get
        End Property

        Public ReadOnly Property StrValue As String
            Get
                If _v.VarType = TypeCode.String Then
                    Return _v.AsString()
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' .NET clr class object value
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ObjValue As Object
            Get
                If _v.VarType = TypeCode.Object Then
                    Return _v._obj
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public ReadOnly Property SngValue As Single
            Get
                If _v.VarType = TypeCode.Single Then
                    Return _v.AsSingle()
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property LngValue As Long
            Get
                If _v.VarType = TypeCode.Int64 Then
                    Return _v.AsInt64()
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property DateValue As Date
            Get
                If _v.VarType = TypeCode.DateTime Then
                    Return New Date(_v._lng)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(is_readonly As Boolean)
            _readonly = is_readonly
        End Sub

        ' --- 强类型 Set 方法 (无装箱) ---

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="value"></param>
        Public Sub SetBoolean(value As Boolean)
            _v = JsValue.Boolean_(value)
        End Sub

        Public Sub SetInteger(value As Integer)
            _v = JsValue.Int32(value)
        End Sub

        Public Sub SetDouble(value As Double)
            _v = JsValue.Number(value)
        End Sub

        Public Sub SetString(value As String)
            _v = JsValue.Str(value)
        End Sub

        Public Sub SetObject(value As Object)
            _v = JsValue.OfObject(value)
        End Sub

        Public Sub SetLong(value As Long)
            _v = JsValue.Int64(value)
        End Sub

        Public Sub SetSingle(value As Single)
            _v = JsValue.Single_(value)
        End Sub

        Public Sub SetDate(value As Date)
            _v = JsValue.Date_(value)
        End Sub

        ' ========================================================
        ' JsValue 直接读写接口 (解释器热路径专用，零装箱)
        ' ========================================================

        ''' <summary>以零装箱的方式读取槽位当前值</summary>
        Public Function GetJsValue() As JsValue
            Return _v
        End Function

        ''' <summary>以零装箱的方式写入槽位值</summary>
        Public Sub SetJsValue(v As JsValue)
            _v = v
        End Sub

        ' --- 通用 Set 方法 (用于外部 Object 传入时，仅在边界执行一次转换) ---
        Public Sub SetValue(value As Object)
            If value Is Nothing Then
                Call ClearValues()
            Else
                _v = JsValue.OfObject(value)
            End If
        End Sub

        ' --- 通用 Get 方法 (返回 Object，读取基础类型时会发生一次装箱，应尽量避免在引擎内部核心循环使用) ---
        Public Function GetValue() As Object
            Return _v.AsObject()
        End Function

        ' 清空槽位值（切换类型时释放旧引用，防止内存泄漏）
        Protected Sub ClearValues()
            _v = JsValue.Undef
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
