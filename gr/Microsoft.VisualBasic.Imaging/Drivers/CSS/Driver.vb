#Region "Microsoft.VisualBasic::7198ae6f70bd1c6b353198bf905ab13f, gr\Microsoft.VisualBasic.Imaging\Drivers\CSS\Driver.vb"

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

    '     Class Driver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class CSSSelector
    ' 
    '         Properties: IsGlobal, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class GlobalCSSSelector
    ' 
    '         Properties: IsGlobal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Enum Types
    ' 
    '         [Integer], Brush, Float, Font, Padding
    '         Size, Stroke
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Driver.CSS

    Public Class Driver : Inherits ExportAPIAttribute

        Sub New(name$)
            Call MyBase.New(name)
        End Sub
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class CSSSelector : Inherits Attribute

        Public ReadOnly Property Type As Types
        Public Overridable ReadOnly Property IsGlobal As Boolean
            Get
                Return False
            End Get
        End Property

        Sub New(type As Types)
            Me.Type = type
        End Sub

        Public Overrides Function ToString() As String
            Return Type.ToString
        End Function
    End Class

    Public Class GlobalCSSSelector : Inherits CSSSelector

        Public Overrides ReadOnly Property IsGlobal As Boolean
            Get
                Return True
            End Get
        End Property

        Sub New(type As Types)
            Call MyBase.New(type)
        End Sub

        Public Overrides Function ToString() As String
            Return MyBase.ToString() & " @canvas"
        End Function
    End Class

    Public Enum Types

        ''' <summary>
        ''' <see cref="CSSFont"/>
        ''' </summary>
        Font
        ''' <summary>
        ''' <see cref="MIME.Markup.HTML.CSS.Stroke"/>
        ''' </summary>
        Stroke
        ''' <summary>
        ''' <see cref="Fill"/>
        ''' </summary>
        Brush
        ''' <summary>
        ''' <see cref="CSSsize"/>
        ''' </summary>
        Size
        ''' <summary>
        ''' <see cref="MIME.Markup.HTML.CSS.Padding"/>
        ''' </summary>
        Padding
        ''' <summary>
        ''' line width, box width, etc, almost the same as <see cref="Size"/>
        ''' </summary>
        [Integer]
        ''' <summary>
        ''' Circle Radius(examples as node size in ``d3.js``)
        ''' </summary>
        Float
    End Enum
End Namespace
