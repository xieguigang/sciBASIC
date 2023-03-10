''' <summary>
''' Barcode box class
''' </summary>
''' <remarks>
''' The barcode box class represent the total size of the barcode
''' plus optional text. It is used by PdfTable class.
''' </remarks>
Public Class BarcodeBox
    ''' <summary>
    ''' Barcode origin X
    ''' </summary>
    Public OriginX As Double

    ''' <summary>
    ''' Barcode origin Y
    ''' </summary>
    Public OriginY As Double

    ''' <summary>
    ''' Total width including optional text.
    ''' </summary>
    Public TotalWidth As Double

    ''' <summary>
    ''' Total height including optional text.
    ''' </summary>
    Public TotalHeight As Double

    ''' <summary>
    ''' Constructor for no text
    ''' </summary>
    ''' <param name="TotalWidth">Total width</param>
    ''' <param name="TotalHeight">Total height</param>
    Public Sub New(TotalWidth As Double, TotalHeight As Double)
        Me.TotalWidth = TotalWidth
        Me.TotalHeight = TotalHeight
        Return
    End Sub

    ''' <summary>
    ''' Constructor for text included
    ''' </summary>
    ''' <param name="OriginX">Barcode origin X</param>
    ''' <param name="OriginY">Barcode origin Y</param>
    ''' <param name="TotalWidth">Total width</param>
    ''' <param name="TotalHeight">Total height</param>
    Public Sub New(OriginX As Double, OriginY As Double, TotalWidth As Double, TotalHeight As Double)
        Me.OriginX = OriginX
        Me.OriginY = OriginY
        Me.TotalWidth = TotalWidth
        Me.TotalHeight = TotalHeight
        Return
    End Sub
End Class
