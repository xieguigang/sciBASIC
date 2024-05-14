#Region "Microsoft.VisualBasic::e2129d0a26c311ae11d7a793fd4e374b, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\IProperty.vb"

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

    '   Total Lines: 80
    '    Code Lines: 35
    ' Comment Lines: 29
    '   Blank Lines: 16
    '     File Size: 2.80 KB


    '     Interface IProperty
    ' 
    '         Function: GetValue
    ' 
    '         Sub: SetValue
    ' 
    '     Interface IDynamicsObject
    ' 
    '         Function: GetItemValue, GetNames, HasName
    ' 
    '         Sub: Add, SetValue
    ' 
    '     Interface IDynamicMeta
    ' 
    '         Properties: Properties
    ' 
    '     Class DynamicMetadataAttribute
    ' 
    '         Function: (+2 Overloads) GetMetadata, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.DataSourceModel

    Public Interface IProperty : Inherits IReadOnlyId

        ''' <summary>
        ''' Gets property value from <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        Function GetValue(target As Object) As Object

        ''' <summary>
        ''' Set <paramref name="value"/> to the property of <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="value"></param>
        Sub SetValue(target As Object, value As Object)
    End Interface

    ''' <summary>
    ''' Apply for R# object cast .NET CLR object to list
    ''' </summary>
    Public Interface IDynamicsObject

        Sub Add(propertyName$, value As Object)
        Sub SetValue(propertyName$, value As Object)

        Function GetNames() As IEnumerable(Of String)
        Function GetItemValue(propertyName As String) As Object
        Function HasName(name As String) As Boolean

    End Interface

    ''' <summary>
    ''' Abstracts for the dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IDynamicMeta(Of T)

        ''' <summary>
        ''' Properties
        ''' </summary>
        ''' <returns></returns>
        Property Properties As Dictionary(Of String, T)
    End Interface

    ''' <summary>
    ''' just used for tagged on the <see cref="DynamicPropertyBase(Of T).Properties"/> property
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class DynamicMetadataAttribute : Inherits Attribute

        Public Overrides Function ToString() As String
            Return "This property is a metadata pack"
        End Function

        Public Shared Function GetMetadata(properties As IEnumerable(Of PropertyInfo)) As PropertyInfo
            Dim find As PropertyInfo = properties _
               .Where(Function(t) t.GetCustomAttribute(Of DynamicMetadataAttribute) IsNot Nothing) _
               .FirstOrDefault

            Return find
        End Function

        ''' <summary>
        ''' get the target <see cref="DynamicPropertyBase(Of T).Properties"/> its <see cref="PropertyInfo"/>
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function GetMetadata(type As Type) As PropertyInfo
            Dim properties = type.GetProperties
            Dim find As PropertyInfo = GetMetadata(properties)

            Return find
        End Function
    End Class
End Namespace
