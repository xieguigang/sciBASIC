#Region "Microsoft.VisualBasic::b7d2ad13f6d392d4ad7da408afcd724b, Microsoft.VisualBasic.Core\src\My\JavaScript\JavaScriptObject\JavaScriptValue.vb"

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

'   Total Lines: 53
'    Code Lines: 41 (77.36%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 12 (22.64%)
'     File Size: 1.47 KB


'     Class JavaScriptValue
' 
'         Properties: Accessor, IsConstant, Literal
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: GetValue, ToString
' 
'         Sub: SetValue
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports any = Microsoft.VisualBasic.Scripting

Namespace My.JavaScript

    Public Class JavaScriptValue

        ''' <summary>
        ''' A symbol reference to the parent object
        ''' </summary>
        ''' <returns></returns>
        Public Property Accessor As BindProperty(Of DataFrameColumnAttribute)

        ''' <summary>
        ''' The constant literal value
        ''' </summary>
        ''' <returns></returns>
        Public Property Literal As Object

        Dim target As JavaScriptObject

        ''' <summary>
        ''' is scalar constant literal value?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsConstant As Boolean
            Get
                If Accessor Is Nothing Then
                    Return True
                End If

                Return Accessor.member Is Nothing
            End Get
        End Property

        Sub New(bind As BindProperty(Of DataFrameColumnAttribute), target As JavaScriptObject)
            Me.Accessor = bind
            Me.target = target
        End Sub

        Sub New()
        End Sub

        Public Function GetValue() As Object
            If IsConstant Then
                Return Literal
            Else
                Return Accessor.GetValue(target)
            End If
        End Function

        Public Sub SetValue(value As Object)
            If IsConstant Then
                Literal = value
            Else
                Accessor.SetValue(target, value)
            End If
        End Sub

        ''' <summary>
        ''' Check of the literal value is primitive value?
        ''' </summary>
        ''' <returns></returns>
        Public Function CheckLiteral() As Boolean
            If IsConstant Then
                Return Literal Is Nothing OrElse DataFramework.IsPrimitive(Literal.GetType, autoCastEnum:=True)
            End If

            Return False
        End Function

        Public Overrides Function ToString() As String
            Return any.ToString(GetValue)
        End Function
    End Class

End Namespace
