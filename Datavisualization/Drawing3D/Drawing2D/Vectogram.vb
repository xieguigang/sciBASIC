Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Drawing.Drawing2D.VectorElements

Namespace Drawing2D

    ''' <summary>
    ''' 用来描述一个向量图的GDI绘图设备
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Vectogram : Implements ISaveHandle

        Implements System.IDisposable
        Implements Generic.IEnumerable(Of Drawing2D.VectorElements.LayoutsElement)

        Dim _InternalGDIDevice As Microsoft.VisualBasic.GDIPlusDeviceHandle
        Dim _InternalDrawingElementList As List(Of Drawing2D.VectorElements.LayoutsElement) =
            New List(Of Drawing2D.VectorElements.LayoutsElement)

        Public ReadOnly Property GDIDevice As GDIPlusDeviceHandle
            Get
                Return _InternalGDIDevice
            End Get
        End Property

        Public ReadOnly Property Size As Size
            Get
                Return _InternalGDIDevice.Size
            End Get
        End Property

#Region "Constructors"

        Sub New(Size As Size)
            _InternalGDIDevice = Size.CreateGDIDevice
        End Sub

        Sub New(Width As Integer, Height As Integer)
            Call Me.New(New Size(Width, Height))
        End Sub

        Sub New(Width As Integer, AspectRatio As Double)
            Call Me.New(New Size(Width, CInt(Width * AspectRatio)))
        End Sub

        Sub New(ImageResource As Image)
            _InternalGDIDevice = ImageResource.GrFromImage
        End Sub
#End Region

        Public Function AddDrawingElement(Element As Drawing2D.VectorElements.LayoutsElement) As Integer
            Call Me._InternalDrawingElementList.Add(Element)
            Return Me._InternalDrawingElementList.Count
        End Function

        Public Function AddTextElement(str As String, Font As Font, Color As Color, Location As Point) As Drawing2D.VectorElements.DrawingString
            Dim strElement As DrawingString = New DrawingString(str, Color, Me._InternalGDIDevice, Location) With {.Font = Font}
            Call Me._InternalDrawingElementList.Add(strElement)
            Return strElement
        End Function

        Public Function AddCircle(FillColor As Color, TopLeft As Point, d As Integer) As Drawing2D.VectorElements.Circle
            Dim Circle As New Drawing2D.VectorElements.Circle(LeftTop:=TopLeft, D:=d, GDI:=Me._InternalGDIDevice, FillColor:=FillColor)
            Call Me._InternalDrawingElementList.Add(Circle)
            Return Circle
        End Function

        Public Function ToImage() As Image
            For Each Element As LayoutsElement In Me._InternalDrawingElementList
                Call Element.InvokeDrawing()
            Next

            Return _InternalGDIDevice.ImageResource
        End Function

        Public Overrides Function ToString() As String
            Return _InternalGDIDevice.ToString
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

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
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

        Public Iterator Function GetEnumerator() As IEnumerator(Of LayoutsElement) Implements IEnumerable(Of LayoutsElement).GetEnumerator
            For Each DrawingElement In Me._InternalDrawingElementList
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
            Return Save(Path, encoding.GetEncodings)
        End Function
#End Region
    End Class
End Namespace