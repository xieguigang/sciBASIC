#Region "Microsoft.VisualBasic::2c036c031dab620506de252ee5144e6c, vs_solutions\JavaScript\Runtime\SpreadMarker.vb"

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

    '   Total Lines: 20
    '    Code Lines: 8 (40.00%)
    ' Comment Lines: 6 (30.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (30.00%)
    '     File Size: 570 B


    '     Class SpreadMarker
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Runtime

    ''' <summary>
    ''' Marker wrapping the value of a spread element (<c>...xs</c>) while building
    ''' call argument lists, array literals and object literals —
    ''' consumed by <see cref="JsRuntime.JsBuildArgs"/> /
    ''' <see cref="JsRuntime.JsArrayFrom"/> / <see cref="JsRuntime.JsObjFrom"/>.
    ''' </summary>
    Public NotInheritable Class SpreadMarker

        Public ReadOnly Value As Object

        Public Sub New(value As Object)
            Me.Value = value
        End Sub

    End Class

End Namespace

