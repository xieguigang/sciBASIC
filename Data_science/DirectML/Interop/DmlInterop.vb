' ------------------------------------------------------------------------
' 非托管内存的构建器
'
' DirectML 的描述符是"结构体 + 指针数组"的形式：
'   DML_OPERATOR_DESC { Type, Desc* }
'   DML_TENSOR_DESC   { Type, Desc* } -> DML_BUFFER_TENSOR_DESC { ..., UINT* Sizes, UINT* Strides, ... }
' 这些指针指向的内存必须活到 CreateOperator 返回。
' DmlOperatorBuilder 统一负责申请/持有/释放这些内存块，避免逐处使用 Using 造成悬垂指针。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>
    ''' 一组非托管内存块的构建器：所有通过它申请的内存都在 <see cref="Dispose"/> 时统一释放。
    ''' </summary>
    Public NotInheritable Class DmlOperatorBuilder
        Implements IDisposable

        Private ReadOnly _blocks As New List(Of IntPtr)
        Private _disposed As Boolean

        ''' <summary>申请一块已清零的非托管内存。</summary>
        ''' <param name="bytes">字节数。</param>
        ''' <returns>内存指针。</returns>
        Public Function Alloc(bytes As Integer) As IntPtr
            If _disposed Then
                Throw New ObjectDisposedException(NameOf(DmlOperatorBuilder))
            End If

            If bytes <= 0 Then
                bytes = 4
            End If

            Dim ptr As IntPtr = Marshal.AllocHGlobal(bytes)

            ' 清零：DirectML 会读取未显式赋值的可选字段
            Dim zeros As Byte() = New Byte(bytes - 1) {}

            Call Marshal.Copy(zeros, 0, ptr, bytes)
            Call _blocks.Add(ptr)

            Return ptr
        End Function

        ''' <summary>写入 <c>UINT[]</c>（没有元素时返回 <see cref="IntPtr.Zero"/>）。</summary>
        Public Function UIntArray(values As UInteger()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim ptr As IntPtr = Alloc(values.Length * 4)
            Dim tmp As Integer() = New Integer(values.Length - 1) {}

            ' UInteger -> Integer 只做位复制，避免 VB 的溢出检查
            Call Buffer.BlockCopy(values, 0, tmp, 0, values.Length * 4)
            Call Marshal.Copy(tmp, 0, ptr, tmp.Length)

            Return ptr
        End Function

        ''' <summary>写入 <c>INT[]</c>。</summary>
        Public Function IntArray(values As Integer()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim ptr As IntPtr = Alloc(values.Length * 4)

            Call Marshal.Copy(values, 0, ptr, values.Length)

            Return ptr
        End Function

        ''' <summary>写入 <c>FLOAT[]</c>。</summary>
        Public Function SingleArray(values As Single()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim ptr As IntPtr = Alloc(values.Length * 4)

            Call Marshal.Copy(values, 0, ptr, values.Length)

            Return ptr
        End Function

        ''' <summary>把结构体写入新申请的内存。</summary>
        Public Function StructPtr(Of T As Structure)(value As T) As IntPtr
            Dim ptr As IntPtr = Alloc(Marshal.SizeOf(Of T)())

            Call Marshal.StructureToPtr(value, ptr, False)

            Return ptr
        End Function

        ''' <summary>把结构体数组写入新申请的内存。</summary>
        Public Function StructArray(Of T As Structure)(values As T()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim size As Integer = Marshal.SizeOf(Of T)()
            Dim ptr As IntPtr = Alloc(size * values.Length)

            For i As Integer = 0 To values.Length - 1
                Call Marshal.StructureToPtr(values(i), New IntPtr(ptr.ToInt64() + i * size), False)
            Next

            Return ptr
        End Function

        ''' <summary>写入 <c>ULONG[]</c>。</summary>
        Public Function ULongArray(values As ULong()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim ptr As IntPtr = Alloc(values.Length * 8)
            Dim tmp As Long() = New Long(values.Length - 1) {}

            Call Buffer.BlockCopy(values, 0, tmp, 0, values.Length * 8)
            Call Marshal.Copy(tmp, 0, ptr, tmp.Length)

            Return ptr
        End Function

        ''' <summary>写入 <c>LONG[]</c>（64 位整数）。</summary>
        Public Function LongArray(values As Long()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim ptr As IntPtr = Alloc(values.Length * 8)

            Call Marshal.Copy(values, 0, ptr, values.Length)

            Return ptr
        End Function

        ''' <summary>把所有内存块指针写进一个 <c>void*[]</c>（用于 CreateOperatorInitializer 之类的接口）。</summary>
        Public Function PointerArray(values As IntPtr()) As IntPtr
            If values Is Nothing OrElse values.Length = 0 Then
                Return IntPtr.Zero
            End If

            Dim ptr As IntPtr = Alloc(values.Length * IntPtr.Size)

            Call Marshal.Copy(values, 0, ptr, values.Length)

            Return ptr
        End Function

        ''' <summary>构造并写入 <c>DML_OPERATOR_DESC</c>，返回其指针。</summary>
        ''' <param name="type">算子类型。</param>
        ''' <param name="desc">具体算子描述符的指针。</param>
        Public Function OperatorDesc(type As DML_OPERATOR_TYPE, desc As IntPtr) As IntPtr
            Return StructPtr(New DML_OPERATOR_DESC(type, desc))
        End Function

        ''' <summary>释放全部非托管内存。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            For Each ptr As IntPtr In _blocks
                Call Marshal.FreeHGlobal(ptr)
            Next

            _blocks.Clear()
        End Sub

    End Class

    ''' <summary>直接读取非托管结构体的辅助方法（诊断用）。</summary>
    Public Module DmlNativeHelper

        ''' <summary>从指针读取结构体。</summary>
        Public Function Read(Of T As Structure)(ptr As IntPtr) As T
            If ptr = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(ptr))
            End If

            Return Marshal.PtrToStructure(Of T)(ptr)
        End Function

    End Module

End Namespace
