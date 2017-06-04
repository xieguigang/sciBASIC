#Region "Microsoft.VisualBasic::da03af28526f9d711f02223e4cce9be9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\InteropService\SharedORM\CLI.vb"

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

Imports System.Reflection
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    <AttributeUsage(AttributeTargets.Class)>
    Public Class CLIAttribute : Inherits Attribute

        Public Shared Function ParseAssembly(dll$) As Type
            Dim assembly As Assembly = Assembly.LoadFile(dll)
            Dim type As Type = LinqAPI.DefaultFirst(Of Type) <=
 _
                From t As Type
                In assembly.GetTypes
                Where Not t.GetCustomAttribute(Of CLIAttribute) Is Nothing
                Select t ' 

            Return type
        End Function
    End Class
End Namespace
