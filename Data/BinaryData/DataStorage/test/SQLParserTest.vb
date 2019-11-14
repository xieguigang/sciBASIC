Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema

Module SQLParserTest

    Sub Main()
        Dim sql = "CREATE TABLE [ProcessingNodeScores] (
						[WorkflowID] [int] NOT NULL,
						[Level] [int] NOT NULL,
						[ScoreID] [int] NOT NULL,
						[ScoreName] [varchar] (50) COLLATE NOCASE NOT NULL ,		
						[DisplayName] [varchar](256) COLLATE NOCASE NOT NULL,
						[Description] [varchar](1024) COLLATE NOCASE NOT NULL,
						[FormatString] [varchar](64) COLLATE NOCASE NOT NULL,
						[ScoreCategory] [int] NOT NULL,
						[IsMainScore] [bit] NOT NULL,
						[ScoreGUID] [varchar] (36) COLLATE NOCASE NOT NULL,
						[DataType] [varchar] (256) COLLATE NOCASE NOT NULL ,		
						[SemanticTerms] [varchar],
						[NodeGuid] [varchar] (36) COLLATE NOCASE NOT NULL,
						[NodeName][varchar] (50) COLLATE NOCASE NOT NULL ,
						UNIQUE ( [WorkflowID], [NodeGuid], [ScoreName])
					)"

        Dim tokens = New SQLParser(sql).GetTokens.ToArray

        Pause()
    End Sub
End Module
