library(arules)
library(Matrix)

data(Groceries)

write.csv(as(Groceries,'data.frame'), "./Groceries.csv", row.names = FALSE, col.names = FALSE);


data("Adult")

write.csv(as(Adult,'data.frame'), "./Adult.csv", row.names = FALSE, col.names = FALSE);