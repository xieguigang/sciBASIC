#Region "Microsoft.VisualBasic::64ebf96b77b33f6cb584edfd8f3c0501, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\CustomColorPicker.vb"

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

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Text
Imports System.Windows.Forms

Namespace Unity3.Controls
	Public Partial Class CustomColorPicker
		Inherits UserControl
		Implements IColorPicker
		#Region "Class Variables"

		Private m_hsl As ColorManager.HSL
		Private m_rgb As Color
		Private m_cmyk As ColorManager.CMYK

		Public Enum eDrawStyle
			Hue
			Saturation
			Brightness
			Red
			Green
			Blue
		End Enum


		#End Region

		Public Property Color() As Color Implements IColorPicker.Color
			Get
				Return Color.FromArgb(255 - tbAlpha.Value, m_rgb)
			End Get
			Set
				m_rgb = value
				tbAlpha.Value = 255 - m_rgb.A
				UpdateUI(value)
			End Set
		End Property

		Public Sub New(color As Color)
			InitializeComponent()
			lblOriginalColor.BackColor = color
			rbHue.Checked = True
			tbAlpha.Value = 255 - color.A
			UpdateUI(color)
		End Sub

		Private isUpdating As Boolean
		Private Sub UpdateUI(color__1 As Color)
			isUpdating = True
			m_rgb = color__1
			m_hsl = ColorManager.RGB_to_HSL(m_rgb)
			m_cmyk = ColorManager.RGB_to_CMYK(m_rgb)

			txtHue.Text = Round(m_hsl.H * 360).ToString()
			txtSat.Text = Round(m_hsl.S * 100).ToString()
			txtBrightness.Text = Round(m_hsl.L * 100).ToString()
			txtRed.Text = m_rgb.R.ToString()
			txtGreen.Text = m_rgb.G.ToString()
			txtBlue.Text = m_rgb.B.ToString()

			colorBox.HSL = m_hsl
			colorSlider.HSL = m_hsl

			colorPanelPending.Color = Color.FromArgb(255 - tbAlpha.Value, m_rgb)

			Me.WriteHexData(m_rgb)
			isUpdating = False
		End Sub


		#Region "Events"

		Private Sub colorBox_Scroll(sender As Object, e As System.EventArgs)
			If Not isUpdating Then
				UpdateUI(colorBox.RGB)
			End If
		End Sub

		Private Sub colorSlider_Scroll(sender As Object, e As System.EventArgs)
			If Not isUpdating Then
				UpdateUI(colorSlider.RGB)
			End If
		End Sub

		#Region "Hex Box (m_txt_Hex)"

		Private Sub txtHex_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = ""
			'm_txt_Hex.Text.ToUpper();
			Dim has_illegal_chars As Boolean = False

			If text.Length <= 0 Then
				has_illegal_chars = True
			End If
			For Each letter As Char In text
				If Not Char.IsNumber(letter) Then
					If letter >= "A"C AndAlso letter <= "F"C Then
						Continue For
					End If
					has_illegal_chars = True
					Exit For
				End If
			Next

			If has_illegal_chars Then
				MessageBox.Show("Hex must be a hex value between 0x000000 and 0xFFFFFF")
				WriteHexData(m_rgb)
				Return
			End If

			UpdateUI(ParseHexData(text))
		End Sub


		#End Region

		#Region "Color Boxes"

		Private Sub lblOriginalColor_Click(sender As Object, e As System.EventArgs)
		End Sub


		Private Sub m_lbl_Secondary_Color_Click(sender As Object, e As System.EventArgs)
		End Sub


		#End Region

		#Region "Radio Buttons"

		Private Sub rbHue_CheckedChanged(sender As Object, e As System.EventArgs)
			If rbHue.Checked Then
				colorSlider.DrawStyle = VerticalColorSlider.eDrawStyle.Hue
				colorBox.DrawStyle = ColorBox.eDrawStyle.Hue
			End If
		End Sub


		Private Sub rbSat_CheckedChanged(sender As Object, e As System.EventArgs)
			If rbSat.Checked Then
				colorSlider.DrawStyle = VerticalColorSlider.eDrawStyle.Saturation
				colorBox.DrawStyle = ColorBox.eDrawStyle.Saturation
			End If
		End Sub


		Private Sub rbBrightness_CheckedChanged(sender As Object, e As System.EventArgs)
			If rbBrightness.Checked Then
				colorSlider.DrawStyle = VerticalColorSlider.eDrawStyle.Brightness
				colorBox.DrawStyle = ColorBox.eDrawStyle.Brightness
			End If
		End Sub


		Private Sub rbRed_CheckedChanged(sender As Object, e As System.EventArgs)
			If rbRed.Checked Then
				colorSlider.DrawStyle = VerticalColorSlider.eDrawStyle.Red
				colorBox.DrawStyle = ColorBox.eDrawStyle.Red
			End If
		End Sub


		Private Sub rbGreen_CheckedChanged(sender As Object, e As System.EventArgs)
			If rbGreen.Checked Then
				colorSlider.DrawStyle = VerticalColorSlider.eDrawStyle.Green
				colorBox.DrawStyle = ColorBox.eDrawStyle.Green
			End If
		End Sub


		Private Sub rbBlue_CheckedChanged(sender As Object, e As System.EventArgs)
			If rbBlue.Checked Then
				colorSlider.DrawStyle = VerticalColorSlider.eDrawStyle.Blue
				colorBox.DrawStyle = ColorBox.eDrawStyle.Blue
			End If
		End Sub


		#End Region

		#Region "Text Boxes"

		Private Sub txtHue_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = txtHue.Text
			Dim has_illegal_chars As Boolean = False

			If text.Length < 1 OrElse text.Length > 3 Then
				has_illegal_chars = True
			Else
				For Each letter As Char In text
					If Not Char.IsNumber(letter) Then
						has_illegal_chars = True
						Exit For
					End If
				Next
			End If

			If has_illegal_chars Then
				MessageBox.Show("Hue must be a number value between 0 and 360")
				txtHue.Text = Round(m_hsl.H * 360).ToString()
				Return
			End If

			Dim hue As Integer = Integer.Parse(text)

			If hue < 0 OrElse hue > 360 Then
				MessageBox.Show("An integer between 0 and 360 is required." & vbLf & "Closest value inserted.")
				hue = If(hue < 0, 0, 360)
			End If

			m_hsl.H = CDbl(hue) / 360

			UpdateUI(ColorManager.HSL_to_RGB(m_hsl))
		End Sub


		Private Sub txtSat_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = txtSat.Text
			Dim has_illegal_chars As Boolean = False

			If text.Length < 1 OrElse text.Length > 3 Then
				has_illegal_chars = True
			Else
				For Each letter As Char In text
					If Not Char.IsNumber(letter) Then
						has_illegal_chars = True
						Exit For
					End If
				Next
			End If

			If has_illegal_chars Then
				MessageBox.Show("Saturation must be a number value between 0 and 100")
				txtSat.Text = Round(m_hsl.S * 100).ToString()
				Return
			End If

			Dim sat As Integer = Integer.Parse(text)

			If sat < 0 OrElse sat > 100 Then
				MessageBox.Show("An integer between 0 and 100 is required." & vbLf & "Closest value inserted.")
				sat = If(sat < 0, 0, 100)
			End If

			m_hsl.S = CDbl(sat) / 100
			UpdateUI(ColorManager.HSL_to_RGB(m_hsl))
		End Sub


		Private Sub txtBrightness_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = txtBrightness.Text
			Dim has_illegal_chars As Boolean = False

			If text.Length < 1 OrElse text.Length > 3 Then
				has_illegal_chars = True
			Else
				For Each letter As Char In text
					If Not Char.IsNumber(letter) Then
						has_illegal_chars = True
						Exit For
					End If
				Next
			End If

			If has_illegal_chars Then
				MessageBox.Show("Black must be a number value between 0 and 360")
				txtBrightness.Text = Round(m_hsl.L * 100).ToString()
				Return
			End If

			Dim lum As Integer = Integer.Parse(text)

			If lum < 0 OrElse lum > 360 Then
				MessageBox.Show("An integer between 0 and 100 is required." & vbLf & "Closest value inserted.")
				lum = If(lum < 0, 0, 360)
			End If

			m_hsl.L = CDbl(lum) / 100

			UpdateUI(ColorManager.HSL_to_RGB(m_hsl))
		End Sub


		Private Sub txtRed_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = txtRed.Text
			Dim has_illegal_chars As Boolean = False

			If text.Length < 1 OrElse text.Length > 3 Then
				has_illegal_chars = True
			Else
				For Each letter As Char In text
					If Not Char.IsNumber(letter) Then
						has_illegal_chars = True
						Exit For
					End If
				Next
			End If

			If has_illegal_chars Then
				MessageBox.Show("Red must be a number value between 0 and 255")
				txtRed.Text = m_rgb.R.ToString()
				Return
			End If

			Dim red As Integer = Integer.Parse(text)

			If red < 0 OrElse red > 255 Then
				MessageBox.Show("An integer between 0 and 255 is required." & vbLf & "Closest value inserted.")
				red = If(red < 0, 0, 255)
			End If

			m_rgb = Color.FromArgb(red, m_rgb.G, m_rgb.B)
			UpdateUI(m_rgb)
		End Sub


		Private Sub txtGreen_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = txtGreen.Text
			Dim has_illegal_chars As Boolean = False

			If text.Length < 1 OrElse text.Length > 3 Then
				has_illegal_chars = True
			Else
				For Each letter As Char In text
					If Not Char.IsNumber(letter) Then
						has_illegal_chars = True
						Exit For
					End If
				Next
			End If

			If has_illegal_chars Then
				MessageBox.Show("Green must be a number value between 0 and 255")
				txtGreen.Text = m_rgb.G.ToString()
				Return
			End If

			Dim green As Integer = Integer.Parse(text)

			If green < 0 OrElse green > 255 Then
				MessageBox.Show("An integer between 0 and 255 is required." & vbLf & "Closest value inserted.")
				green = If(green < 0, 0, 255)
			End If

			m_rgb = Color.FromArgb(m_rgb.R, green, m_rgb.B)
			UpdateUI(m_rgb)
		End Sub


		Private Sub txtBlue_Leave(sender As Object, e As System.EventArgs)
			Dim text As String = txtBlue.Text
			Dim has_illegal_chars As Boolean = False

			If text.Length < 1 OrElse text.Length > 3 Then
				has_illegal_chars = True
			Else
				For Each letter As Char In text
					If Not Char.IsNumber(letter) Then
						has_illegal_chars = True
						Exit For
					End If
				Next
			End If

			If has_illegal_chars Then
				MessageBox.Show("Blue must be a number value between 0 and 255")
				txtBlue.Text = m_rgb.B.ToString()
				Return
			End If

			Dim blue As Integer = Integer.Parse(text)

			If blue < 0 OrElse blue > 255 Then
				MessageBox.Show("An integer between 0 and 255 is required." & vbLf & "Closest value inserted.")
				blue = If(blue < 0, 0, 255)
			End If

			m_rgb = Color.FromArgb(m_rgb.R, m_rgb.G, blue)
			UpdateUI(m_rgb)
		End Sub


		'private void m_txt_Cyan_Leave(object sender, System.EventArgs e)
		'{
		'    string text = "";// m_txt_Cyan.Text;
		'    bool has_illegal_chars = false;

		'    if (text.Length <= 0)
		'        has_illegal_chars = true;
		'    else
		'        foreach (char letter in text)
		'        {
		'            if (!char.IsNumber(letter))
		'            {
		'                has_illegal_chars = true;
		'                break;
		'            }
		'        }

		'    if (has_illegal_chars)
		'    {
		'        MessageBox.Show("Cyan must be a number value between 0 and 100");
		'        UpdateTextBoxes();
		'        return;
		'    }

		'    int cyan = int.Parse(text);

		'    if (cyan < 0)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_cmyk.C = 0.0;
		'    }
		'    else if (cyan > 100)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_cmyk.C = 1.0;
		'    }
		'    else
		'    {
		'        m_cmyk.C = (double)cyan / 100;
		'    }

		'    m_rgb = ColorManager.CMYK_to_RGB(m_cmyk);
		'    m_hsl = ColorManager.RGB_to_HSL(m_rgb);
		'    colorBox.HSL = m_hsl;
		'    colorSlider.HSL = m_hsl;
		'    m_lbl_Primary_Color.BackColor = m_rgb;

		'    UpdateTextBoxes();
		'}
		'private void m_txt_Magenta_Leave(object sender, System.EventArgs e)
		'{
		'    string text = "";// m_txt_Magenta.Text;
		'    bool has_illegal_chars = false;

		'    if (text.Length <= 0)
		'        has_illegal_chars = true;
		'    else
		'        foreach (char letter in text)
		'        {
		'            if (!char.IsNumber(letter))
		'            {
		'                has_illegal_chars = true;
		'                break;
		'            }
		'        }

		'    if (has_illegal_chars)
		'    {
		'        MessageBox.Show("Magenta must be a number value between 0 and 100");
		'        UpdateTextBoxes();
		'        return;
		'    }

		'    int magenta = int.Parse(text);

		'    if (magenta < 0)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_txt_Magenta.Text = "0";
		'        m_cmyk.M = 0.0;
		'    }
		'    else if (magenta > 100)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_txt_Magenta.Text = "100";
		'        m_cmyk.M = 1.0;
		'    }
		'    else
		'    {
		'        m_cmyk.M = (double)magenta / 100;
		'    }

		'    m_rgb = ColorManager.CMYK_to_RGB(m_cmyk);
		'    m_hsl = ColorManager.RGB_to_HSL(m_rgb);
		'    colorBox.HSL = m_hsl;
		'    colorSlider.HSL = m_hsl;
		'    m_lbl_Primary_Color.BackColor = m_rgb;

		'    UpdateTextBoxes();
		'}
		'private void m_txt_Yellow_Leave(object sender, System.EventArgs e)
		'{
		'    string text = m_txt_Yellow.Text;
		'    bool has_illegal_chars = false;

		'    if (text.Length <= 0)
		'        has_illegal_chars = true;
		'    else
		'        foreach (char letter in text)
		'        {
		'            if (!char.IsNumber(letter))
		'            {
		'                has_illegal_chars = true;
		'                break;
		'            }
		'        }

		'    if (has_illegal_chars)
		'    {
		'        MessageBox.Show("Yellow must be a number value between 0 and 100");
		'        UpdateTextBoxes();
		'        return;
		'    }

		'    int yellow = int.Parse(text);

		'    if (yellow < 0)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_txt_Yellow.Text = "0";
		'        m_cmyk.Y = 0.0;
		'    }
		'    else if (yellow > 100)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_txt_Yellow.Text = "100";
		'        m_cmyk.Y = 1.0;
		'    }
		'    else
		'    {
		'        m_cmyk.Y = (double)yellow / 100;
		'    }

		'    m_rgb = ColorManager.CMYK_to_RGB(m_cmyk);
		'    m_hsl = ColorManager.RGB_to_HSL(m_rgb);
		'    colorBox.HSL = m_hsl;
		'    colorSlider.HSL = m_hsl;
		'    m_lbl_Primary_Color.BackColor = m_rgb;

		'    UpdateTextBoxes();
		'}
		'private void m_txt_K_Leave(object sender, System.EventArgs e)
		'{
		'    string text = m_txt_K.Text;
		'    bool has_illegal_chars = false;

		'    if (text.Length <= 0)
		'        has_illegal_chars = true;
		'    else
		'        foreach (char letter in text)
		'        {
		'            if (!char.IsNumber(letter))
		'            {
		'                has_illegal_chars = true;
		'                break;
		'            }
		'        }

		'    if (has_illegal_chars)
		'    {
		'        MessageBox.Show("Key must be a number value between 0 and 100");
		'        UpdateTextBoxes();
		'        return;
		'    }

		'    int key = int.Parse(text);

		'    if (key < 0)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_txt_K.Text = "0";
		'        m_cmyk.K = 0.0;
		'    }
		'    else if (key > 100)
		'    {
		'        MessageBox.Show("An integer between 0 and 100 is required.\nClosest value inserted.");
		'        m_txt_K.Text = "100";
		'        m_cmyk.K = 1.0;
		'    }
		'    else
		'    {
		'        m_cmyk.K = (double)key / 100;
		'    }

		'    m_rgb = ColorManager.CMYK_to_RGB(m_cmyk);
		'    m_hsl = ColorManager.RGB_to_HSL(m_rgb);
		'    colorBox.HSL = m_hsl;
		'    colorSlider.HSL = m_hsl;
		'    m_lbl_Primary_Color.BackColor = m_rgb;

		'    UpdateTextBoxes();
		'}


		#End Region

		#End Region

		#Region "Private Functions"

		Private Function Round(val As Double) As Integer
			Dim ret_val As Integer = CInt(Math.Truncate(val))

			Dim temp As Integer = CInt(Math.Truncate(val * 100))

			If (temp Mod 100) >= 50 Then
				ret_val += 1
			End If

			Return ret_val
		End Function


		Private Sub WriteHexData(rgb As Color)
			'string red = Convert.ToString(rgb.R, 16);
			'if (red.Length < 2) red = "0" + red;
			'string green = Convert.ToString(rgb.G, 16);
			'if (green.Length < 2) green = "0" + green;
			'string blue = Convert.ToString(rgb.B, 16);
			'if (blue.Length < 2) blue = "0" + blue;

			'm_txt_Hex.Text = red.ToUpper() + green.ToUpper() + blue.ToUpper();
			'm_txt_Hex.Update();
		End Sub


		Private Function ParseHexData(hex_data As String) As Color
			If hex_data.Length <> 6 Then
				Return Color.Black
			End If

			Dim r_text As String, g_text As String, b_text As String
			Dim r As Integer, g As Integer, b As Integer

			r_text = hex_data.Substring(0, 2)
			g_text = hex_data.Substring(2, 2)
			b_text = hex_data.Substring(4, 2)

			r = Integer.Parse(r_text, System.Globalization.NumberStyles.HexNumber)
			g = Integer.Parse(g_text, System.Globalization.NumberStyles.HexNumber)
			b = Integer.Parse(b_text, System.Globalization.NumberStyles.HexNumber)

			Return Color.FromArgb(r, g, b)
		End Function


		'private void UpdateTextBoxes()
		'{
		'    txtHue.Text = Round(m_hsl.H * 360).ToString();
		'    txtSat.Text = Round(m_hsl.S * 100).ToString();
		'    txtBrightness.Text = Round(m_hsl.L * 100).ToString();
		'    //m_txt_Cyan.Text = Round(m_cmyk.C * 100).ToString();
		'    //m_txt_Magenta.Text = Round(m_cmyk.M * 100).ToString();
		'    //m_txt_Yellow.Text = Round(m_cmyk.Y * 100).ToString();
		'    //m_txt_K.Text = Round(m_cmyk.K * 100).ToString();
		'    txtRed.Text = m_rgb.R.ToString();
		'    txtGreen.Text = m_rgb.G.ToString();
		'    txtBlue.Text = m_rgb.B.ToString();

		'    //m_txt_Red.Update();
		'    //m_txt_Green.Update();
		'    //m_txt_Blue.Update();
		'    //m_txt_Hue.Update();
		'    //m_txt_Sat.Update();
		'    //m_txt_Brightness.Update();
		'    //m_txt_Cyan.Update();
		'    //m_txt_Magenta.Update();
		'    //m_txt_Yellow.Update();
		'    //m_txt_K.Update();

		'    WriteHexData(m_rgb);
		'}


		#End Region

		#Region "Public Methods"

		Public Property PrimaryColor() As Color
			Get
				Return m_rgb
			End Get
			Set
				UpdateUI(value)
			End Set
		End Property


		Public Property DrawStyle() As eDrawStyle
			Get
				If rbHue.Checked Then
					Return eDrawStyle.Hue
				ElseIf rbSat.Checked Then
					Return eDrawStyle.Saturation
				ElseIf rbBrightness.Checked Then
					Return eDrawStyle.Brightness
				ElseIf rbRed.Checked Then
					Return eDrawStyle.Red
				ElseIf rbGreen.Checked Then
					Return eDrawStyle.Green
				ElseIf rbBlue.Checked Then
					Return eDrawStyle.Blue
				Else
					Return eDrawStyle.Hue
				End If
			End Get
			Set
				Select Case value
					Case eDrawStyle.Hue
						rbHue.Checked = True
						Exit Select
					Case eDrawStyle.Saturation
						rbSat.Checked = True
						Exit Select
					Case eDrawStyle.Brightness
						rbBrightness.Checked = True
						Exit Select
					Case eDrawStyle.Red
						rbRed.Checked = True
						Exit Select
					Case eDrawStyle.Green
						rbGreen.Checked = True
						Exit Select
					Case eDrawStyle.Blue
						rbBlue.Checked = True
						Exit Select
					Case Else
						rbHue.Checked = True
						Exit Select
				End Select
			End Set
		End Property


		#End Region

		Private Sub tbAlpha_ValueChanged(sender As Object, e As EventArgs)
			colorPanelPending.Color = Color.FromArgb(255 - tbAlpha.Value, m_rgb)
		End Sub
	End Class
End Namespace
