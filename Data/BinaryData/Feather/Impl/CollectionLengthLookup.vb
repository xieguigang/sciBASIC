#Region "Microsoft.VisualBasic::ee1308b8cb6d051aceeace4beaccfb78, Data\BinaryData\Feather\Impl\CollectionLengthLookup.vb"

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

    '   Total Lines: 40
    '    Code Lines: 29 (72.50%)
    ' Comment Lines: 1 (2.50%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (25.00%)
    '     File Size: 1.75 KB


    '     Module CollectionLengthLookup
    ' 
    '         Function: CreateLengthGetter, GetLength
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices

Namespace Impl

    Friend Module CollectionLengthLookup

        ReadOnly LengthGetterLookup As New Dictionary(Of Type, Func(Of Object, Integer))()

        <Extension>
        Public Function GetLength(elementType As Type, collection As Object) As Integer
            Dim getter As Func(Of Object, Integer) = Nothing

            ' assuming this is a low contention lock
            SyncLock LengthGetterLookup
                If Not LengthGetterLookup.TryGetValue(elementType, getter) Then
                    getter = CreateLengthGetter(elementType)
                    LengthGetterLookup(elementType) = getter
                End If
            End SyncLock

            Return getter(collection)
        End Function

        Private Function CreateLengthGetter(elementType As Type) As Func(Of Object, Integer)
            Dim typedCollection = GetType(ICollection(Of)).MakeGenericType(elementType)
            Dim count = typedCollection.GetProperty("Count").GetMethod

            Dim dyn = New DynamicMethod($"{NameOf(CollectionLengthLookup)}_{NameOf(elementType.Name)}", GetType(Integer), {GetType(Object)}, restrictedSkipVisibility:=True)
            Dim il = dyn.GetILGenerator()

            il.Emit(OpCodes.Ldarg_0)                        ' object
            il.Emit(OpCodes.Castclass, typedCollection)     ' ICollection<elementType>
            il.Emit(OpCodes.Callvirt, count)                ' int
            il.Emit(OpCodes.Ret)                            ' --empty--

            Return CType(dyn.CreateDelegate(GetType(Func(Of Object, Integer))), Func(Of Object, Integer))
        End Function
    End Module
End Namespace
