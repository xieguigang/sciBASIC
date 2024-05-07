Imports System.Reflection.Emit

Namespace FeatherDotNet.Impl
    Friend Module CollectionLengthLookup
        Private ReadOnly LengthGetterLookup As Dictionary(Of Type, Func(Of Object, Integer)) = New Dictionary(Of Type, Func(Of Object, Integer))()

        Public Function GetLength(elementType As Type, collection As Object) As Integer
            Dim getter As Func(Of Object, Integer)

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
