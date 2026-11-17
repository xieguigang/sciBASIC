' ------------------------------------------------------------------------
' 编译后的 DML 算子（以及算子初始化器）
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' 编译后的 DirectML 算子：持有 <c>IDMLCompiledOperator</c> 与绑定属性。
    ''' </summary>
    Public NotInheritable Class DmlCompiledOperator
        Inherits ComObjectBase

        Private ReadOnly _getBindingProperties As DmlGetBindingPropertiesFn

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._getBindingProperties = GetSlotDelegate(Of DmlGetBindingPropertiesFn)(DmlApi.SlotGetBindingProperties)
            Me.BindingProperties = QueryBindingProperties()
        End Sub

        ''' <summary>描述符数量、临时资源与持久资源需求。</summary>
        Public ReadOnly Property BindingProperties As DML_BINDING_PROPERTIES

        Private Function QueryBindingProperties() As DML_BINDING_PROPERTIES
            Dim size As Integer = Marshal.SizeOf(Of DML_BINDING_PROPERTIES)()
            Dim buffer As IntPtr = Marshal.AllocHGlobal(size)

            Try
                Call _getBindingProperties(NativePointer, buffer)

                Dim raw As Byte() = New Byte(size - 1) {}

                Call Marshal.Copy(buffer, raw, 0, size)
                Me.RawBindingProperties = raw

                Return Marshal.PtrToStructure(Of DML_BINDING_PROPERTIES)(buffer)
            Finally
                Call Marshal.FreeHGlobal(buffer)
            End Try
        End Function

        ''' <summary>绑定属性的原始字节（诊断用）。</summary>
        Public Property RawBindingProperties As Byte()

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

    ''' <summary>
    ''' 算子初始化器：编译后的算子在第一次 dispatch 之前必须由它初始化一次。
    ''' </summary>
    Public NotInheritable Class DmlOperatorInitializer
        Inherits ComObjectBase

        Private ReadOnly _reset As DmlInitializerResetFn
        Private ReadOnly _getBindingProperties As DmlGetBindingPropertiesFn

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._reset = GetSlotDelegate(Of DmlInitializerResetFn)(DmlApi.SlotInitializerReset)
            Me._getBindingProperties = GetSlotDelegate(Of DmlGetBindingPropertiesFn)(DmlApi.SlotGetBindingProperties)
            Me.BindingProperties = QueryBindingProperties()
        End Sub

        ''' <summary>初始化器的绑定属性。</summary>
        Public ReadOnly Property BindingProperties As DML_BINDING_PROPERTIES

        Private Function QueryBindingProperties() As DML_BINDING_PROPERTIES
            Dim size As Integer = Marshal.SizeOf(Of DML_BINDING_PROPERTIES)()
            Dim buffer As IntPtr = Marshal.AllocHGlobal(size)

            Try
                Call _getBindingProperties(NativePointer, buffer)

                Return Marshal.PtrToStructure(Of DML_BINDING_PROPERTIES)(buffer)
            Finally
                Call Marshal.FreeHGlobal(buffer)
            End Try
        End Function

        ''' <summary>重置为另一批被编译算子。</summary>
        Public Sub Reset(operators As IntPtr())
            ThrowIfDisposed()

            Using builder As New DmlOperatorBuilder()
                Dim arrayPtr As IntPtr = builder.PointerArray(operators)
                Dim count As UInteger = If(operators Is Nothing, 0UI, CUInt(operators.Length))

                HResult.ThrowIfFailed(_reset(NativePointer, count, arrayPtr), "IDMLOperatorInitializer::Reset")
            End Using
        End Sub

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

    ''' <summary>命令记录器：把 Dispatch 写进命令列表。</summary>
    Public NotInheritable Class DmlCommandRecorder
        Inherits ComObjectBase

        Private ReadOnly _recordDispatch As DmlRecordDispatchFn

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._recordDispatch = GetSlotDelegate(Of DmlRecordDispatchFn)(DmlApi.SlotRecordDispatch)
        End Sub

        ''' <summary>记录一次 dispatch。</summary>
        Public Sub RecordDispatch(commandList As DmlCommandList, dispatchable As IntPtr, bindings As DmlBindingTable)
            If commandList Is Nothing Then
                Throw New ArgumentNullException(NameOf(commandList))
            End If

            If dispatchable = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(dispatchable))
            End If

            Dim table As IntPtr = IntPtr.Zero

            If bindings IsNot Nothing Then
                table = bindings.NativePointer
            End If

            Call _recordDispatch(NativePointer, commandList.NativePointer, dispatchable, table)
        End Sub

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
