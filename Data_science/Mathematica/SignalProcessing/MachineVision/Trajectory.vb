#Region "Microsoft.VisualBasic::9f34c269e214c00436680a49e07d7ffd, Data_science\Mathematica\SignalProcessing\MachineVision\Trajectory.vb"

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

    '   Total Lines: 48
    '    Code Lines: 33 (68.75%)
    ' Comment Lines: 7 (14.58%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (16.67%)
    '     File Size: 1.27 KB


    ' Class Trajectory
    ' 
    '     Properties: LastPosition, objectSet, positions, TrajectoryID
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Update
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Trajectory of a object
''' </summary>
Public Class Trajectory

    Public ReadOnly Property TrajectoryID As Integer
    Public ReadOnly Property positions As PointF()
        Get
            Return objectSet _
                .SafeQuery _
                .Select(Function(o) CType(o, PointF)) _
                .ToArray
        End Get
    End Property

    Public ReadOnly Property LastPosition As PointF
        Get
            Return objectSet.Last()
        End Get
    End Property

    Public Property objectSet As New List(Of Detection)

    Default Public ReadOnly Property GetFrame(i As Integer) As Detection
        Get
            Return _objectSet(i)
        End Get
    End Property

    Public Sub New(id As Integer, t0 As Detection)
        TrajectoryID = id
        objectSet.Add(t0)
    End Sub

    ''' <summary>
    ''' Add last position to the current object trajectory
    ''' </summary>
    ''' <param name="detection"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Update(detection As Detection)
        Call objectSet.Add(detection)
    End Sub

End Class
