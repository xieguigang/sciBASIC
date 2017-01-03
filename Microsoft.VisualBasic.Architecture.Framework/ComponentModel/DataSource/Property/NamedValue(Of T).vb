#Region "Microsoft.VisualBasic::d0c15a0fa7a766dacff4557c873ce11a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\NamedValue(Of T).vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The value object have a name string.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure NamedValue(Of T) : Implements INamedValue

        ''' <summary>
        ''' Identifier tag data. you can using this property value as a dictionary key.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property Name As String Implements INamedValue.Key

        ''' <summary>
        ''' Object value
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property Value As T

        <XmlAttribute>
        Public Property Description As String

        ''' <summary>
        ''' Does this object have value?
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore> Public ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(Name) AndAlso Value Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Creates a object bind with a specific <see cref="Name"/>.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        Sub New(name$, value As T, Optional describ As String = Nothing)
            Me.Name = name
            Me.Value = value
            Me.Description = describ
        End Sub

        ''' <summary>
        ''' View object.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"{Name} --> {Value.GetJson}"
        End Function

        Public Function FixValue(h As Func(Of T, T)) As NamedValue(Of T)
            Return New NamedValue(Of T)(Name, h(Value))
        End Function
    End Structure
End Namespace
