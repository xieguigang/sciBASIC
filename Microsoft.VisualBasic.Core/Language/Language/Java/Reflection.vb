#Region "Microsoft.VisualBasic::20be5e5db69eaa44a3037b9727dc487a, Microsoft.VisualBasic.Core\Language\Language\Java\Reflection.vb"

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

    '     Module Reflection
    ' 
    '         Function: GetConstructor, NewInstance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace Language.Java

    Public Module Reflection

        ''' <summary>
        ''' Gets the default class constructor
        ''' 
        ''' ```vbnet
        ''' Dim o As New &lt;<paramref name="type"/>>()
        ''' ```
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetConstructor(type As Type) As ConstructorInfo
            Return type.GetConstructor(
                BindingFlags.Public Or BindingFlags.Instance,
                Nothing, {}, Nothing)
        End Function

        <Extension>
        Public Function NewInstance(ctr As ConstructorInfo) As Object
            Return ctr.Invoke(BindingFlags.Public Or BindingFlags.Instance, Nothing, {}, CultureInfo.CurrentCulture)
        End Function
    End Module
End Namespace
