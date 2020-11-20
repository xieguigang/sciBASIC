woman <- list(
	weight = c(115, 117, 120, 123, 126, 129, 132, 135, 139, 142, 146, 150, 154, 159, 164),
    height = c(58.00, 59.00, 60.00, 61.25, 62.00, 63.50, 64.00, 65.00, 66.10, 67.10, 68.00, 69.00, 70.00, 71.80, 72.00)
)

fit<-lm(weight~height,data=women);
summary(fit);

fit2<-lm(weight~height+I(height^2),data=women);
summary(fit2);
