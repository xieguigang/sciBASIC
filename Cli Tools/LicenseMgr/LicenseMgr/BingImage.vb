Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports System.Globalization
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Xml.Linq
Imports System.Xml.XPath

''' <summary>
''' Provides an attached property determining the current Bing image and assigning it to an image or imagebrush.
''' </summary>
Public NotInheritable Class BingImage
    Private Sub New()
    End Sub

    Public Shared ReadOnly UseBingImageProperty As DependencyProperty =
        DependencyProperty.RegisterAttached("UseBingImage", GetType(Boolean), GetType(BingImage), New PropertyMetadata(AddressOf OnUseBingImageChanged))

    Private Shared cachedBingImage As BitmapImage

    Private Shared Async Sub OnUseBingImageChanged(o As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim newValue = CBool(e.NewValue)
        Dim image = TryCast(o, Image)
        Dim imageBrush = TryCast(o, ImageBrush)

        If Not newValue OrElse (image Is Nothing AndAlso imageBrush Is Nothing) Then
            Return
        End If

        If cachedBingImage Is Nothing Then
            Dim url = Await GetCurrentBingImageUrl()
            If url IsNot Nothing Then
                cachedBingImage = New BitmapImage(url)
            Else
                cachedBingImage = New BitmapImage(New Uri("Assets/error.jpeg", UriKind.Relative))
                AppearanceManager.Current.AccentColor = Color.FromRgb(&HE5, &H14, &H0)
            End If
        End If

        If cachedBingImage IsNot Nothing Then
            If image IsNot Nothing Then
                image.Source = cachedBingImage
            ElseIf imageBrush IsNot Nothing Then
                imageBrush.ImageSource = cachedBingImage
            End If
        End If
    End Sub

    Private Shared Async Function GetCurrentBingImageUrl() As Task(Of Uri)
        Try
            Dim client = New HttpClient()
            Dim result = Await client.GetAsync("http://www.bing.com/hpimagearchive.aspx?format=xml&idx=0&n=2&mbl=1&mkt=en-ww")
            If result.IsSuccessStatusCode Then
                Using stream = Await result.Content.ReadAsStreamAsync()
                    Dim doc = XDocument.Load(stream)

                    Dim url = DirectCast(doc.XPathSelectElement("/images/image/url").Value, String)

                    Return New Uri(String.Format(CultureInfo.InvariantCulture, "http://bing.com{0}", url), UriKind.Absolute)
                End Using
            End If
        Catch generatedExceptionName As Exception
        End Try
        Return Nothing
    End Function

    Public Property UseBingImage As Boolean
        Get

        End Get
        Set(value As Boolean)

        End Set
    End Property

    Public Shared Function GetUseBingImage(o As DependencyObject) As Boolean
		Return CBool(o.GetValue(UseBingImageProperty))
	End Function

	Public Shared Sub SetUseBingImage(o As DependencyObject, value As Boolean)
		o.SetValue(UseBingImageProperty, value)
	End Sub
End Class
