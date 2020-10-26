#JWS - Control Panel
JWS comes with a builtin client-side (browser) control panel. By default, this control panel is located under `/cpanel/`, and can be accessed with username `admin` and password `admin`. At the moment, there is no support for changing these usernames and/or passwords, except for editing the hashes file yourself (**not recommended**), therefore, the control panel is disabled by default and it's not recommended to enable it until this security flaw is fixed.  

## Associated Settings
The `JWS.ControlPanel` block in the config file is dedicated to control panel settings. These settings are:  
 - `JWS.ControlPanel.State`: toggle the control panel. *Until the security flaw is fixed, it is recommended to keep the control panel disabled.*  
 - `JWS.ControlPanel.URL`: the (relative) URL from where the control panel can be accessed (`/cpanel/` by default).  
 - `JWS.ControlPanel.WarningOnFailedLogin`: log a failed login for the control panel as a warning.  
 - `JWS.ControlPanel.LoginTitle`: the title to display on the login page.  
 - `JWS.ControlPanel.CPanelTitle`: the title to display on the control panel pages.  
 - `JWS.ControlPanel.Footer`: the footer to add to the control panel and login pages.  
 - `JWS.ControlPanel.CSS`: a block containing CSS references for the pages:  
   - `JWS.ControlPanel.CSS.Login`: CSS file for the login page (from the root of the server).  
   - `JWS.ControlPanel.CSS.Home`: CSS file for the control panel pages (from the root of the server).
