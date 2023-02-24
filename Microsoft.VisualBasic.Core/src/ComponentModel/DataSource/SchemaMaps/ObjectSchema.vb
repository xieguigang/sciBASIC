#Region "Microsoft.VisualBasic::e60b8ad39a90c810c090f46f60e62eea, sciBASIC#\mime\application%json\Serializer\ObjectSchema.vb"

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

    '   Total Lines: 126
    '    Code Lines: 92
    ' Comment Lines: 18
    '   Blank Lines: 16
    '     File Size: 4.48 KB


    ' Class ObjectSchema
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CreateSchema, FindInterfaceImpementations, GetSchema, Score, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' object schema provider for json/xml
    ''' </summary>
    Public Class SoapGraph

        Public ReadOnly addMethod As MethodInfo
        Public ReadOnly isTable As Boolean
        Public ReadOnly writers As IReadOnlyDictionary(Of String, PropertyInfo)
        ''' <summary>
        ''' Value type of the dictionary
        ''' </summary>
        Public ReadOnly valueType As Type
        Public ReadOnly keyType As Type
        Public ReadOnly raw As Type

        Public ReadOnly knownTypes As Type()

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="addMethod"></param>
        ''' <param name="isTable"></param>
        ''' <param name="writers"></param>
        ''' <param name="valueType"></param>
        Private Sub New(addMethod As MethodInfo,
                        isTable As Boolean,
                        writers As IReadOnlyDictionary(Of String, PropertyInfo),
                        keyType As Type,
                        valueType As Type,
                        raw As Type,
                        knownTypes As Type())

            Me.addMethod = addMethod
            Me.isTable = isTable
            Me.writers = writers
            Me.valueType = valueType
            Me.keyType = keyType
            Me.raw = raw
            Me.knownTypes = knownTypes
        End Sub

        ''' <summary>
        ''' get (or cache a new schema graph object if not exists) a schema graph object
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function GetSchema(type As Type) As SoapGraph
            Static cache As New Dictionary(Of Type, SoapGraph)
            Return cache.ComputeIfAbsent(key:=type, lazyValue:=AddressOf CreateSchema)
        End Function

        Private Shared Function CreateSchema(schema As Type) As SoapGraph
            Dim isTable As Boolean = schema.IsInheritsFrom(GetType(DictionaryBase)) OrElse schema.ImplementInterface(GetType(IDictionary))
            Dim writers = schema.Schema(PropertyAccess.NotSure, PublicProperty, nonIndex:=True)
            Dim addMethod As MethodInfo = schema.GetMethods _
                .Where(Function(m)
                           Dim params = m.GetParameters

                           Return Not m.IsStatic AndAlso
                               Not params.IsNullOrEmpty AndAlso
                               params.Length = 2 AndAlso
                               m.Name = "Add"
                       End Function) _
                .FirstOrDefault
            Dim keyType As Type = Nothing
            Dim valueType As Type = Nothing

            If isTable Then
                With schema.GetGenericArguments
                    If .Length = 1 Then
                        keyType = GetType(String)
                        valueType = .GetValue(Scan0)
                    Else
                        keyType = .GetValue(Scan0)
                        valueType = .GetValue(1)
                    End If
                End With
            End If

            ' for avoid a infinity loop that caused by the circlar reference
            ' we just get the types at here
            ' create object schema graph object at required
            Dim knownTypes As Type() = schema _
                .GetCustomAttributes(Of KnownTypeAttribute) _
                .SafeQuery _
                .Select(Function(a) a.Type) _
                .ToArray

            Return New SoapGraph(
                raw:=schema,
                addMethod:=addMethod,
                writers:=writers,
                isTable:=isTable,
                valueType:=valueType,
                keyType:=keyType,
                knownTypes:=knownTypes
            )
        End Function

        Public Iterator Function FindInterfaceImpementations(type As Type) As IEnumerable(Of SoapGraph)
            For Each known As Type In knownTypes
                If known.ImplementInterface(type) Then
                    Yield SoapGraph.GetSchema(known)
                End If
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"{raw.Namespace}::{raw.Name}"
        End Function

    End Class
End Namespace