#Region "Microsoft.VisualBasic::e82582233ab9370ed4a916d504f35ba8, Data\BinaryData\BinaryData\XDR\Attributes\CaseAttribute.vb"

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

    '     Class CaseAttribute
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, Inherited:=True, AllowMultiple:=True)>
    Public Class CaseAttribute
        Inherits Attribute

        Public ReadOnly Value As Object

        Public Sub New(val As Object)
            Dim vT As Type = val.GetType()

            If vT Is GetType(Integer) OrElse vT.IsEnum Then
                Value = val
            Else
                Throw New InvalidOperationException("required enum type or int")
            End If
        End Sub
    End Class
End Namespace

