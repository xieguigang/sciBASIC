Namespace ApplicationServices.DynamicInterop

    ''' <summary> Interface for native handle.</summary>
    ''' <remarks>This is similar in intent to the BCL SafeHandle, but with release 
    '''          behaviors that are more desirable in particular circumstances.
    '''          </remarks>
    Public Interface INativeHandle : Inherits IDisposable

        ''' <summary> Returns the value of the handle.</summary>
        '''
        ''' <returns> The handle.</returns>
        Function GetHandle() As IntPtr

        ''' <summary>Manually increments the reference counter</summary>
        Sub AddRef()

        ''' <summary>Manually decrements the reference counter. Triggers disposal if count reaches is zero.</summary>
        Sub Release()
    End Interface
End Namespace