```R
fit<-lm(weight~height,data=women);
summary(fit);

# Call:
# lm(formula = weight ~ height, data = women)
# 
# Residuals:
#     Min      1Q  Median      3Q     Max 
# -1.7333 -1.1333 -0.3833  0.7417  3.1167 
# 
# Coefficients:
#              Estimate Std. Error t value Pr(>|t|)    
# (Intercept) -87.51667    5.93694  -14.74 1.71e-09 ***
# height        3.45000    0.09114   37.85 1.09e-14 ***
# ---
# Signif. codes:  0 ‘***’ 0.001 ‘**’ 0.01 ‘*’ 0.05 ‘.’ 0.1 ‘ ’ 1
# 
# Residual standard error: 1.525 on 13 degrees of freedom
# Multiple R-squared:  0.991,     Adjusted R-squared:  0.9903 
# F-statistic:  1433 on 1 and 13 DF,  p-value: 1.091e-14

fit2<-lm(weight~height+I(height^2),data=women);
summary(fit2);

# Call:
# lm(formula = weight ~ height + I(height^2), data = women)
# 
# Residuals:
#      Min       1Q   Median       3Q      Max 
# -0.50941 -0.29611 -0.00941  0.28615  0.59706 
# 
# Coefficients:
#              Estimate Std. Error t value Pr(>|t|)    
# (Intercept) 261.87818   25.19677  10.393 2.36e-07 ***
# height       -7.34832    0.77769  -9.449 6.58e-07 ***
# I(height^2)   0.08306    0.00598  13.891 9.32e-09 ***
# ---
# Signif. codes:  0 ‘***’ 0.001 ‘**’ 0.01 ‘*’ 0.05 ‘.’ 0.1 ‘ ’ 1
# 
# Residual standard error: 0.3841 on 12 degrees of freedom
# Multiple R-squared:  0.9995,    Adjusted R-squared:  0.9994 
# F-statistic: 1.139e+04 on 2 and 12 DF,  p-value: < 2.2e-16
```
