#Region "Microsoft.VisualBasic::cee71cd243701898c0b74795d745cfbf, ..\VisualBasic_AppFramework\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing3D\Point3D.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing

Namespace Drawing3D

    ''' <summary>
    ''' Defines the Point3D class that represents points in 3D space.
    ''' Developed by leonelmachava &lt;leonelmachava@gmail.com>
    ''' http://codentronix.com
    '''
    ''' Copyright (c) 2011 Leonel Machava
    ''' </summary>
    Public Structure Point3D

        Public Sub New(x As Double, y As Double, Optional z As Double = 0)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public Sub New(Position As Point)
            Call Me.New(Position.X, Position.Y)
        End Sub

        Public Property X As Double
        Public Property Y As Double
        Public Property Z As Double

        Public Function RotateX(angle As Integer) As Point3D
            Dim rad As Double, cosa As Double, sina As Double, yn As Double, zn As Double

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            yn = Me.Y * cosa - Me.Z * sina
            zn = Me.Y * sina + Me.Z * cosa
            Return New Point3D(Me.X, yn, zn)
        End Function

        Public Function RotateY(angle As Integer) As Point3D
            Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Zn As Double

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            Zn = Me.Z * cosa - Me.X * sina
            Xn = Me.Z * sina + Me.X * cosa

            Return New Point3D(Xn, Me.Y, Zn)
        End Function

        Public Function RotateZ(angle As Integer) As Point3D
            Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Yn As Double

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            Xn = Me.X * cosa - Me.Y * sina
            Yn = Me.X * sina + Me.Y * cosa
            Return New Point3D(Xn, Yn, Me.Z)
        End Function

        Public Function Project(viewWidth As Integer, viewHeight As Integer, fov As Integer, viewDistance As Integer) As Point3D
            Dim factor As Double, Xn As Double, Yn As Double
            factor = fov / (viewDistance + Me.Z)
            Xn = Me.X * factor + viewWidth / 2
            Yn = Me.Y * factor + viewHeight / 2
            Return New Point3D(Xn, Yn, Me.Z)
        End Function

        Public Shared Sub Project(ByRef x As Single,
                                  ByRef y As Single,
                                  ByRef z As Single,
                                  viewWidth As Integer,
                                  viewHeight As Integer,
                                  fov As Integer,
                                  viewDistance As Integer)

            Dim factor As Double

            factor = fov / (viewDistance + z)
            x = x * factor + viewWidth / 2
            y = y * factor + viewHeight / 2
        End Sub
    End Structure
End Namespace
