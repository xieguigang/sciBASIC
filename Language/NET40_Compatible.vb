#Region "Microsoft.VisualBasic::1fcb4aef4d239de44380d52e0062661e, Microsoft.VisualBasic.Core\Language\NET40_Compatible.vb"

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

    '     Class CallerMemberName
    ' 
    ' 
    ' 
    '     Interface IReadOnlyDictionary
    ' 
    ' 
    ' 
    '     Interface IReadOnlyCollection
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    ' Some advanced features are not supported in .NET 4.0

#If NET_40 = 1 Then

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class CallerMemberName : Inherits Attribute
    End Class

    Public Interface IReadOnlyDictionary(Of K, V) : Inherits IDictionary(Of K, V)
    End Interface

    Public Interface IReadOnlyCollection(Of T) : Inherits ICollection(Of T)
    End Interface
#End If
End Namespace
