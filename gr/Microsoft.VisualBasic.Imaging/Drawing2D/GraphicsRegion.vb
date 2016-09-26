Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D

    ''' <summary>
    ''' 绘图区域的参数
    ''' </summary>
    Public Structure GraphicsRegion

        Public Size As Size
        Public Margin As Size

        Public ReadOnly Property GraphicsRegion As Rectangle
            Get
                Dim topLeft As New Point(Margin.Width, Margin.Height)
                Dim size As New Size(
                    Me.Size.Width - Margin.Width * 2,
                    Me.Size.Height - Margin.Height * 2)

                Return New Rectangle(topLeft, size)
            End Get
        End Property

        Public ReadOnly Property EntireArea As Rectangle
            Get
                Return New Rectangle(New Point, Size)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace