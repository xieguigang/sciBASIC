#Region "Microsoft.VisualBasic::f7dba7f49e2fcca27a0e385727cd04b6, mime\text%yaml\1.2\Syntax\Directive.vb"

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

    '   Total Lines: 42
    '    Code Lines: 28
    ' Comment Lines: 3
    '   Blank Lines: 11
    '     File Size: 1.09 KB


    '     Class Directive
    ' 
    ' 
    ' 
    '     Class YamlDirective
    ' 
    '         Function: ToString
    ' 
    '     Class TagDirective
    ' 
    '         Function: ToString
    ' 
    '     Class ReservedDirective
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Syntax

    Public MustInherit Class Directive
    End Class

    ''' <summary>
    ''' YAML version
    ''' </summary>
    Public Class YamlDirective
        Inherits Directive

        Public Version As YamlVersion

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType().Name}> {Version.ToString}"
        End Function
    End Class

    Public Class TagDirective
        Inherits Directive

        Public Prefix As TagPrefix
        Public Handle As TagHandle

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Prefix.ToString} {Handle.ToString}"
        End Function
    End Class

    Public Class ReservedDirective
        Inherits Directive

        Public Name As String
        Public Parameters As New List(Of String)()

        Public Overrides Function ToString() As String
            Return $"{Me.GetType.Name} {Name}({Parameters.SafeQuery.JoinBy(", ")})"
        End Function
    End Class
End Namespace
