library(feather)
library(hms)

# Notes:
# 1. When appropriate, I generally went with "slightly negative, passing zero,
# going to positive
# 2. POSIXct is a double representing fractional seconds since epoch
# 3. Categories have orders, this one has B, C, A
# 4. Feather works with time as the "hms" (hour, minute, second) class.
# 5. stringsAsFactors = FALSE prevents a very stupid built-in behavior, ignore it.

d <- data.frame(Boolean = c(TRUE, TRUE, TRUE, FALSE, FALSE),
                Integer = seq.int(-1, 3),
                Double = seq(-1.5, 2.5, 1),
                Category = factor(c("A", "A", "B", "B", "C"), levels = c("B", "C", "A")),
                Timestamp = as.POSIXct("1970-01-01 00:00:00", tz = "UTC") + seq(-1, 3),
                Time = hms(hours = 0:4, minutes = 0:4, seconds = 0:4),
                Date = as.Date("1969-12-30") + 1:5,
                String = c("A", "", "aaaaaaaa", "bbbbbbbbbbbbb", "CC"),
                stringsAsFactors = FALSE)

write_feather(d, "r-feather-test.feather")

# nullables: make both the third and the last item NA in each
d_nullable <- d
d_nullable[c(3, 5), ] <- NA
write_feather(d_nullable, "r-feather-test-nullable.feather")