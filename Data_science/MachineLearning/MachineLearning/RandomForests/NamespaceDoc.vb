Namespace RandomForests

    ''' <summary>
    ''' # Random Forests
    ''' 
    ''' ## Overview
    ''' 
    ''' We assume that the user knows about the construction Of Single classification trees. Random Forests 
    ''' grows many classification trees. To classify a New Object from an input vector, put the input vector 
    ''' down Each Of the trees In the forest. Each tree gives a classification, And we say the tree "votes" 
    ''' For that Class. The forest chooses the classification having the most votes (over all the trees In 
    ''' the forest).
    ''' 
    ''' Each tree Is grown as follows
    ''' 
    ''' + If the number Of cases In the training Set Is N, sample N cases at random - but With replacement, 
    '''   from the original data. This sample will be the training Set For growing the tree.
    ''' + If there are M input variables, a number ``m&lt;&lt;M`` Is specified such that at Each node, m 
    '''   variables are selected at random out Of the M And the best split On these m Is used To split the node. 
    '''   The value Of m Is held constant during the forest growing.
    ''' + Each tree Is grown to the largest extent possible. There Is no pruning.
    ''' 
    ''' In the original paper on random forests, it was shown that the forest error rate depends on two things:
    ''' 
    ''' + The correlation between any two trees In the forest. Increasing the correlation increases the forest 
    '''   Error rate.
    ''' + The strength Of Each individual tree In the forest. A tree With a low Error rate Is a strong classifier. 
    '''   Increasing the strength Of the individual trees decreases the forest Error rate.
    ''' 
    ''' Reducing m reduces both the correlation And the strength. Increasing it increases both. Somewhere In 
    ''' between Is an "optimal" range Of m - usually quite wide. Using the oob Error rate (see below) a value 
    ''' Of m In the range can quickly be found. This Is the only adjustable parameter To which random forests 
    ''' Is somewhat sensitive.
    ''' 
    ''' ## Features of Random Forests
    ''' 
    ''' + It Is unexcelled in accuracy among current algorithms.
    ''' + It runs efficiently On large data bases.
    ''' + It can handle thousands Of input variables without variable deletion.
    ''' + It gives estimates Of what variables are important In the classification.
    ''' + It generates an internal unbiased estimate Of the generalization Error As the forest building progresses.
    ''' + It has an effective method For estimating missing data And maintains accuracy When a large proportion Of the data are missing.
    ''' + It has methods For balancing Error In Class population unbalanced data sets.
    ''' + Generated forests can be saved For future use On other data.
    ''' + Prototypes are computed that give information about the relation between the variables And the classification.
    ''' + It computes proximities between pairs Of cases that can be used In clustering, locating outliers, Or (by scaling) give interesting views Of the data.
    ''' + The capabilities Of the above can be extended To unlabeled data, leading To unsupervised clustering, data views And outlier detection.
    ''' + It offers an experimental method For detecting variable interactions.
    ''' 
    ''' ## Remarks
    ''' 
    ''' Random forests does Not overfit. You can run As many trees As you want. It Is fast. Running On a data 
    ''' Set With 50,000 cases And 100 variables, it produced 100 trees In 11 minutes On a 800Mhz machine. 
    ''' For large data sets the major memory requirement Is the storage Of the data itself, And three Integer 
    ''' arrays With the same dimensions As the data. If proximities are calculated, storage requirements grow 
    ''' As the number Of cases times the number Of trees.
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace