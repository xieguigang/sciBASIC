#Region "Microsoft.VisualBasic::2603da6939dad1febeb9f9e095816bd5, gr\network-visualization\network_layout\Cola\PowerGraph\PowerEdge.vb"

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

    '   Total Lines: 43
    '    Code Lines: 37 (86.05%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (13.95%)
    '     File Size: 1.21 KB


    '     Class PowerEdge
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Cola

    Public Class PowerEdge(Of T)

        Public source As T
        Public target As T
        Public type As Integer

        Default Public Property Item(name As String) As T
            Get
                If name = NameOf(source) Then
                    Return source
                ElseIf name = NameOf(target) Then
                    Return target
                Else
                    Throw New NotImplementedException(name)
                End If
            End Get
            Set
                If name = NameOf(source) Then
                    source = Value
                ElseIf name = NameOf(target) Then
                    target = Value
                Else
                    Throw New NotImplementedException(name)
                End If
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(source As T, target As T, type As Integer)
            Me.source = source
            Me.target = target
            Me.type = type
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{source}, {target}]"
        End Function
    End Class
End Namespace
