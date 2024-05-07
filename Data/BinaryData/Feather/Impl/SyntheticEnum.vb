Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Reflection.Emit

Namespace FeatherDotNet.Impl
    Friend Module SyntheticEnum
        Friend Class Key
            Implements IEquatable(Of Key)
            Public Enums As Type()

            Public Sub New(enums As Type())
                Me.Enums = enums
            End Sub

            Public Overloads Function Equals(other As Key) As Boolean Implements IEquatable(Of Key).Equals
                Return other.Enums.SequenceEqual(Enums)
            End Function

            Public Overrides Function Equals(obj As Object) As Boolean
                If Not (TypeOf obj Is Key) Then Return False

                Return Equals(CType(obj, Key))
            End Function

            Public Overrides Function GetHashCode() As Integer
                Dim ret = 0
                For Each t In Enums
                    ret *= 17
                    ret += t.GetHashCode()
                Next

                Return ret
            End Function
        End Class

        Private ReadOnly Assembly As AssemblyBuilder
        Private ReadOnly [Module] As ModuleBuilder
        Private ReadOnly SyntheticEnumLookup As Dictionary(Of Key, Type)

        Sub New()
            Assembly = AssemblyBuilder.DefineDynamicAssembly(New AssemblyName("FeatherDotNet_SynthethicEnumLookup_DynamicAssembly"), AssemblyBuilderAccess.Run)
            [Module] = Assembly.DefineDynamicModule("DynamicModule")
            SyntheticEnumLookup = New Dictionary(Of Key, Type)()
        End Sub

        Public Function Lookup(enumTypes As IEnumerable(Of Type)) As Type
            Dim inOrder = enumTypes.OrderBy(Function(t) t.AssemblyQualifiedName).ThenBy(Function(t) t.FullName).ThenBy(Function(t) t.GUID).ToArray()
            Dim key = New Key(inOrder)

            ' assumed low contention
            SyncLock SyntheticEnumLookup
                Dim syntheticEnum As Type
                If SyntheticEnumLookup.TryGetValue(key, syntheticEnum) Then Return syntheticEnum
                syntheticEnum = CreateSyntheticEnum(inOrder)
                SyntheticEnumLookup(key) = syntheticEnum

                Return syntheticEnum
            End SyncLock
        End Function

        Public Function IsSynthetic(enumType As Type) As Boolean
            Return enumType.Assembly.Equals(Assembly)
        End Function

        Private Function CreateSyntheticEnum(realEnums As Type()) As Type
            Dim enumName = "SytheticEnumFor_" & String.Join("-", realEnums.[Select](Function(t) t.FullName))

            Dim builder = [Module].DefineEnum(enumName, TypeAttributes.Public, GetType(Long))

            Dim distinctNames = realEnums.SelectMany(Function(e) [Enum].GetNames(e)).Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(Function(__) __).ToArray()

            For i = 0 To distinctNames.Length - 1
                builder.DefineLiteral(distinctNames(i), CLng(i + 1))
            Next

            Return builder.CreateTypeInfo().AsType()
        End Function
    End Module
End Namespace
