Imports FirstFloor.ModernUI.Presentation
Imports System.ComponentModel
Imports System.Linq
Imports System.Windows.Media

Namespace Content
	''' <summary>
	''' A simple view model for configuring theme, font and accent colors.
	''' </summary>
	Public Class SettingsAppearanceViewModel
		Inherits NotifyPropertyChanged
		Private Const FontSmall As String = "small"
		Private Const FontLarge As String = "large"

		Private Const PaletteMetro As String = "metro"
		Private Const PaletteWP As String = "windows phone"
		Private Const PaletteCustom As String = "custom"

		' 9 accent colors from metro design principles
		' blue
		' teal
		' green
		' lime
		' orange
		' orange red
		' red
		' magenta
			' purple
		Private metroAccentColors As Color() = New Color() {Color.FromRgb(&H33, &H99, &Hff), Color.FromRgb(&H0, &Hab, &Ha9), Color.FromRgb(&H33, &H99, &H33), Color.FromRgb(&H8c, &Hbf, &H26), Color.FromRgb(&Hf0, &H96, &H9), Color.FromRgb(&Hff, &H45, &H0), _
			Color.FromRgb(&He5, &H14, &H0), Color.FromRgb(&Hff, &H0, &H97), Color.FromRgb(&Ha2, &H0, &Hff)}

		' 20 accent colors from Windows Phone 8
		' lime
		' green
		' emerald
		' teal
		' cyan
		' cobalt
		' indigo
		' violet
		' pink
		' magenta
		' crimson
		' red
		' orange
		' amber
		' yellow
		' brown
		' olive
		' steel
		' mauve
			' taupe
		Private wpAccentColors As Color() = New Color() {Color.FromRgb(&Ha4, &Hc4, &H0), Color.FromRgb(&H60, &Ha9, &H17), Color.FromRgb(&H0, &H8a, &H0), Color.FromRgb(&H0, &Hab, &Ha9), Color.FromRgb(&H1b, &Ha1, &He2), Color.FromRgb(&H0, &H50, &Hef), _
			Color.FromRgb(&H6a, &H0, &Hff), Color.FromRgb(&Haa, &H0, &Hff), Color.FromRgb(&Hf4, &H72, &Hd0), Color.FromRgb(&Hd8, &H0, &H73), Color.FromRgb(&Ha2, &H0, &H25), Color.FromRgb(&He5, &H14, &H0), _
			Color.FromRgb(&Hfa, &H68, &H0), Color.FromRgb(&Hf0, &Ha3, &Ha), Color.FromRgb(&He3, &Hc8, &H0), Color.FromRgb(&H82, &H5a, &H2c), Color.FromRgb(&H6d, &H87, &H64), Color.FromRgb(&H64, &H76, &H87), _
			Color.FromRgb(&H76, &H60, &H8a), Color.FromRgb(&H87, &H79, &H4e)}

		' Custom accent colors
		' white
			' black
		Private customAccentColors As Color() = New Color() {Color.FromRgb(&Hff, &Hff, &Hff), Color.FromRgb(&H0, &H0, &H0)}

		Private m_selectedPalette As String = PaletteWP

		Private m_selectedAccentColor As Color
		Private m_themes As New LinkCollection()
		Private m_selectedTheme As Link
		Private m_selectedFontSize As String

		Public Sub New()
            ' add the default themes
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "dark",
                 .Source = AppearanceManager.DarkThemeSource
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "light",
                 .Source = AppearanceManager.LightThemeSource
            })

            ' add additional themes
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "bing image",
                 .Source = New Uri("/Assets/ModernUI.BingImage.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "hello kitty",
                 .Source = New Uri("/Assets/ModernUI.HelloKitty.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "love",
                 .Source = New Uri("/Assets/ModernUI.Love.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "snowflakes",
                 .Source = New Uri("/Assets/ModernUI.Snowflakes.xaml", UriKind.Relative)
            })

            ' add custom themes
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "windows 8.1",
                 .Source = New Uri("/Assets/ModernUI.Windows8.1.xaml", UriKind.Relative)
            })
            Me.m_themes.Add(New Link() With {
                 .DisplayName = "metro",
                 .Source = New Uri("/Assets/ModernUI.Metro.xaml", UriKind.Relative)
            })

            Me.SelectedFontSize = If(AppearanceManager.Current.FontSize = FontSize.Large, FontLarge, FontSmall)
			SyncThemeAndColor()

			AddHandler AppearanceManager.Current.PropertyChanged, AddressOf OnAppearanceManagerPropertyChanged
		End Sub

		Private Sub SyncThemeAndColor()
			' synchronizes the selected viewmodel theme with the actual theme used by the appearance manager.
			Me.SelectedTheme = Me.m_themes.FirstOrDefault(Function(l) l.Source.Equals(AppearanceManager.Current.ThemeSource))

			' and make sure accent color is up-to-date
			Me.SelectedAccentColor = AppearanceManager.Current.AccentColor
		End Sub

		Private Sub OnAppearanceManagerPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
			If e.PropertyName = "ThemeSource" OrElse e.PropertyName = "AccentColor" Then
				SyncThemeAndColor()
			End If
		End Sub

		Public ReadOnly Property Themes() As LinkCollection
			Get
				Return Me.m_themes
			End Get
		End Property

		Public ReadOnly Property FontSizes() As String()
			Get
				Return New String() {FontSmall, FontLarge}
			End Get
		End Property

		Public ReadOnly Property Palettes() As String()
			Get
				Return New String() {PaletteMetro, PaletteWP, PaletteCustom}
			End Get
		End Property

		Public ReadOnly Property AccentColors() As Color()
			Get
				Return If(Me.m_selectedPalette = PaletteMetro, Me.metroAccentColors, If(Me.m_selectedPalette = PaletteWP, Me.wpAccentColors, Me.customAccentColors))
			End Get
		End Property

		Public Property SelectedPalette() As String
			Get
				Return Me.m_selectedPalette
			End Get
			Set
				If Me.m_selectedPalette <> value Then
					Me.m_selectedPalette = value
					OnPropertyChanged("AccentColors")

					Me.SelectedAccentColor = Me.AccentColors.FirstOrDefault()
				End If
			End Set
		End Property

		Public Property SelectedTheme() As Link
			Get
				Return Me.m_selectedTheme
			End Get
			Set
				If Me.m_selectedTheme IsNot value Then
					Me.m_selectedTheme = value
					OnPropertyChanged("SelectedTheme")

					' and update the actual theme
					AppearanceManager.Current.ThemeSource = value.Source
				End If
			End Set
		End Property

		Public Property SelectedFontSize() As String
			Get
				Return Me.m_selectedFontSize
			End Get
			Set
				If Me.m_selectedFontSize <> value Then
					Me.m_selectedFontSize = value
					OnPropertyChanged("SelectedFontSize")

					AppearanceManager.Current.FontSize = If(value = FontLarge, FontSize.Large, FontSize.Small)
				End If
			End Set
		End Property

		Public Property SelectedAccentColor() As Color
			Get
				Return Me.m_selectedAccentColor
			End Get
			Set
				If Me.m_selectedAccentColor <> value Then
					Me.m_selectedAccentColor = value
					OnPropertyChanged("SelectedAccentColor")

					AppearanceManager.Current.AccentColor = value
				End If
			End Set
		End Property
	End Class
End Namespace
