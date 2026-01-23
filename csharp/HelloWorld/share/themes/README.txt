A new theme can be added to the themes directory specified above by setting the environment variable GTK_THEME.

After much frustration, I figured out that Windows gtk3 wants themes in
(maybe other, but at least) the {executable directory}/share/themes/...
Linux wants them in the normal directories ~/.themes ~/.local/share/themes/

export GTK_DATA_PREFIX=./
makes linux act like windows, and will look in the {executable directory}/share/themes/...

The built in themes on windows and linux are : HighContrast, HighContrastInverted, win32, and Adwaita

So that I dont forget I am adding this comment.
