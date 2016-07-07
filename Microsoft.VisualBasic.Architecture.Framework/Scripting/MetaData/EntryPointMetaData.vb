#Region "19c73b16fe8cd5b911323a69c4285636, ..\Microsoft.VisualBasic.Architecture.Framework\Scripting\MetaData\EntryPointMetaData.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Reflection

Namespace Scripting.MetaData

    <AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple:=False, Inherited:=True)>
    Public Class FunctionReturns : Inherits Attribute

        Public ReadOnly Property Description As String

        Sub New(Description As String)
            Me.Description = Description
        End Sub

        Public Overrides Function ToString() As String
            Return Description
        End Function

        Public Shared ReadOnly Property TypeRef As Type = GetType(FunctionReturns)

        Public Shared Function GetDescription(Method As System.Reflection.MethodInfo) As String
            Dim attrs As Object() = Method.ReturnParameter.GetCustomAttributes(attributeType:=FunctionReturns.TypeRef, inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return ""
            End If

            Dim value = DirectCast(attrs(Scan0), FunctionReturns).Description
            Return value
        End Function
    End Class

    ''' <summary>
    ''' 用于解决函数重载的函数数字签名的属性
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class OverloadsSignatureHandle : Inherits Attribute

        Dim _TypeIdBrief As String, _FullName As Type

        Public ReadOnly Property TypeIDBrief As String
            Get
                Return _TypeIdBrief
            End Get
        End Property

        Public ReadOnly Property FullName As Type
            Get
                Return _FullName
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="TypeIdBrief">Brief name for the target signature type <paramref name="FullName"></paramref></param>
        ''' <param name="FullName">Target signature type for function overloads.</param>
        ''' <remarks></remarks>
        Sub New(TypeIdBrief As String, FullName As Type)
            _TypeIdBrief = TypeIdBrief.ToLower
            _FullName = FullName
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}", _TypeIdBrief, _FullName.FullName)
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)>
    Public Class ImportsConstant : Inherits Attribute

        Dim _Name As String
        Public ReadOnly Property Name As String
            Get
                Return _Name
            End Get
        End Property

        Sub New(<Parameter("imports.constant", "")> Optional Name As String = "")
            _Name = Name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Private Shared ReadOnly _TypeInfo As System.Type = GetType(ImportsConstant)

        Public Shared ReadOnly Property TypeInfo As System.Type
            Get
                Return _TypeInfo
            End Get
        End Property
    End Class
End Namespace
