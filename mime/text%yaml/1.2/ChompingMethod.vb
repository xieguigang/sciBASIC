#Region "Microsoft.VisualBasic::bedcc4bc233daac4c1f0f8f6ed56313c, mime\text%yaml\1.2\ChompingMethod.vb"

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

    '     Enum ChompingMethod
    ' 
    '         Clip, Keep, Strip
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace Grammar

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
