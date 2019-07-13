#Region "Microsoft.VisualBasic::196f606a0eab968477bea059d0ec3ace, Microsoft.VisualBasic.Core\Extensions\Image\GDI+\FontFace.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class FontFace
    ' 
    '         Properties: InstalledFontFamilies
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetFontName, IsInstalled, MeasureString
    ' 
    '     Module DefaultFontValues
    ' 
    ' 
    '         Class MicrosoftYaHei
    ' 
    '             Properties: Bold, Large, Normal
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports DefaultFont = Microsoft.VisualBasic.Language.Default.Default(Of System.Drawing.Font)

Namespace Imaging

    ''' <summary>
    ''' Font names collection
    ''' </summary>
    Public NotInheritable Class FontFace

        ''' <summary>
        ''' 微软雅黑字体的名称
        ''' </summary>
        Public Const MicrosoftYaHei As String = "Microsoft YaHei"
        Public Const MicrosoftYaHeiUI As String = "Microsoft YaHei UI"
        Public Const Ubuntu As String = "Ubuntu"
        Public Const SegoeUI As String = "Segoe UI"
        Public Const Arial As String = "Arial"
        Public Const BookmanOldStyle As String = "Bookman Old Style"
        Public Const Calibri As String = "Calibri"
        Public Const Cambria As String = "Cambria"
        Public Const CambriaMath As String = "Cambria Math"
        Public Const Consolas As String = "Consolas"
        Public Const CourierNew As String = "Courier New"
        Public Const NSimSun As String = "NSimSun"
        Public Const SimSun As String = "SimSun"
        Public Const Verdana As String = "Verdana"
        Public Const Tahoma As String = "Tahoma"
        Public Const TimesNewRoman As String = "Times New Roman"

        Public Shared ReadOnly Property InstalledFontFamilies As IReadOnlyCollection(Of String)

        Shared ReadOnly fontFamilies As Dictionary(Of String, String)

        Shared Sub New()
            Dim fontFamilies() As FontFamily
            Dim installedFontCollection As New InstalledFontCollection()

            ' Get the array of FontFamily objects.
            fontFamilies = installedFontCollection.Families
            InstalledFontFamilies = fontFamilies.Select(Function(f) f.Name).ToArray
            FontFace.fontFamilies = New Dictionary(Of String, String)

            For Each family$ In InstalledFontFamilies
                FontFace.fontFamilies(LCase(family)) = family
            Next
        End Sub

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 检查当前的操作系统之中是否安装有指定名称的字体
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsInstalled(name As String) As Boolean
            Return fontFamilies.ContainsKey(name) OrElse fontFamilies.ContainsKey(LCase(name))
        End Function

        ''' <summary>
        ''' 由于字体名称的大小写敏感，所以假若是html css之类的渲染的话，由于可能会是小写的字体名称会导致无法
        ''' 正确的加载所需要的字体，所以可以使用这个函数来消除这种由于大小写敏感而带来的bug
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="default">默认使用Windows10的默认字体</param>
        ''' <returns></returns>
        Public Shared Function GetFontName(name$, Optional default$ = FontFace.SegoeUI) As String
            If fontFamilies.ContainsKey(name) Then
                Return fontFamilies(name)
            Else
                name = LCase(name)

                If fontFamilies.ContainsKey(name) Then
                    Return fontFamilies(name)
                Else
                    Return [default]
                End If
            End If
        End Function

        Public Shared Function MeasureString(text As String, font As Font) As SizeF
            Static dummy_img As Image = New Bitmap(1, 1)
            Static dummy_drawing As Graphics = Graphics.FromImage(dummy_img)

            Return dummy_drawing.MeasureString(text, font)
        End Function
    End Class

    ''' <summary>
    ''' Default font values
    ''' </summary>
    Public Module DefaultFontValues

        Public NotInheritable Class MicrosoftYaHei

            Public Shared ReadOnly Property Normal As DefaultFont = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Regular)
            Public Shared ReadOnly Property Large As DefaultFont = New Font(FontFace.MicrosoftYaHei, 30, FontStyle.Regular)
            Public Shared ReadOnly Property Bold As DefaultFont = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Bold)

            Private Sub New()
            End Sub
        End Class
    End Module
End Namespace
