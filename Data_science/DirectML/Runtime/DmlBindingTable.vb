' ------------------------------------------------------------------------
' DML 绑定表
'
' 顺序约定（与 DML 一致）：
'   输入（含常量权重）按算子描述的输入顺序；
'   输出按算子描述的输出顺序；
'   BindTemporaryResource / BindPersistentResource 各一个。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>DirectML 绑定表包装。</summary>
    Public NotInheritable Class DmlBindingTable
        Inherits ComObjectBase

        Private ReadOnly _bindInputs As DmlBindArrayFn
        Private ReadOnly _bindOutputs As DmlBindArrayFn
        Private ReadOnly _bindTemporary As DmlBindResourceFn
        Private ReadOnly _bindPersistent As DmlBindResourceFn
        Private ReadOnly _reset As DmlBindingTableResetFn

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._bindInputs = GetSlotDelegate(Of DmlBindArrayFn)(DmlApi.SlotBindInputs)
            Me._bindOutputs = GetSlotDelegate(Of DmlBindArrayFn)(DmlApi.SlotBindOutputs)
            Me._bindTemporary = GetSlotDelegate(Of DmlBindResourceFn)(DmlApi.SlotBindTemporaryResource)
            Me._bindPersistent = GetSlotDelegate(Of DmlBindResourceFn)(DmlApi.SlotBindPersistentResource)
            Me._reset = GetSlotDelegate(Of DmlBindingTableResetFn)(DmlApi.SlotBindingTableReset)
        End Sub

        ''' <summary>
        ''' 重新初始化绑定表（每次 dispatch 前都必须调用，之后再 Bind*）。
        ''' </summary>
        Public Sub Reset(dispatchable As IntPtr, heap As DmlDescriptorHeap,
                         firstDescriptor As UInteger, descriptorCount As UInteger)
            If heap Is Nothing Then
                Throw New ArgumentNullException(NameOf(heap))
            End If

            Dim desc As New DML_BINDING_TABLE_DESC With {
                .Dispatchable = dispatchable,
                .CPUDescriptorHandle = heap.GetCpuHandle(firstDescriptor),
                .GPUDescriptorHandle = heap.GetGpuHandle(firstDescriptor),
                .SizeInDescriptors = descriptorCount
            }

            Dim size As Integer = Marshal.SizeOf(Of DML_BINDING_TABLE_DESC)()
            Dim ptr As IntPtr = Marshal.AllocHGlobal(size)

            Try
                Call Marshal.StructureToPtr(desc, ptr, False)
                HResult.ThrowIfFailed(_reset(NativePointer, ptr), "IDMLBindingTable::Reset")
            Finally
                Call Marshal.FreeHGlobal(ptr)
            End Try
        End Sub

        ''' <summary>绑定输入（顺序必须与算子描述的输入一致）。</summary>
        Public Sub BindInputs(bindings As DML_BUFFER_BINDING())
            Bind(bindings, _bindInputs)
        End Sub

        ''' <summary>绑定输出。</summary>
        Public Sub BindOutputs(bindings As DML_BUFFER_BINDING())
            Bind(bindings, _bindOutputs)
        End Sub

        Private Sub Bind(bindings As DML_BUFFER_BINDING(), [call] As DmlBindArrayFn)
            If bindings Is Nothing OrElse bindings.Length = 0 Then
                Return
            End If

            Dim descSize As Integer = Marshal.SizeOf(Of DML_BINDING_DESC)()
            Dim bindingSize As Integer = Marshal.SizeOf(Of DML_BUFFER_BINDING)()
            Dim descPtr As IntPtr = Marshal.AllocHGlobal(descSize * bindings.Length)
            Dim bindingPtr As IntPtr = Marshal.AllocHGlobal(bindingSize * bindings.Length)

            Try
                For i As Integer = 0 To bindings.Length - 1
                    Dim element As New IntPtr(bindingPtr.ToInt64() + i * bindingSize)

                    Call Marshal.StructureToPtr(bindings(i), element, False)

                    Dim desc As New DML_BINDING_DESC(DML_BINDING_TYPE.BUFFER, element)

                    Call Marshal.StructureToPtr(desc, New IntPtr(descPtr.ToInt64() + i * descSize), False)
                Next

                Call [call](NativePointer, CUInt(bindings.Length), descPtr)
            Finally
                Call Marshal.FreeHGlobal(descPtr)
                Call Marshal.FreeHGlobal(bindingPtr)
            End Try
        End Sub

        ''' <summary>绑定临时资源（<see cref="DmlAllocation"/> 为空时表示不需要）。</summary>
        Public Sub BindTemporaryResource(allocation As DmlAllocation)
            BindOptionalResource(allocation, _bindTemporary)
        End Sub

        ''' <summary>绑定持久资源（DML 拥有的常量权重）。</summary>
        Public Sub BindPersistentResource(allocation As DmlAllocation)
            BindOptionalResource(allocation, _bindPersistent)
        End Sub

        Private Sub BindOptionalResource(allocation As DmlAllocation, binder As DmlBindResourceFn)
            If Not allocation.IsValid Then
                Call binder(NativePointer, IntPtr.Zero)

                Return
            End If

            Dim binding As New DML_BUFFER_BINDING(allocation.NativeResource, allocation.Offset, allocation.SizeInBytes)
            Dim bindingSize As Integer = Marshal.SizeOf(Of DML_BUFFER_BINDING)()
            Dim bindingPtr As IntPtr = Marshal.AllocHGlobal(bindingSize)

            Try
                Call Marshal.StructureToPtr(binding, bindingPtr, False)

                Dim desc As New DML_BINDING_DESC(DML_BINDING_TYPE.BUFFER, bindingPtr)
                Dim descSize As Integer = Marshal.SizeOf(Of DML_BINDING_DESC)()
                Dim descPtr As IntPtr = Marshal.AllocHGlobal(descSize)

                Try
                    Call Marshal.StructureToPtr(desc, descPtr, False)
                    Call binder(NativePointer, descPtr)
                Finally
                    Call Marshal.FreeHGlobal(descPtr)
                End Try
            Finally
                Call Marshal.FreeHGlobal(bindingPtr)
            End Try
        End Sub

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
