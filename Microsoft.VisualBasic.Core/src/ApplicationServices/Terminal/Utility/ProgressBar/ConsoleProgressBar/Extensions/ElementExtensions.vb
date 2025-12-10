#Region "Microsoft.VisualBasic::57656ba5697c23ecd914aeb46d1da95a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Extensions\ElementExtensions.vb"

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

    '   Total Lines: 71
    '    Code Lines: 32 (45.07%)
    ' Comment Lines: 24 (33.80%)
    '    - Xml Docs: 70.83%
    ' 
    '   Blank Lines: 15 (21.13%)
    '     File Size: 3.29 KB


    '     Module ElementExtensions
    ' 
    '         Function: (+2 Overloads) GetRenderActions
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar.Extensions
    ''' <summary>
    ''' Extensions for Element
    ''' </summary>
    Public Module ElementExtensions
        ''' <summary>
        ''' Returns a list of Actions to write the element in Console
        ''' </summary>
        ''' <param name="element"></param>
        ''' <param name="progressBar"></param>
        ''' <param name="valueTransformer">Function to Transform the value before write</param>
        ''' <returns></returns>
        <Extension()>
        Public Function GetRenderActions(element As Element(Of String), progressBar As ProgressBar, Optional valueTransformer As Func(Of String, String) = Nothing) As List(Of Action)
            Dim list = New List(Of Action)()

            If progressBar Is Nothing OrElse element Is Nothing OrElse Not element.GetVisible(progressBar) Then Return list

            Dim foregroundColor = element.GetForegroundColor(progressBar)
            If foregroundColor.HasValue Then list.Add(Sub() Console.ForegroundColor = foregroundColor.Value)

            Dim backgroundColor = element.GetBackgroundColor(progressBar)
            If backgroundColor.HasValue Then list.Add(Sub() Console.BackgroundColor = backgroundColor.Value)

            Dim value = element.GetValue(progressBar)
            If valueTransformer IsNot Nothing Then value = valueTransformer.Invoke(value)

            list.Add(Sub() Console.Write(value))
            list.Add(Sub() Console.ResetColor())

            Return list
        End Function

        ''' <summary>
        ''' Returns a list of Actions to write the element in Console
        ''' </summary>
        ''' <param name="element"></param>
        ''' <param name="progressBar"></param>
        ''' <param name="repetition"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function GetRenderActions(element As Element(Of Char), progressBar As ProgressBar, Optional repetition As Integer = 1) As List(Of Action)
            Dim list = New List(Of Action)()

            If progressBar Is Nothing OrElse repetition < 1 OrElse element Is Nothing OrElse Not element.GetVisible(progressBar) Then Return list

            Dim foregroundColor = element.GetForegroundColor(progressBar)
            If foregroundColor.HasValue Then list.Add(Sub() Console.ForegroundColor = foregroundColor.Value)

            Dim backgroundColor = element.GetBackgroundColor(progressBar)
            If backgroundColor.HasValue Then list.Add(Sub() Console.BackgroundColor = backgroundColor.Value)

            Dim value = element.GetValue(progressBar)
            list.Add(Sub() Console.Write(New String(value, repetition)))
            list.Add(Sub() Console.ResetColor())

            Return list
        End Function
    End Module

End Namespace
