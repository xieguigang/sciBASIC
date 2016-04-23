Namespace Visualise.Elements

    Public Class ImageResource

        Public Property BackgroundImage As Image
        Public Property BackColor As Color

        Sub New()
        End Sub

        Sub New(res As Image)
            BackgroundImage = res
        End Sub

        Sub New(color As Color)
            BackColor = color
        End Sub

        Public Sub SetResources(ctrl As Control)
            If BackgroundImage Is Nothing Then
                ctrl.BackColor = BackColor
                ctrl.BackgroundImage = Nothing
            Else
                ctrl.BackgroundImage = BackgroundImage
            End If
        End Sub
    End Class

    ''' <summary>
    ''' 标题栏的视觉元素的数据包
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CaptionResources

        Public Property BackgroundImage As ImageResource
        Public Property InformationArea As ImageResource
        Public Property Icon As Image

        Public Const YaHei As String = "Microsoft YaHei"

        Public Property Minimize As ButtonResource
        Public Property Maximum As ButtonResource
        Public Property Close As ButtonResource
        Public Property ControlBox As ImageResource
        ''' <summary>
        ''' The button size of the <see cref="Minimize"/>, <see cref="Maximum"/> and <see cref="Close"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property ControlboxButtonSize As Size

        Public Property TitleFont As Font = New Font(FontFace.MicrosoftYaHei, 16)
        Public Property SubCaptionFont As Font = New Font(FontFace.MicrosoftYaHei, 10)

        Public Property ShowText As Boolean = True
        Public Property ShowIcon As Boolean = True

        Public Property CloseButtonTooltip As String = "Close Window"
        Public Property MinButtonTooltip As String = "Minimize"
        Public Property MaxButtonTooltip As String = "Maximize"

        Public ReadOnly Property TitleLength(CaptionText As String, SubCaptionText As String) As Integer
            Get
                Return Math.Max(CaptionText.MeasureString(TitleFont).Width, SubCaptionText.MeasureString(SubCaptionFont).Width) + 60
            End Get
        End Property

        ''' <summary>
        ''' 标题栏的高度上限
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HeightLimits As Integer

        Public Property BorderPen As Pen

        Public Sub DrawCaption(CaptionText As String, SubCaptionText As String, caption As Windows.Forms.Controls.Caption)
            If InformationArea Is Nothing Then
                InformationArea = New ImageResource(caption.BackColor)
            End If

            Dim ImageResources As Image = InformationArea.BackgroundImage
            Dim TextFont As New Font(YaHei, 15)
            Dim SubCaptionFont As New Font(YaHei, 9)

            If ImageResources Is Nothing Then
                ImageResources = caption.Size.CreateGDIDevice(InformationArea.BackColor)
            End If

            Using Gr As Graphics = Graphics.FromImage(image:=ImageResources)

                Dim X, Y As Integer

                If ShowIcon AndAlso Not Icon Is Nothing Then
                    X = -1 * (Icon.Width - 42) / 2  'Central the icon in the left side of the caption bar
                    Y = -1 * (Icon.Height - 42) / 2

                    Call Gr.DrawImage(Icon, X, Y, Icon.Width, Icon.Height)

                    X = 58
                Else
                    X = 5
                End If

                If ShowText Then
                    Call Gr.DrawString(CaptionText, TextFont, Brushes.White, New Point With {.X = X, .Y = 7})
                    Call Gr.DrawString(SubCaptionText, SubCaptionFont, Brushes.White, New Point With {.X = X + 2, .Y = 25})
                End If

                caption.InformationArea.BackgroundImage = ImageResources
            End Using
        End Sub

        Public Function GetIcon() As Icon
            Return CType(Me, Icon)
        End Function

        Public Function GenerateToolsTip(CaptionText As String, SubCaptionText As String) As String
            Return String.Format("{0} - {1}", CaptionText, SubCaptionText)
        End Function

        Public Shared Narrowing Operator CType(obj As CaptionResources) As Icon
            If obj.Icon Is Nothing Then
                Return Nothing
            End If
            Return System.Drawing.Icon.FromHandle(CType(obj.Icon, Bitmap).GetHicon)
        End Operator

        ''' <summary>
        ''' Default UI visualize style.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Molk() As CaptionResources
            Dim Caption As New CaptionResources

            Caption.Minimize = New ButtonResource With {.Normal = My.Resources.Minimize}
            Caption.Close = New ButtonResource With {.Normal = My.Resources.Close}
            Caption.ControlBox = New ImageResource(My.Resources.ControlBox)
            Caption.Maximum = New ButtonResource With {.Normal = My.Resources.Maximum}
            Caption.BackgroundImage = New ImageResource(My.Resources.Caption)
            Caption.InformationArea = New ImageResource(My.Resources.Caption)
            Caption.Icon = My.Resources.vs10ide
            Caption.ControlboxButtonSize = New Size(30, 18)
            Caption.HeightLimits = 44

            Return Caption
        End Function
    End Class
End Namespace