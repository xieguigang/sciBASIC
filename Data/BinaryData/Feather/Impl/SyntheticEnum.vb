#Region "Microsoft.VisualBasic::296007a8292600fbb1699639b1842be1, Data\BinaryData\Feather\Impl\SyntheticEnum.vb"

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

    '   Total Lines: 81
    '    Code Lines: 62 (76.54%)
    ' Comment Lines: 1 (1.23%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (22.22%)
    '     File Size: 3.18 KB


    '     Module SyntheticEnum
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateSyntheticEnum, IsSynthetic, Lookup
    '         Class Key
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: (+2 Overloads) Equals, GetHashCode
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Reflection.Emit

Namespace Impl
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
                Dim syntheticEnum As Type = Nothing
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
