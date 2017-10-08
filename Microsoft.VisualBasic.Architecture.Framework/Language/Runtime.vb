#Region "Microsoft.VisualBasic::c8ff0e7546c4b171d4a7c6e4939d308a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Runtime.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace Language

    Public Class ArgumentReference : Implements INamedValue

        Public name$, value

        Private Property Key As String Implements IKeyedEntity(Of String).Key
            Get
                Return name
            End Get
            Set(value As String)
                name = value
            End Set
        End Property

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
        Public Shared Operator =(var As ArgumentReference, value As Object) As ArgumentReference
            var.value = value
            Return var
        End Operator

        Public Shared Operator <>(var As ArgumentReference, value As Object) As ArgumentReference
            Throw New NotImplementedException
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
