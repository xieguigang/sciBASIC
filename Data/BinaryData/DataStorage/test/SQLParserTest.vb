Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema

Module SQLParserTest

    Sub Main()
        Call stringtest()
        Call test2()
    End Sub

    Sub test1()
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

    Sub stringtest()
        Dim sql = "'<?xml version=""1.0"" encoding=""utf-16""?>
                      <ArrayOfDouble xmlns:xsd=""http//www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                          <double>0</double>
                                  <double>0</double>
                      </ArrayOfDouble>'"

        Dim tokens = New SQLParser(sql).GetTokens.ToArray

        Pause()
    End Sub

    Sub test2()
        Dim sql = <SQL>
                      CREATE TABLE "RetentionTimeRasterItem"
					(
					"WorkflowID" int NOT NULL, 
						"ID" int NOT NULL, 
						"MSOrder" int NOT NULL DEFAULT 0, 
						"Polarity" int NOT NULL DEFAULT 0, 
						"IonizationSource" int NOT NULL DEFAULT 0, 
						"MassAnalyzer" int NOT NULL DEFAULT 0, 
						"MassRange" [varchar] (1024) COLLATE NOCASE NULL DEFAULT '&lt;?xml version="1.0" encoding="utf-16"?>
                      &lt;ArrayOfDouble xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                          &lt;double>0&lt;/double>
                                  &lt;double>0&lt;/double>
                      &lt;/ArrayOfDouble>', 
						"ResolutionAtMass200" int NULL, 
						"ScanRate" int NOT NULL DEFAULT 0, 
						"ScanType" int NOT NULL DEFAULT 0, 
						"ActivationTypes" int NOT NULL DEFAULT 0, 
						"ActivationEnergies" [varchar] (1024) COLLATE NOCASE NULL, 
						"IsolationWindow" [varchar] (1024) COLLATE NOCASE NULL DEFAULT '&lt;?xml version="1.0" encoding="utf-16"?>
                      &lt;ArrayOfDouble xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                          &lt;double>0&lt;/double>
                                          &lt;double>0&lt;/double>
                      &lt;/ArrayOfDouble>', 
						"IsolationMass" double NULL, 
						"IsolationWidth" double NULL, 
						"IsolationOffset" double NULL, 
						"IsMultiplexed" int NULL, 
						"FileID" int NOT NULL DEFAULT 0, 
						"Trace" BLOB NULL
					)

                  </SQL>

        Dim sqltext = sql.Value
        Dim tokens = New SQLParser(sqltext).GetTokens.ToArray

        Pause()
    End Sub
End Module
