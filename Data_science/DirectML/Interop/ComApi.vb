' ------------------------------------------------------------------------
' 手写 COM vtable 互操作的基础设施
'
' 说明：
'   1. 这里只使用 BCL 的 System.Runtime.InteropServices，没有任何 NuGet 依赖；
'   2. COM 接口的方法表槽位直接用 Marshal.GetDelegateForFunctionPointer 绑定，
'      槽位号来自 Windows SDK 头文件（d3d12.h / dxgi.h / DirectML.h）里的 Vtbl 结构体；
'   3. 委托必须由**持有该接口的对象**缓存（字段），否则委托可能在调用前被 GC 回收，
'      造成 AccessViolation。本模块的 ComObjectBase 已经把 IUnknown 三个槽位缓存好，
'      派生类只需要用 GetSlotDelegate 把各自的槽位缓存到字段里。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>
    ''' COM 互操作的基础设施：vtable 槽位绑定、IUnknown 三槽位与对象包装基类。
    ''' </summary>
    Public Module ComApi

        ''' <summary>S_OK</summary>
        Public Const S_OK As Integer = 0
        ''' <summary>S_FALSE</summary>
        Public Const S_FALSE As Integer = 1
        ''' <summary>E_NOINTERFACE（0x80004002）</summary>
        Public Const E_NOINTERFACE As Integer = -2147467262
        ''' <summary>E_POINTER（0x80004003）</summary>
        Public Const E_POINTER As Integer = -2147467261
        ''' <summary>E_FAIL（0x80004005）</summary>
        Public Const E_FAIL As Integer = -2147467259

        ''' <summary>IUnknown::QueryInterface</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function QueryInterfaceFn(instance As IntPtr, ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        ''' <summary>IUnknown::AddRef</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function AddRefFn(instance As IntPtr) As UInteger

        ''' <summary>IUnknown::Release</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function ReleaseFn(instance As IntPtr) As UInteger

        ''' <summary>
        ''' 读取指定槽位的函数指针：vtable 本身是函数指针数组，槽位 0 为 QueryInterface。
        ''' </summary>
        ''' <param name="instance">COM 接口指针。</param>
        ''' <param name="slot">槽位号（从 0 开始）。</param>
        ''' <returns>函数指针。</returns>
        Public Function GetVtableSlot(instance As IntPtr, slot As Integer) As IntPtr
            If instance = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(instance))
            End If

            Dim vtable As IntPtr = Marshal.ReadIntPtr(instance)

            Return Marshal.ReadIntPtr(vtable, slot * IntPtr.Size)
        End Function

        ''' <summary>
        ''' 把指定槽位绑定成强类型委托。
        ''' </summary>
        ''' <typeparam name="TDelegate">与原生签名一致的委托类型。</typeparam>
        ''' <param name="instance">COM 接口指针。</param>
        ''' <param name="slot">槽位号（从 0 开始）。</param>
        ''' <returns>委托实例；调用方必须缓存它。</returns>
        Public Function GetDelegate(Of TDelegate As Class)(instance As IntPtr, slot As Integer) As TDelegate
            Dim fn As IntPtr = GetVtableSlot(instance, slot)
            Dim created As System.Delegate = Marshal.GetDelegateForFunctionPointer(fn, GetType(TDelegate))

            Return DirectCast(CObj(created), TDelegate)
        End Function

        ''' <summary>
        ''' 查询接口并返回指定 IID 的指针；不支持时返回 <see cref="IntPtr.Zero"/>。
        ''' </summary>
        ''' <param name="instance">COM 接口指针。</param>
        ''' <param name="iid">目标接口 IID。</param>
        ''' <returns>接口指针或零。</returns>
        Public Function TryQueryInterface(instance As IntPtr, ByRef iid As Guid) As IntPtr
            Dim qi As QueryInterfaceFn = GetDelegate(Of QueryInterfaceFn)(instance, 0)
            Dim ppv As IntPtr = IntPtr.Zero
            Dim hr As Integer = qi(instance, iid, ppv)

            If hr = S_OK Then
                Return ppv
            End If

            Return IntPtr.Zero
        End Function

    End Module

    ''' <summary>
    ''' COM 对象的托管包装基类：统一管理原生指针的引用计数与 vtable 槽位委托缓存。
    ''' </summary>
    ''' <remarks>
    ''' 派生类在构造函数里把需要的槽位用 <see cref="GetSlotDelegate"/> 缓存到字段，
    ''' 在 <see cref="Release"/> 里释放子对象，并保证 <see cref="Dispose"/> 幂等。
    ''' </remarks>
    Public MustInherit Class ComObjectBase
        Implements IDisposable

        Private ReadOnly _ptr As IntPtr
        Private ReadOnly _queryInterface As QueryInterfaceFn
        Private ReadOnly _addRef As AddRefFn
        Private ReadOnly _release As ReleaseFn
        Private _disposed As Boolean

        ''' <summary>包装一个已经持有引用的原生接口指针（构造成功后所有权转移给本对象）。</summary>
        ''' <param name="ptr">接口指针，不能为零。</param>
        Protected Sub New(ptr As IntPtr)
            If ptr = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(ptr), "COM 接口指针不能为空")
            End If

            Me._ptr = ptr
            Me._queryInterface = ComApi.GetDelegate(Of QueryInterfaceFn)(ptr, 0)
            Me._addRef = ComApi.GetDelegate(Of AddRefFn)(ptr, 1)
            Me._release = ComApi.GetDelegate(Of ReleaseFn)(ptr, 2)
        End Sub

        ''' <summary>原生接口指针。</summary>
        Public ReadOnly Property NativePointer As IntPtr
            Get
                Return _ptr
            End Get
        End Property

        ''' <summary>对象是否已经释放。</summary>
        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return _disposed
            End Get
        End Property

        ''' <summary>增加引用计数。</summary>
        Public Function AddRef() As UInteger
            ThrowIfDisposed()

            Return _addRef(_ptr)
        End Function

        ''' <summary>
        ''' 查询指定接口并返回原始指针（调用方负责 Release）。
        ''' </summary>
        ''' <param name="iid">目标接口 IID。</param>
        ''' <returns>接口指针；不支持时返回 <see cref="IntPtr.Zero"/>。</returns>
        Public Function QueryInterface(ByRef iid As Guid) As IntPtr
            ThrowIfDisposed()

            Dim ppv As IntPtr = IntPtr.Zero

            If _queryInterface(_ptr, iid, ppv) = ComApi.S_OK Then
                Return ppv
            End If

            Return IntPtr.Zero
        End Function

        ''' <summary>绑定本对象某个 vtable 槽位的委托（调用方必须把结果保存到字段）。</summary>
        Protected Function GetSlotDelegate(Of TDelegate As Class)(slot As Integer) As TDelegate
            Return ComApi.GetDelegate(Of TDelegate)(_ptr, slot)
        End Function

        ''' <summary>释放原生对象。</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            Try
                Release()
            Finally
                GC.SuppressFinalize(Me)
            End Try
        End Sub

        ''' <summary>派生类在此释放子对象与自身引用。</summary>
        Protected Overridable Sub Release()
        End Sub

        ''' <summary>释放本对象持有的原生引用。</summary>
        Protected Sub ReleaseNative()
            If _ptr <> IntPtr.Zero Then
                Call _release(_ptr)
            End If
        End Sub

        ''' <summary>对象已释放时抛出异常。</summary>
        Protected Sub ThrowIfDisposed()
            If _disposed Then
                Throw New ObjectDisposedException(Me.GetType().Name)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Me.GetType().Name}@0x{_ptr.ToInt64():X}"
        End Function

    End Class

End Namespace
