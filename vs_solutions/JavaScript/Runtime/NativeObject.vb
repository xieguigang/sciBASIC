#Region "Microsoft.VisualBasic::46f338d54ec8e5f2db1b48ab875899bc, vs_solutions\JavaScript\Runtime\NativeObject.vb"

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

    '   Total Lines: 11
    '    Code Lines: 8 (72.73%)
    ' Comment Lines: 1 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (18.18%)
    '     File Size: 325 B


    '     Class NativeObject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>Builtin container object (Math, console, JSON, Object, Array, global fns).</summary>
    Public NotInheritable Class NativeObject
        Public ReadOnly Name As String
        Public Sub New(name As String)
            Me.Name = name
        End Sub
    End Class

End Namespace
