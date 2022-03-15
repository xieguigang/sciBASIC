#Region "Microsoft.VisualBasic::92bff8f4f2603c5bcb01a9b70bd3828b, sciBASIC#\gr\OpenGL\Camera.vb"

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

    '   Total Lines: 45
    '    Code Lines: 36
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.37 KB


    ' Class Camera
    ' 
    '     Properties: At, Pitch, Position, Right, Roll
    '                 UP, Yaw
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: ToString
    ' 
    '     Sub: InvokePitch, Render, Walk
    ' 
    ' /********************************************************************************/

#End Region

Imports OpenTK
Imports OpenTK.Graphics.OpenGL

Public Class Camera

    Public Property Position As Vector3
    Public Property Pitch As Single
    Public Property Yaw As Single
    Public Property Roll As Single

    Public ReadOnly Property At As Vector3
    Public Property UP As Vector3
    Public Property Right As Vector3

    Public Sub New(x As Single, y As Single, z As Single)
        Call Me.New(New Vector3(x, y, z), New Vector3(0.0, 0.0, -1.0))
    End Sub

    Public Sub New(pos As Vector3, at As Vector3)
        Me.Position = pos
        Me.At = at
    End Sub

    Public Sub InvokePitch(amount As Single)
        _Pitch += amount
        Dim mat As Matrix4 = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Pitch))
        _At = Vector3.TransformVector(_At, mat)
    End Sub

    Public Sub Render()
        Call GL.Rotate(-Pitch, 1, 0, 0)
        Call GL.Rotate(-Yaw, 0, 1, 0)
        Call GL.Rotate(-Roll, 0, 0, 1)
        Call GL.Translate(-Position.X, -Position.Y, -Position.Z)
    End Sub

    Public Overrides Function ToString() As String
        Return Position.ToString
    End Function

    Public Sub Walk(delta As Single)
        _Position.Z -= delta * Math.Cos(MathHelper.DegreesToRadians(Yaw))
        _Position.X -= delta * Math.Sin(MathHelper.DegreesToRadians(Yaw))
    End Sub
End Class
