#Region "Microsoft.VisualBasic::a4a5bfce4d442e3556100228404e955f, vs_solutions\dev\LicenseMgr\LicenseMgr\BingImage.vb"

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

    ' Class BingImage
    ' 
    '     Properties: UseBingImage
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetUseBingImage
    ' 
    '     Sub: SetUseBingImage
    ' 
    ' /********************************************************************************/

#End Region

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

    Public Shared Function GetUseBingImage(o As DependencyObject) As Boolean
        Return CBool(o.GetValue(UseBingImageProperty))
    End Function

    Public Shared Sub SetUseBingImage(o As DependencyObject, value As Boolean)
        o.SetValue(UseBingImageProperty, value)
    End Sub
End Class
