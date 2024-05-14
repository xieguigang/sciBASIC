#Region "Microsoft.VisualBasic::5499279945bfc4639c1313a474eebe5c, Microsoft.VisualBasic.Core\src\Serialization\JSON\Formatter\Strategies\SingleQuoteStrategy.vb"

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

    '   Total Lines: 18
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 805 B


    '     Class SingleQuoteStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class SingleQuoteStrategy
        Implements ICharacterStrategy
        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If Not context.IsProcessingDoubleQuoteInitiatedString AndAlso Not context.WasLastCharacterABackSlash Then
                context.IsProcessingSingleQuoteInitiatedString = Not context.IsProcessingSingleQuoteInitiatedString
            End If

            context.AppendCurrentChar()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return "'"c
            End Get
        End Property
    End Class
End Namespace
