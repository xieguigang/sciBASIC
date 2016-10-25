#Region "Microsoft.VisualBasic::a5d0a7bb46b881df62ccce9caea2b018, ..\visualbasic_App\Data_science\Mathematical\ODE\var.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.diffEq

''' <summary>
''' Y variable in the ODE
''' </summary>
Public Class var : Inherits float
    Implements Ivar

    Public Property Index As Integer
    Public Property Name As String Implements sIdEnumerable.Identifier
    Public Overrides Property value As Double Implements Ivar.value

    Public Shared ReadOnly type As Type = GetType(var)

    Sub New()
    End Sub

    Sub New(name As String)
        Me.Name = name
    End Sub

    ''' <summary>
    ''' Value copy
    ''' </summary>
    ''' <param name="var"></param>
    Sub New(var As var)
        With var
            Index = .Index
            Name = .Name
            value = .value
        End With
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{Index}] {Name} As System.Double = {value}"
    End Function

    Public Overloads Shared Operator =(var As var, x As Double) As var
        var.value = x
        Return var
    End Operator

    Public Overloads Shared Narrowing Operator CType(x As var) As Integer
        Return x.Index
    End Operator

    Public Overloads Shared Operator <>(var As var, x As Double) As var
        Throw New NotSupportedException
    End Operator
End Class

Public Interface Ivar : Inherits sIdEnumerable

    Property value As Double
End Interface
