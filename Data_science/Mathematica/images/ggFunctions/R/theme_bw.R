#' theme_bw create a Black&White theme for ggplot2 plot
#'
#' @param base_size [integer]: Font size.
#' @param base_family [character]: Font family.
#' @param noGrid [logical]: A grid in the background of the plot should be drawn.
#' @return A ggplot2 theme object.
#' @export
# @examples
# p <- ggplot(mtcars, aes(x = wt, y=mpg), . ~ cyl) + geom_point()
# p <- p + theme_bw()

theme_bw <- function (base_size = 12, base_family = "", noGrid = FALSE) {
    if (noGrid) {
        noGridColour <- c("transparent", "transparent") # "white"
    } else {
        noGridColour <- c("grey90", "grey98")
    }
    theme_grey(base_size = base_size, base_family = base_family) %+replace%
        theme(
            axis.text = element_text(size = rel(0.8)),
            axis.ticks = element_line(colour = "black"),
            legend.background = element_rect(fill = "white", colour = "black"),
            legend.key = element_rect(fill = "white", colour = "black"),
            legend.position = "right",
            legend.justification = "center",
            legend.box = NULL,
            panel.background = element_rect(fill = "white", colour = NA),
            panel.border = element_rect(fill = NA, colour = "black"),
            panel.grid.major = element_line(colour = noGridColour[1], size = 0.2),
            panel.grid.minor = element_line(colour = noGridColour[2], size = 0.5),
            strip.background = element_rect(fill = "grey80", colour = "black", size = 0.2)
        )
}