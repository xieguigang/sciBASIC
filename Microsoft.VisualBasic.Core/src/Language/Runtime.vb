#Region "Microsoft.VisualBasic::6266b53f6242a8f568431e952e963008, Microsoft.VisualBasic.Core\src\Language\Runtime.vb"

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

    '   Total Lines: 173
    '    Code Lines: 120
    ' Comment Lines: 26
    '   Blank Lines: 27
    '     File Size: 6.05 KB


    '     Class ArgumentReference
    ' 
    '         Properties: Expression, Key, ValueType
    ' 
    '         Function: [As], GetUnderlyingType, ToString
    '         Operators: <>, =
    ' 
    '     Class TypeSchema
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Equals, ToString
    '         Operators: (+2 Overloads) And, (+2 Overloads) Or
    ' 
    '     Class Runtime
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.Default

Namespace Language

    ''' <summary>
    ''' ``[name => value]`` tuple
    ''' </summary>
    Public Class ArgumentReference : Inherits Value
        Implements INamedValue

        Private Property Key As String Implements IKeyedEntity(Of String).Key
            Get
                Return Name
            End Get
            Set(value As String)
                Name = value
            End Set
        End Property

        Public ReadOnly Property Expression(Optional null$ = "Nothing",
                                            Optional stringEscaping As Func(Of String, String) = Nothing,
                                            Optional isVar As Predicate(Of String) = Nothing) As String
            Get
                Dim val$

                Static [isNot] As New [Default](Of Predicate(Of String))(Function(var) False)

                If Value Is Nothing Then
                    val = null
                ElseIf Value.GetType Is GetType(String) Then
                    ' string can be a variable name
                    If (isVar Or [isNot])(Value) Then
                        val = Value
                    Else
                        val = $"""{(stringEscaping Or noEscaping)(Value)}"""
                    End If
                ElseIf Value.GetType Is GetType(Char) Then
                    val = $"""{Value}"""
                Else
                    val = Value
                End If

                Return $"{Name} = {val}"
            End Get
        End Property

        Public ReadOnly Property ValueType As Type
            Get
                If value Is Nothing Then
                    Return GetType(Void)
                Else
                    Return value.GetType
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetUnderlyingType() As Type
            Return ValueType
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [As](Of T)() As NamedValue(Of T)
            Return New NamedValue(Of T)(name, value)
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As Object = {Scripting.ToString(value, "null")}"
        End Function

        ''' <summary>
        ''' Argument variable value assign
        ''' </summary>
        ''' <param name="var">The argument name</param>
        ''' <param name="value">argument value</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(var As ArgumentReference, value As Object) As ArgumentReference
            var.Value = value
            Return var
        End Operator

        Public Overloads Shared Operator <>(var As ArgumentReference, value As Object) As ArgumentReference
            Throw New NotImplementedException
        End Operator

#If NET48_OR_GREATER Or NETCOREAPP Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(arg As ArgumentReference) As (name As String, value As Object)
            Return (arg.Name, arg.Value)
        End Operator

#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(name As String) As ArgumentReference
            Return New ArgumentReference With {.Name = name}
        End Operator
    End Class

    Public Class TypeSchema

        Public ReadOnly Property Type As Type

        Sub New(type As Type)
            Me.Type = type
        End Sub

        Public Overrides Function ToString() As String
            Return Type.FullName
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator And(info As TypeSchema, types As Type()) As Boolean
            Return types.All(Function(t) Equals(info.Type, base:=t))
        End Operator

        Private Overloads Shared Function Equals(info As Type, base As Type) As Boolean
            If info.IsInheritsFrom(base) Then
                Return True
            Else
                If base.IsInterface AndAlso info.ImplementInterface(base) Then
                    Return True
                Else
                    Return False
                End If
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator Or(info As TypeSchema, types As Type()) As Boolean
            Return types.Any(Function(t) Equals(info.Type, base:=t))
        End Operator
    End Class

    ''' <summary>
    ''' Runtime helper
    ''' 
    ''' ```vbnet
    ''' Imports VB = Microsoft.VisualBasic.Language.Runtime
    ''' 
    ''' With New VB
    '''     ' ...
    ''' End With
    ''' ```
    ''' </summary>
    Public Class Runtime

        ''' <summary>
        ''' Language syntax supports for argument list
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Argument(name$) As ArgumentReference
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New ArgumentReference With {
                    .name = name
                }
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "sciBASIC for VB.NET language runtime API"
        End Function
    End Class
End Namespace
