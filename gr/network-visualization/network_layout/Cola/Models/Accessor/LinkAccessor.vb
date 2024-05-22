#Region "Microsoft.VisualBasic::986f0ec2e98f21afd042e249390c8b46, gr\network-visualization\network_layout\Cola\Models\Accessor\LinkAccessor.vb"

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

    '   Total Lines: 27
    '    Code Lines: 16 (59.26%)
    ' Comment Lines: 4 (14.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (25.93%)
    '     File Size: 881 B


    '     Class LinkAccessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class LinkTypeAccessor
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: GetLinkType
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Cola

    Public Class LinkAccessor : Inherits LinkAccessor(Of Link(Of Integer))

        Public getLength As Func(Of Link(Of Integer), Double)

        Sub New()
            Me.getSourceIndex = Function(e) e.source
            Me.getTargetIndex = Function(e) e.target
            Me.getLength = Function(e) e.length
            Me.setLength = Sub(e, l) e.length = l
        End Sub
    End Class

    Public Class LinkTypeAccessor(Of Link) : Inherits LinkAccessor(Of Link)

        Public Delegate Function IGetLinkType(link As Link) As Integer

        ''' <summary>
        ''' return a unique identifier for the type of the link
        ''' </summary>
        ''' <returns></returns>
        Public Property GetLinkType As IGetLinkType
    End Class
End Namespace
