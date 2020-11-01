Namespace ApplicationServices.DynamicInterop

    ''' <summary> A stub implementation for the INativeHandle interface </summary>
    Public MustInherit Class NativeHandle
        Implements INativeHandle

        ''' <summary> Specialised constructor for use only by derived class.</summary>
        '''
        ''' <param name="pointer">         The handle, value of the pointer to the native object</param>
        ''' <param name="currentRefCount"> (Optional) Number of pre-existing references for the native object</param>
        ''' <remarks>If a native object was created prior to its use by .NET, its lifetime may need to extend its use 
        '''          from .NET. In practice the scenario is unlikely</remarks>
        Protected Sub New(ByVal pointer As IntPtr, ByVal Optional currentRefCount As Integer = 0)
            SetHandle(pointer, currentRefCount)
        End Sub

        ''' <summary> Specialised default constructor for use only by derived class. 
        '''           Defers setting the handle to the derived class</summary>
        Protected Sub New()
        End Sub

        Private Function IsValidHandle(ByVal pointer As IntPtr) As Boolean
            Return pointer <> IntPtr.Zero
        End Function

        ''' <summary> Sets a handle.</summary>
        '''
        ''' <exception cref="ArgumentException"> Thrown when a pointer is a Zero pointer
        '''                                      .</exception>
        '''
        ''' <param name="pointer">         The handle, value of the pointer to the native object</param>
        ''' <param name="currentRefCount"> (Optional) Number of pre-existing references for the native object</param>
        ''' <remarks>If a native object was created prior to its use by .NET, its lifetime may need to extend its use 
        '''          from .NET. In practice the scenario is unlikely</remarks>
        Protected Sub SetHandle(ByVal pointer As IntPtr, ByVal Optional currentRefCount As Integer = 0)
            If Not IsValidHandle(pointer) Then Throw New ArgumentException(String.Format("pointer '{0}' is not valid", pointer.ToString()))
            handle = pointer
            ReferenceCount = currentRefCount + 1
        End Sub

        Private finalizing As Boolean = False

        ''' <summary> Finaliser. Triggers the disposal of this object if not manually done.</summary>
        Protected Overrides Sub Finalize()
            finalizing = True
            Release()
        End Sub

        ''' <summary> Releases the native resource for this handle.</summary>
        '''
        ''' <returns> True if it succeeds, false if it fails.</returns>
        Protected MustOverride Function ReleaseHandle() As Boolean

        ''' <summary> 
        ''' Gets the number of references to the native resource for this handle.
        ''' </summary>
        ''' <value> The number of references.</value>
        Public Property ReferenceCount As Integer

        ''' <summary> Gets a value indicating whether this handle has been disposed of already</summary>
        '''
        ''' <value> True if disposed, false if not.</value>
        Public ReadOnly Property Disposed As Boolean
            Get
                Return IsInvalid
            End Get
        End Property

        ''' <summary> The handle to the native resource.</summary>
        Protected handle As IntPtr

        ''' <summary> Gets a value indicating whether this handle is invalid.</summary>
        Public ReadOnly Property IsInvalid As Boolean
            Get
                Return handle = IntPtr.Zero
            End Get
        End Property

        ''' <summary> If the reference counts allows it, release the resource refered to by this handle.</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            DisposeImpl(True)
        End Sub

        Private Sub DisposeImpl(ByVal decrement As Boolean)
            If Disposed Then Return
            If decrement Then ReferenceCount -= 1

            If ReferenceCount <= 0 Then
                If ReleaseHandle() Then
                    handle = IntPtr.Zero
                    If Not finalizing Then GC.SuppressFinalize(Me)
                End If
            End If
        End Sub

        ''' <summary> Returns the value of the handle.</summary>
        '''
        ''' <returns> The handle.</returns>
        Public Function GetHandle() As IntPtr Implements INativeHandle.GetHandle
            Return handle
        End Function

        ''' <summary> Manually increments the reference counter.</summary>
        Public Sub AddRef() Implements INativeHandle.AddRef
            ReferenceCount += 1
        End Sub

        ''' <summary> Manually decrements the reference counter. Triggers disposal if count reaches is zero.</summary>
        Public Sub Release() Implements INativeHandle.Release
            DisposeImpl(True)
        End Sub
    End Class
End Namespace
