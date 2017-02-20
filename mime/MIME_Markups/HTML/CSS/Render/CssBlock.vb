Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports Microsoft.VisualBasic.MIME.Markup.HTML.Render

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents a block of CSS property values
    ''' </summary>
    ''' <remarks>
    ''' To learn more about CSS blocks visit CSS spec:
    ''' http://www.w3.org/TR/CSS21/syndata.html#block
    ''' </remarks>
    Public Class CssBlock
#Region "Fields"
        Private _block As String
        Private _propertyValues As Dictionary(Of PropertyInfo, String)
        Private _properties As Dictionary(Of String, String)

#End Region

#Region "Ctor"

        ''' <summary>
        ''' Initializes internal's
        ''' </summary>
        Private Sub New()
            _propertyValues = New Dictionary(Of PropertyInfo, String)()
            _properties = New Dictionary(Of String, String)()
        End Sub

        ''' <summary>
        ''' Creates a new block from the block's source
        ''' </summary>
        ''' <param name="blockSource"></param>
        Public Sub New(blockSource As String)
            Me.New()
            _block = blockSource

            'Extract property assignments
            Dim matches As MatchCollection = Parser.Match(Parser.CssProperties, blockSource)

            'Scan matches
            For Each match As Match In matches
                'Split match by colon
                Dim chunks As String() = match.Value.Split(":"c)

                If chunks.Length <> 2 Then
                    Continue For
                End If

                'Extract property name and value
                Dim propName As String = chunks(0).Trim()
                Dim propValue As String = chunks(1).Trim()

                'Remove semicolon
                If propValue.EndsWith(";") Then
                    propValue = propValue.Substring(0, propValue.Length - 1).Trim()
                End If

                'Add property to list
                Properties.Add(propName, propValue)

                'Register only if property checks with reflection
                If CssBox._properties.ContainsKey(propName) Then
                    PropertyValues.Add(CssBox._properties(propName), propValue)
                End If
            Next
        End Sub

#End Region

#Region "Props"

        ''' <summary>
        ''' Gets the properties and its values
        ''' </summary>
        Public ReadOnly Property Properties() As Dictionary(Of String, String)
            Get
                Return _properties
            End Get
        End Property

        ''' <summary>
        ''' Gets the dictionary with property-ready values
        ''' </summary>
        Public ReadOnly Property PropertyValues() As Dictionary(Of PropertyInfo, String)
            Get
                Return _propertyValues
            End Get
        End Property


        ''' <summary>
        ''' Gets the block's source
        ''' </summary>
        Public ReadOnly Property BlockSource() As String
            Get
                Return _block
            End Get
        End Property


#End Region

#Region "Method"

        ''' <summary>
        ''' Updates the PropertyValues dictionary
        ''' </summary>
        Friend Sub UpdatePropertyValues()
            PropertyValues.Clear()

            For Each prop As String In Properties.Keys
                If CssBox._properties.ContainsKey(prop) Then
                    PropertyValues.Add(CssBox._properties(prop), Properties(prop))
                End If
            Next
        End Sub

        ''' <summary>
        ''' Asigns the style on this block o the specified box
        ''' </summary>
        ''' <param name="b"></param>
        Public Sub AssignTo(b As CssBox)
            For Each prop As PropertyInfo In PropertyValues.Keys
                Dim value As String = PropertyValues(prop)

                If value = CssConstants.Inherit AndAlso b.ParentBox IsNot Nothing Then
                    value = Convert.ToString(prop.GetValue(b.ParentBox, Nothing))
                End If

                prop.SetValue(b, value, Nothing)
            Next
        End Sub

#End Region
    End Class
End Namespace