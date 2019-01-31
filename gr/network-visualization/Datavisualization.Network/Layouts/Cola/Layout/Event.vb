#Region "Microsoft.VisualBasic::e6b99b0af378808e16ec804c861f941d, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout\Event.vb"

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

    '     Enum EventType
    ' 
    '         [end], start, tick
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class [Event]
    ' 
    ' 
    '         Structure Comparer
    ' 
    '             Function: Compare
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    ''' <summary>
    ''' The layout process fires three events:
    ''' 
    ''' > - ``start``: layout iterations started
    ''' > - ``tick``: fired once per iteration, listen to this to animate
    ''' > - ``end``: layout converged, you might like to zoom-to-fit or 
    ''' >       something at notification of this event
    ''' </summary>
    Public Enum EventType
        ''' <summary>
        ''' layout iterations started
        ''' </summary>
        start
        ''' <summary>
        ''' fired once per iteration, listen to this to animate
        ''' </summary>
        tick
        ''' <summary>
        ''' layout converged, you might like to zoom-to-fit or something 
        ''' at notification of this event
        ''' </summary>
        [end]
    End Enum

    Public Class [Event]

        Public type As EventType
        Public alpha As Double
        Public stress As Double
        Public listener As Action
        Public s As Segment
        Public pos As Double
        Public isOpen As Boolean
        Public v As Node

        Public Structure Comparer : Implements IComparer(Of [Event])

            Public Function Compare(a As [Event], b As [Event]) As Integer Implements IComparer(Of [Event]).Compare
                Return a.pos - b.pos + a.type - b.type
            End Function
        End Structure

        Default Public Property Accessor(name As String) As Object
            Get
                Select Case name
                    Case NameOf(type)
                        Return type
                    Case NameOf(alpha)
                        Return alpha
                    Case NameOf(stress)
                        Return stress
                    Case NameOf(listener)
                        Return listener
                    Case Else
                        Throw New NotImplementedException(name)
                End Select
            End Get
            Set(value As Object)
                Select Case name
                    Case NameOf(type)
                        type = value
                    Case NameOf(alpha)
                        alpha = value
                    Case NameOf(stress)
                        stress = value
                    Case NameOf(listener)
                        listener = value
                    Case Else
                        Throw New NotImplementedException(name)
                End Select
            End Set
        End Property
    End Class
End Namespace
