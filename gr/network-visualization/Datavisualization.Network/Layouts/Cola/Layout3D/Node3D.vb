#Region "Microsoft.VisualBasic::4059f06d9685f72d40afc59012800b42, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Models\Node3D.vb"

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

    '     Class Node3D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class Node3D : Inherits Node

        Public variable As Variable

        Public z As Double

        Default Public Property Axis(dimName As String) As Double
            Get
                Select Case dimName
                    Case NameOf(x)
                        Return x
                    Case NameOf(y)
                        Return y
                    Case NameOf(z)
                        Return z
                    Case Else
                        Throw New NotImplementedException(dimName)
                End Select
            End Get
            Set(value As Double)
                Select Case dimName
                    Case NameOf(x)
                        x = value
                    Case NameOf(y)
                        y = value
                    Case NameOf(z)
                        z = value
                    Case Else
                        Throw New NotImplementedException(dimName)
                End Select
            End Set
        End Property

        Public Sub New(Optional x As Double = 0, Optional y As Double = 0, Optional z As Double = 0)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub
    End Class
End Namespace
