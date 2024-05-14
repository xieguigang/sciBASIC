#Region "Microsoft.VisualBasic::a8704cdc6b29ea0fa670bc017c8064fc, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\PropertyValue.vb"

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

    '   Total Lines: 109
    '    Code Lines: 66
    ' Comment Lines: 25
    '   Blank Lines: 18
    '     File Size: 3.95 KB


    '     Interface IPropertyValue
    ' 
    '         Properties: [Property]
    ' 
    '     Class Bind
    ' 
    '         Properties: IsPrimitive, memberName, Type
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.Abstract

Namespace ComponentModel.DataSourceModel.SchemaMaps

    Public Interface IPropertyValue : Inherits INamedValue, Value(Of String).IValueOf
        Property [Property] As String
    End Interface

    Public Class Bind

        Protected Friend ReadOnly handleSetValue As Action(Of Object, Object)
        Protected Friend ReadOnly handleGetValue As Func(Of Object, Object)

        ''' <summary>
        ''' The property/field object that bind with its custom attribute <see cref="field"/> of type 
        ''' </summary>
        Public ReadOnly member As MemberInfo
        Protected caster As LoadObject

        ''' <summary>
        ''' Gets the type of this <see cref="member"/>.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As Type

        ''' <summary>
        ''' get member name directly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property memberName As String
            Get
                Return member.Name
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the <see cref="System.Type"/> is one of the primitive types.
        ''' </summary>
        ''' <returns>
        ''' true if the <see cref="System.Type"/> is one of the primitive types; otherwise, false.
        ''' </returns>
        Public ReadOnly Property IsPrimitive As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Scripting.IsPrimitive(Type)
            End Get
        End Property

        Private Sub New(type As Type, member As MemberInfo)
            If InputHandler.CasterString.ContainsKey(type) Then
                caster = InputHandler.CasterString(type)
            End If

            Me.Type = type
            Me.member = member
        End Sub

        Sub New(prop As PropertyInfo)
            Call Me.New(prop.PropertyType, prop)

            ' Compile the property get/set as the delegate
            With prop
                handleSetValue = AddressOf prop.SetValue  ' .DeclaringType.PropertySet(.Name)
                handleGetValue = AddressOf prop.GetValue  ' .DeclaringType.PropertyGet(.Name)
            End With
        End Sub

        Sub New(field As FieldInfo)
            Call Me.New(field.FieldType, field)

            With field
                handleSetValue = AddressOf field.SetValue  ' .DeclaringType.FieldSet(.Name)
                handleGetValue = AddressOf field.GetValue  ' .DeclaringType.FieldGet(.Name)
            End With
        End Sub

        Sub New(method As MethodInfo)
            Call Me.New(method.ReturnType, method)

            If method.IsNonParametric(optionalAsNone:=True) Then
                Throw New InvalidConstraintException("Only allows parameterless method or all of the parameter should be optional!")
            End If

            With method
                handleSetValue = Sub(a, b)
                                     Throw New ReadOnlyException
                                 End Sub
                handleGetValue = Function(obj) method.Invoke(obj, {})
            End With
        End Sub

        ''' <summary>
        ''' parse string as the target type value by 
        ''' using the specific caster method.
        ''' </summary>
        ''' <param name="val"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Parse(val As String) As Object
            Return caster(val)
        End Function
    End Class
End Namespace
