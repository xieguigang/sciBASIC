#Region "Microsoft.VisualBasic::150386fe3396050394bd3493fa83f9a4, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\MarshalExtra.vb"

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

    '   Total Lines: 87
    '    Code Lines: 43 (49.43%)
    ' Comment Lines: 33 (37.93%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 11 (12.64%)
    '     File Size: 4.99 KB


    '     Class MarshalExtra
    ' 
    '         Function: AllocHGlobal, ArrayOfStructureToPtr, PtrToStructure, StructureToPtr
    ' 
    '         Sub: FreeNativeArrayOfStruct, FreeNativeStruct
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices

Namespace ApplicationServices.DynamicInterop

    ''' <summary> 
    ''' Extra methods on top of System.Runtime.InteropServices.Marshal for allocating unmanaged memory, copying unmanaged
    ''' memory blocks, and converting managed to unmanaged types</summary>
    Friend Class MarshalExtra
        ''' <summary> Allocates memory from the unmanaged memory of the process for a given type</summary>
        '''
        ''' <typeparam name="T"> A type that has an equivalent in the unmanaged world e.g. int or a struct Point </typeparam>
        '''
        ''' <returns> A pointer to the newly allocated memory. This memory must be released using the FreeHGlobal(IntPtr) method, or related.</returns>
        Public Shared Function AllocHGlobal(Of T)() As IntPtr
            Dim iSize = Marshal.SizeOf(GetType(T))
            Return Marshal.AllocHGlobal(iSize)
        End Function

        ''' <summary> Marshals data from an unmanaged block of memory to a newly allocated managed object of the type specified by a generic type parameter. 
        '''           Note it is almost superseded in .NET Framework 4.5.1 and later versions; consider your needs</summary>
        '''
        ''' <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        '''                                      illegal values.</exception>
        '''
        ''' <typeparam name="T"> The type of the object to which the data is to be copied. This must be a structure.</typeparam>
        ''' <param name="ptr">   A pointer to an unmanaged block of memory.</param>
        ''' <param name="cleanup"> (Optional) If true, free the native memory block pointed to by ptr. This feature is handy in generated marshalling code.</param>
        '''
        ''' <returns> A managed object that contains the data that the ptr parameter points to</returns>
        Public Shared Function PtrToStructure(Of T As Structure)(ptr As IntPtr, Optional cleanup As Boolean = False) As T
            If ptr = IntPtr.Zero Then Throw New ArgumentException("pointer must not be IntPtr.Zero")
            Dim result As T = Marshal.PtrToStructure(ptr, GetType(T))
            If cleanup Then Marshal.FreeHGlobal(ptr)
            Return result
        End Function

        ''' <summary>Marshals data from a managed object of a specified type to an unmanaged block of memory.
        '''           Note it is almost superseded in .NET Framework 4.5.1 and later versions; consider your needs</summary>
        '''
        ''' <typeparam name="T"> The type of the managed object.</typeparam>
        ''' <param name="structure"> A managed object that holds the data to be marshaled. The object must be a structure.</param>
        '''
        ''' <returns> A pointer to a newly allocated unmanaged block of memory.</returns>
        Public Shared Function StructureToPtr(Of T As Structure)([structure] As T) As IntPtr
            Dim ptr = IntPtr.Zero
            Dim localStruct = [structure]
            Dim iSize = Marshal.SizeOf(GetType(T))
            ptr = Marshal.AllocHGlobal(iSize)
            Marshal.StructureToPtr(localStruct, ptr, False)
            Return ptr
        End Function

        ''' <summary> Frees all substructures of a specified type that the specified unmanaged memory block points to.</summary>
        '''
        ''' <typeparam name="T"> The type of the managed object.</typeparam>
        ''' <param name="ptr">           A pointer to an unmanaged block of memory.</param>
        ''' <param name="managedObject"> [in,out] The managed object.</param>
        ''' <param name="copy">          (Optional) True to copy.</param>
        Public Shared Sub FreeNativeStruct(Of T As Structure)(ptr As IntPtr, ByRef managedObject As T, Optional copy As Boolean = False)
            If ptr = IntPtr.Zero Then Return '?

            If copy Then
                managedObject = CType(Marshal.PtrToStructure(ptr, GetType(T)), T)
                ' Marshal.PtrToStructure<T>(ptr, managedObject);
            End If

            Marshal.FreeHGlobal(ptr)
        End Sub

        Public Shared Function ArrayOfStructureToPtr(Of T As Structure)(managedObjects As T()) As IntPtr
            Dim structSize As Integer = Marshal.SizeOf(GetType(T))
            Dim result = Marshal.AllocHGlobal(managedObjects.Length * structSize)

            For i = 0 To managedObjects.Length - 1
                Dim offset = i * structSize
                Marshal.StructureToPtr(managedObjects(i), IntPtr.Add(result, offset), False)
            Next

            Return result
        End Function

        Public Shared Sub FreeNativeArrayOfStruct(Of T As Structure)(ptr As IntPtr, ByRef managedObjects As T(), Optional copy As Boolean = False)
            If ptr = IntPtr.Zero Then Return
            Marshal.FreeHGlobal(ptr)
        End Sub
    End Class
End Namespace
