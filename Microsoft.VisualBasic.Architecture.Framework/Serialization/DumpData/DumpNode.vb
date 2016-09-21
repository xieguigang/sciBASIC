#Region "Microsoft.VisualBasic::9970b4acad1ba83e1db1ba568f1d30eb, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Serialization\DumpData\DumpNode.vb"

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

Imports System.Data.Linq.Mapping

Namespace Serialization

    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field, allowmultiple:=False, inherited:=True)>
    Public Class DumpNode : Inherits Attribute
        Public Shared ReadOnly [GetTypeId] As System.Type = GetType(DumpNode)
    End Class
End Namespace