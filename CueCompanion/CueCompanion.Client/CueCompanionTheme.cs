using MudBlazor;

namespace CueCompanion.Client;

public static class CueCompanionThemes
{
    public static MudTheme CueCompanionTheme = new()
    {
        //PaletteDark = new PaletteDark()
        //{
        //    Black = "rgba(39,39,47,1)",
        //    White = "rgba(255,255,255,1)",
        //    Primary = "rgba(119,107,231,1)",
        //    PrimaryContrastText = "rgba(255,255,255,1)",
        //    Secondary = "rgba(255,64,129,1)",
        //    SecondaryContrastText = "rgba(255,255,255,1)",
        //    Tertiary = "rgba(30,200,165,1)",
        //    TertiaryContrastText = "rgba(255,255,255,1)",
        //    Info = "rgba(50,153,255,1)",
        //    InfoContrastText = "rgba(255,255,255,1)",
        //    Success = "rgba(11,186,131,1)",
        //    SuccessContrastText = "rgba(255,255,255,1)",
        //    Warning = " rgba(255,168,0,1)",
        //    WarningContrastText = "rgba(255,255,255,1)",
        //    Error = "rgba(246,78,98,1)",
        //    ErrorContrastText = "rgba(255,255,255,1)",
        //    Dark = "rgba(39,39,47,1)",
        //    DarkContrastText = "rgba(255,255,255,1)",
        //    TextPrimary = "rgba(255,255,255,0.698)",
        //    TextSecondary = "rgba(255,255,255,0.698)",
        //    TextDisabled = "rgba(255,255,255,0.2)",
        //    ActionDefault = "rgba(173,173,177,1)",
        //    ActionDisabled = " rgba(255,255,255,0.258)",
        //    ActionDisabledBackground = " rgba(255,255,255,0.117)",
        //    Background = "rgba(50,51,61,1)",
        //    BackgroundGray = "rgba(39,39,47,1)",
        //    Surface = "rgba(55,55,64,1)",
        //    DrawerBackground = "rgba(39,39,47,1)",
        //    DrawerText = "rgba(255,255,255,0.498)",
        //    DrawerIcon = "rgba(255,255,255,0.498)",
        //    AppbarBackground = "rgba(39,39,47,1)",
        //    AppbarText = "rgba(255,255,255,0.698)",
        //    LinesDefault = "rgba(255,255,255,0.117)",
        //    LinesInputs = "rgba(255,255,255,0.298)",
        //    TableLines = "rgba(255,255,255,0.117)",
        //    TableStriped = "rgba(255,255,255,0.2)",
        //    TableHover = "rgba(0,0,0,0.039)",
        //    
        //},
        PaletteDark = new PaletteDark
        {
            Black = "rgba(39,39,47,1)",
            White = "rgba(255,255,255,1)",
            Primary = "#FF5440",
            PrimaryContrastText = "rgba(255,255,255,1)",
            Secondary = "rgba(255,64,129,1)",
            SecondaryContrastText = "rgba(255,255,255,1)",
            Tertiary = "rgba(30,200,165,1)",
            TertiaryContrastText = "rgba(255,255,255,1)",
            Info = "rgba(50,153,255,1)",
            InfoContrastText = "rgba(255,255,255,1)",
            Success = "rgba(11,186,131,1)",
            SuccessContrastText = "rgba(255,255,255,1)",
            Warning = "rgba(255,168,0,1)",
            WarningContrastText = "rgba(255,255,255,1)",
            Error = "rgba(246,78,98,1)",
            ErrorContrastText = "rgba(255,255,255,1)",
            Dark = "rgba(39,39,47,1)",
            DarkContrastText = "rgba(255,255,255,1)",
            TextPrimary = "rgba(255,255,255,0.6980392156862745)",
            TextSecondary = "rgba(255,255,255,0.4980392156862745)",
            TextDisabled = "rgba(255,255,255,0.2)",
            ActionDefault = "rgba(173,173,177,1)",
            ActionDisabled = "rgba(255,255,255,0.25882352941176473)",
            ActionDisabledBackground = "rgba(255,255,255,0.11764705882352941)",
            Background = "#151515",
            BackgroundGray = "rgba(39,39,47,1)",
            Surface = "rgba(55,55,64,1)",
            DrawerBackground = "rgba(25,25,28,1)",
            DrawerText = "rgba(255,255,255,0.4980392156862745)",
            DrawerIcon = "rgba(255,255,255,0.4980392156862745)",
            AppbarBackground = "rgba(25,25,28,1)",
            AppbarText = "rgba(255,255,255,0.6980392156862745)",
            LinesDefault = "rgba(255,255,255,0.11764705882352941)",
            LinesInputs = "rgba(255,255,255,0.2980392156862745)",
            TableLines = "rgba(255,255,255,0.11764705882352941)",
            TableStriped = "rgba(255,255,255,0.2)",
            TableHover = "rgba(0,0,0,0.0392156862745098)",
            Divider = "rgba(255,255,255,0.11764705882352941)",
            DividerLight = "rgba(255,255,255,0.058823529411764705)",
            Skeleton = "rgba(255,255,255,0.10980392156862745)",
            PrimaryDarken = "rgb(158,13,13)",
            PrimaryLighten = "rgb(235,42,42)",
            SecondaryDarken = "rgb(255,31,105)",
            SecondaryLighten = "rgb(255,102,153)",
            TertiaryDarken = "rgb(25,169,140)",
            TertiaryLighten = "rgb(42,223,187)",
            InfoDarken = "rgb(10,133,255)",
            InfoLighten = "rgb(92,173,255)",
            SuccessDarken = "rgb(9,154,108)",
            SuccessLighten = "rgb(13,222,156)",
            WarningDarken = "rgb(214,143,0)",
            WarningLighten = "rgb(255,182,36)",
            ErrorDarken = "rgb(244,47,70)",
            ErrorLighten = "rgb(248,119,134)",
            DarkDarken = "rgb(23,23,28)",
            DarkLighten = "rgb(56,56,67)",
            BorderOpacity = 1,
            HoverOpacity = 0.06,
            RippleOpacity = 0.1,
            RippleOpacitySecondary = 0.2,
            GrayDefault = "#9E9E9E",
            GrayLight = "#BDBDBD",
            GrayLighter = "#E0E0E0",
            GrayDark = "#757575",
            GrayDarker = "#616161",
            OverlayDark = "rgba(33,33,33,0.4980392156862745)",
            OverlayLight = "rgba(255,255,255,0.4980392156862745)"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "sans-serif"],
                FontSize = "16px",
                FontWeight = "400",
                LineHeight = "1.5"
            },
            H1 = new H1Typography
            {
                FontFamily = ["Inter", "Roboto", "sans-serif"],
                FontSize = "2rem",
                FontWeight = "400",
                LineHeight = "1.5"
            },
            H2 = new H2Typography
            {
                FontFamily = ["Inter", "Roboto", "sans-serif"],
                FontSize = "1.5rem",
                FontWeight = "400",
                LineHeight = "1.5"
            },
            H3 = new H3Typography
            {
                FontFamily = ["Inter", "Roboto", "sans-serif"],
                FontSize = "1.2rem",
                FontWeight = "400",
                LineHeight = "1.5"
            }
        }
    };
}