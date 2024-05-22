#Region "Microsoft.VisualBasic::c200c8eeff2906f21da9699abaf81ade, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\NativeHandle.vb"

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

    '   Total Lines: 113
    '    Code Lines: 58 (51.33%)
    ' Comment Lines: 37 (32.74%)
    '    - Xml Docs: 83.78%
    ' 
    '   Blank Lines: 18 (15.93%)
    '     File Size: 4.99 KB


    '     Class NativeHandle
    ' 
    '         Properties: Disposed, IsInvalid, ReferenceCount
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetHandle, IsValidHandle
    ' 
    '         Sub: AddRef, Dispose, DisposeImpl, Finalize, Release
    '              SetHandle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        Protected Sub New(pointer As IntPtr, Optional currentRefCount As Integer = 0)
            SetHandle(pointer, currentRefCount)
        End Sub

        ''' <summary> Specialised default constructor for use only by derived class. 
        '''           Defers setting the handle to the derived class</summary>
        Protected Sub New()
        End Sub

        Private Function IsValidHandle(pointer As IntPtr) As Boolean
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
        Protected Sub SetHandle(pointer As IntPtr, Optional currentRefCount As Integer = 0)
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

        Private Sub DisposeImpl(decrement As Boolean)
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
