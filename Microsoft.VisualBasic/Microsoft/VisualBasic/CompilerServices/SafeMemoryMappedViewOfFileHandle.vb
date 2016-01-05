Imports Microsoft.Win32.SafeHandles
Imports System
Imports System.Runtime.ConstrainedExecution
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices
    <SecurityCritical> _
    Friend NotInheritable Class SafeMemoryMappedViewOfFileHandle
        Inherits SafeHandleZeroOrMinusOneIsInvalid
        ' Methods
        Friend Sub New()
            MyBase.New(True)
        End Sub

        Friend Sub New(handle As IntPtr, ownsHandle As Boolean)
            MyBase.New(ownsHandle)
            MyBase.SetHandle(handle)
        End Sub

        <SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)> _
        Protected Overrides Function ReleaseHandle() As Boolean
            Dim flag As Boolean
            Try 
                If UnsafeNativeMethods.UnmapViewOfFile(MyBase.handle) Then
                    Return True
                End If
                flag = False
            Finally
                MyBase.handle = IntPtr.Zero
            End Try
            Return flag
        End Function

    End Class
End Namespace

