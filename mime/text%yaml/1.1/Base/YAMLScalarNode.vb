#Region "Microsoft.VisualBasic::288272f623e0501f02cb2df7ca9d6e13, mime\text%yaml\1.1\Base\YAMLScalarNode.vb"

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


    ' Code Statistics:

    '   Total Lines: 261
    '    Code Lines: 214
    ' Comment Lines: 3
    '   Blank Lines: 44
    '     File Size: 8.40 KB


    '     Class YAMLScalarNode
    ' 
    '         Properties: Empty, IsIndent, IsMultyline, NodeType, Style
    '                     Value
    ' 
    '         Constructor: (+12 Overloads) Sub New
    '         Sub: Emit, (+11 Overloads) SetValue, UpdateStyle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'#define USE_HEX_FLOAT

Imports System.Globalization
Imports System.Text.RegularExpressions

Namespace Grammar11

    Public NotInheritable Class YAMLScalarNode
        Inherits YAMLNode

        Shared ReadOnly s_illegal As New Regex("[\:\[\]'""]")

        Private m_objectType As ScalarType = ScalarType.[String]
        Private m_string As String = String.Empty
        Private m_value As Long = 0
        Private m_double As Double = 0.0

        Public Sub New()
        End Sub

        Public Sub New(value As Boolean)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As Byte)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As Short)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As UShort)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As Integer)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As UInteger)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As Long)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As ULong)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As Single)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As Double)
            SetValue(value)
            Style = ScalarStyle.Plain
        End Sub

        Public Sub New(value As String)
            SetValue(value)
            UpdateStyle()
        End Sub

        Public Sub SetValue(value As Boolean)
            m_value = If(value, 1, 0)
            m_objectType = ScalarType.[Boolean]
        End Sub

        Public Sub SetValue(value As Byte)
            m_value = value
            m_objectType = ScalarType.[Byte]
        End Sub

        Public Sub SetValue(value As Short)
            m_value = value
            m_objectType = ScalarType.Int16
        End Sub

        Public Sub SetValue(value As UShort)
            m_value = value
            m_objectType = ScalarType.UInt16
        End Sub

        Public Sub SetValue(value As Integer)
            m_value = value
            m_objectType = ScalarType.Int32
        End Sub

        Public Sub SetValue(value As UInteger)
            m_value = value
            m_objectType = ScalarType.UInt32
        End Sub

        Public Sub SetValue(value As Long)
            m_value = value
            m_objectType = ScalarType.Int64
        End Sub

        Public Sub SetValue(value As ULong)
            m_value = CLng(value)
            m_objectType = ScalarType.UInt64
        End Sub

        Public Sub SetValue(value As Single)
#If USE_HEX_FLOAT Then
			' It is more precise technic but output looks vague and less readable
			Dim hex As UInteger = BitConverterExtensions.ToUInt32(value)
			m_string = "0x{hex.ToHexString()}({value.ToString(CultureInfo.InvariantCulture)})"
			m_objectType = ScalarType.[String]
#Else
            m_double = value
            m_objectType = ScalarType.[Single]
#End If
        End Sub

        Public Sub SetValue(value As Double)
#If USE_HEX_FLOAT Then
			' It is more precise technic but output looks vague and less readable
			Dim hex As ULong = BitConverterExtensions.ToUInt64(value)
			m_string = "0x{hex.ToHexString()}({value.ToString(CultureInfo.InvariantCulture)})"
			m_objectType = ScalarType.[String]
#Else
            m_double = value
            m_objectType = ScalarType.[Double]
#End If
        End Sub

        Public Sub SetValue(value As String)
            m_string = value
            m_objectType = ScalarType.[String]
        End Sub

        Friend Overrides Sub Emit(emitter As Emitter)
            MyBase.Emit(emitter)

            Select Case Style
                Case ScalarStyle.Hex, ScalarStyle.Plain
                    emitter.Write(Value)
                    Exit Select

                Case ScalarStyle.SingleQuoted
                    emitter.Write("'"c)
                    emitter.Write(Value)
                    emitter.Write("'"c)
                    Exit Select

                Case ScalarStyle.DoubleQuoted
                    emitter.Write(""""c)
                    emitter.Write(Value)
                    emitter.Write(""""c)
                    Exit Select
                Case Else

                    Throw New Exception("Unsupported scalar style {Style}")
            End Select
        End Sub

        Private Sub UpdateStyle()
            Dim value As String = Me.Value
            If s_illegal.IsMatch(value) Then
                If value.Contains("'") Then
                    If value.Contains("""") Then
                        value = value.Replace("'", "''")
                        SetValue(value)
                        Style = ScalarStyle.SingleQuoted
                    Else
                        Style = ScalarStyle.DoubleQuoted
                    End If
                Else
                    Style = ScalarStyle.SingleQuoted
                End If
            Else
                Style = ScalarStyle.Plain
            End If
        End Sub

        Public Shared ReadOnly Property Empty As New YAMLScalarNode()

        Public Overrides ReadOnly Property NodeType() As YAMLNodeType
            Get
                Return YAMLNodeType.Scalar
            End Get
        End Property

        Public Overrides ReadOnly Property IsMultyline() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property IsIndent() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Property Value() As String
            Get
                If Style = ScalarStyle.Hex Then
                    Select Case m_objectType
                        Case ScalarType.[Byte]
                            Return CByte(m_value).ToHexString()
                        Case ScalarType.Int16
                            Return CShort(m_value).ToHexString()
                        Case ScalarType.UInt16
                            Return CUShort(m_value).ToHexString()
                        Case ScalarType.Int32
                            Return CInt(m_value).ToHexString()
                        Case ScalarType.UInt32
                            Return CUInt(m_value).ToHexString()
                        Case ScalarType.Int64
                            Return m_value.ToHexString()
                        Case ScalarType.UInt64

                            Return CULng(m_value).ToHexString()

                        Case ScalarType.[Single]
                            Return CSng(m_double).ToHexString()
                        Case ScalarType.[Double]
                            Return m_double.ToHexString()
                        Case ScalarType.[String]
                            Return m_string
                        Case Else
                            Throw New NotImplementedException(m_objectType.ToString())
                    End Select
                End If

                Select Case m_objectType
                    Case ScalarType.[Boolean]
                        Return If(m_value = 0, 0.ToString(), 1.ToString())
                    Case ScalarType.[Single]
                        Return CSng(m_double).ToString(CultureInfo.InvariantCulture)
                    Case ScalarType.[Double]
                        Return m_double.ToString(CultureInfo.InvariantCulture)
                    Case ScalarType.[String]
                        Return m_string
                    Case Else

                        Return m_value.ToString()
                End Select
            End Get
            Set
                m_string = Value
            End Set
        End Property

        Public Property Style() As ScalarStyle
    End Class
End Namespace
