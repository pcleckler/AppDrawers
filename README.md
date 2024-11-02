# AppDrawers
A Windows 10 &amp; 11 Utility for displaying shortcuts and capturing clipboard snippets.

When activated, AppDrawers displays a menu listing the contents of a directory containing Windows shortcuts `(*.lnk)` or Clip files `(*.Clip)`. The menu appears wherever the mouse cursor currently resides. When shortcuts to AppDrawer-configured directories are pinned to the Taskbar, the menu appears above the Taskbar icon.

# Installation

The application requires manual copying to a directory for use. There is no install/uninstall process currently defined. In service mode (the default mode when executing the application), a log file is written to the directory the application is configured to start up in. The application will need to be able to create files in that folder.

# Shortcut Display

Shortcuts have to be manually created to request directory structures be displayed.

For example, if a user would like to see their desktop shortcuts as a menu on the Taskbar, then a shortcut would need to be created in a folder (such as Documents\AppDrawers). The shortcut should be configured as follows:

Target

    "<Install folder>\AppDrawers.exe" "http://localhost:9393/AppDrawers/directory?dir=C:\Users\<userId>\Desktop"

Run

    Minimized

Where `Install folder` is the installation folder selected for `AppDrawers.exe`.

The shortcut's icon should be personalized, otherwise a generic icon will be displayed.

Finally, the shortcut should be right-clicked, and the menu option "Pin to Taskbar" selected. For Windows 11, this may be in a submenu.

# Clipping

AppDrawers supports capturing Clipboard snippets. It saves these as text files with the extension "Clip" in the directory chosen for the Taskbar shortcut. To activate clipping for a directory, `clipping=true` should be added to the shortcut's URL.

For example, a storage folder would need to be created (such as Documents\AppDrawers\Clips), and a shortcut would need to be created in a folder (such as Documents\AppDrawers). The shortcut should be configured as follows:

Target

    "<Install folder>\AppDrawers.exe" "http://localhost:9393/AppDrawers/directory?dir=C:\Users\<userId>\Documents\AppDrawers\Clips&clipping=true"

Run

    Minimized

Again, where `Install folder` is the installation folder selected for `AppDrawers.exe`.

Also again, the shortcut's icon should be personalized, otherwise a generic icon will be displayed.

The shortcut should be right-clicked and the "Pin to Taskbar" option selected.

> Note that when the `clipping` option is used, the `New Shortcut` option is replaced with a `Capture Clip` option.

# Troubleshooting

If the application launches, but produces an `Access Denied` error, the default application port may already be in use. Ensure there are no other applications listening on port 9393. Alternatively, the `/port <port>` command line option can be supplied to the shortcut that starts the AppDrawers service, and an alternate port chosen.