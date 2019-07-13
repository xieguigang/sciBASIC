#Region "Microsoft.VisualBasic::024d215f06ecc1b4e0d2dc2c071b4650, Microsoft.VisualBasic.Core\Language\StringHelpers.vb"

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

    '     Module FormatHelpers
    ' 
    '         Function: StringFormat, xFormat
    ' 
    '     Structure FormatHelper
    ' 
    '         Function: ToString
    '         Operators: (+2 Overloads) <=, (+2 Overloads) >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.C

Namespace Language

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` syntax helpers
    ''' </summary>
    Public Module FormatHelpers

        ''' <summary>
        ''' ``<see cref="sprintf"/>`` extensions
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function xFormat(s As String) As FormatHelper
            Return New FormatHelper With {.source = s}
        End Function

        ''' <summary>
        ''' Synax like ``"formats" &lt;= {args}.xFormat`` 
        ''' Format by <see cref="sprintf"/>
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StringFormat(args As String()) As FormatHelper
            Return New FormatHelper With {.args = args}
        End Function
    End Module

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` reference
    ''' </summary>
    Public Structure FormatHelper

        Dim source$, args$()

        Public Overrides Function ToString() As String
            Return source
        End Function

        Public Shared Operator <=(pattern As String, format As FormatHelper) As String
            Return sprintf(pattern, format.args)
        End Operator

        Public Shared Operator >=(pattern As String, format As FormatHelper) As String
            Throw New NotSupportedException
        End Operator

        Public Shared Operator <=(format As FormatHelper, args As String()) As String
            Return sprintf(format.source, args)
        End Operator

        Public Shared Operator >=(format As FormatHelper, args As String()) As String
            Throw New NotSupportedException
        End Operator
    End Structure
End Namespace
