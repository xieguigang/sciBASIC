Imports System.Collections.Generic
Imports System.Text

Namespace YAML.Grammar
    ''' <summary>
    ''' How line breaks after last non empty line in block scalar are handled.
    ''' </summary>
    Public Enum ChompingMethod
        ''' <summary>
        ''' Keep one
        ''' </summary>
        Clip

        ''' <summary>
        ''' Keep none
        ''' </summary>
        Strip

        ''' <summary>
        ''' Keep all
        ''' </summary>
        Keep
    End Enum
End Namespace
