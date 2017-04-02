#Region "Microsoft.VisualBasic::731f4d8a6932e2a8368950dacde8f27a, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Vectogram.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text
Imports Microsoft.VisualBasic.Text

Namespace Drawing2D

    ''' <summary>
    ''' 用来描述一个向量图的GDI绘图设备
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Vectogram : Implements ISaveHandle
        Implements IDisposable
        Implements IEnumerable(Of Shape)

        Dim _lstElements As List(Of Shape) = New List(Of Shape)

        Public ReadOnly Property GDIDevice As Graphics2D

        Public ReadOnly Property Size As Size
            Get
                Return _GDIDevice.Size
            End Get
        End Property

#Region "Constructors"

        Sub New(Size As Size)
            _GDIDevice = Size.CreateGDIDevice
        End Sub

        Sub New(Width As Integer, Height As Integer)
            Call Me.New(New Size(Width, Height))
        End Sub

        Sub New(Width As Integer, AspectRatio As Single)
            Call Me.New(New Size(Width, CInt(Width * AspectRatio)))
        End Sub

        Sub New(ImageResource As Image)
            _GDIDevice = ImageResource.GdiFromImage
        End Sub
#End Region

        Public Function AddDrawingElement(Element As Shape) As Integer
            Call Me._lstElements.Add(Element)
            Return Me._lstElements.Count
        End Function

        Public Function AddTextElement(str As String, Font As Font, Color As Color, Location As Point) As [String]
            Dim strElement As New [String](str, Font, New Rectangle(Location, New Size)) With {
                .Pen = New SolidBrush(Color)
            }
            '   Call Me._lstElements.Add(strElement)
            Return strElement
        End Function

        Public Function AddCircle(FillColor As Color, TopLeft As Point, d As Integer) As Circle
            Dim Circle As New Circle(topLeft:=TopLeft, d:=d, FillColor:=FillColor)
            Call Me._lstElements.Add(Circle)
            Return Circle
        End Function

        Public Function ToImage() As Image
            For Each Element As Shape In Me._lstElements
                Call Element.Draw(GDIDevice.Graphics)
            Next

            Return _GDIDevice.ImageResource
        End Function

        Public Overrides Function ToString() As String
            Return _GDIDevice.ToString
        End Function

#Region "System Interface Implements"

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose( disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose( disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Iterator Function GetEnumerator() As IEnumerator(Of Shape) Implements IEnumerable(Of Shape).GetEnumerator
            For Each DrawingElement In Me._lstElements
                Yield DrawingElement
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Function Save(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            If String.IsNullOrEmpty(Path) Then
                Path = "./Vectogram.vcs"
            End If

            Dim Script As String = New Drawing2D.DrawingScript(Me).ToScript
            Return Script.SaveTo(Path, encoding)
        End Function

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.CodePage)
        End Function
#End Region
    End Class
End Namespace
