#Region "Microsoft.VisualBasic::950315d0105e3283ee9bb403e4a738ab, ..\sciBASIC#\Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\CLI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    <AttributeUsage(AttributeTargets.Class)>
    Public Class CLIAttribute : Inherits Attribute

        Public Shared Function ParseAssembly(dll$) As Type
            Dim assembly As Assembly = Assembly.LoadFile(dll)
            Dim type As Type = LinqAPI.DefaultFirst(Of Type) _
 _
                () <= From t As Type
                      In EmitReflection.GetTypesHelper(assembly)
                      Where Not t.GetCustomAttribute(Of CLIAttribute) Is Nothing
                      Select t ' 

            Return type
        End Function
    End Class
End Namespace
