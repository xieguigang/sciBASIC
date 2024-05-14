#Region "Microsoft.VisualBasic::8f5dc2d2723bf368ee62ccf0148ba17d, Microsoft.VisualBasic.Core\src\Data\DefaultAttribute.vb"

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

    '   Total Lines: 38
    '    Code Lines: 29
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 1.52 KB


    '     Class DefaultAttribute
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDefaultAttribute, GetDefaultValues, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Property default value
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class DefaultAttribute : Inherits DefaultValueAttribute

        Sub New(value As Object)
            Call MyBase.New(value)
        End Sub

        Public Overrides Function ToString() As String
            Return InputHandler.ToString(Value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDefaultValues(type As Type) As Dictionary(Of String, Object)
            Return type.Schema(PropertyAccess.Readable, PublicProperty, nonIndex:=True) _
                       .ToDictionary(Function(p) p.Value.Name,
                                     Function(p)
                                         Return GetDefaultAttribute(p.Value)?.Value
                                     End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDefaultAttribute([property] As PropertyInfo) As DefaultValueAttribute
            Return [property] _
                .GetCustomAttributes(Of DefaultValueAttribute)(inherit:=True) _
                .FirstOrDefault
        End Function
    End Class
End Namespace
