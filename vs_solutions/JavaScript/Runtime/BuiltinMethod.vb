#Region "Microsoft.VisualBasic::e400301d1b458e97361d7ca6267363e9, vs_solutions\JavaScript\Runtime\BuiltinMethod.vb"

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

    '   Total Lines: 13
    '    Code Lines: 10 (76.92%)
    ' Comment Lines: 1 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (15.38%)
    '     File Size: 434 B


    '     Class BuiltinMethod
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>A builtin method bound to its receiver; callable through <see cref="JsRuntime.JsInvoke"/>.</summary>
    Public NotInheritable Class BuiltinMethod
        Public ReadOnly Target As Object
        Public ReadOnly Name As String
        Public Sub New(target As Object, name As String)
            Me.Target = target
            Me.Name = name
        End Sub
    End Class

End Namespace
