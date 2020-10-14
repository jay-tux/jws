# JWS - Templates
Templates are used for quickly defining multiple web pages using a same structure.  

## Enabling Templates
Template settings reside in the `JWS.Templates` block of your JWS configuration file. To enable templating, set the `JWS.Templates.State` variable to `on`.
With this setting, the JWS system will recognize `*.jtmp` files as templates and `*.jtfl` files as template data. This also removes the client's ability to directly access the template file.  
Another important setting is `JWS.Paths.Template`, it defines the path where JWS will look for template files, by default, it is defined as `@Home/.config/jws/html/template` (as the `template` subdirectory of the `html` directory).

### Advanced Template Configuration
In the `JWS.Templates` block, there are five different settings:  
 - `JWS.Templates.State`: `on` if you want to enable templating, otherwise `off`.  
 - `JWS.Templates.Assoc`: the MIME type associated with templates. These files can't be accessed directly, and will yield an HTTP 403 (Forbidden) response when accessed.  
 - `JWS.Templates.Data`: the MIME type associated with template data. When accessed, these files will yield their associated template, but with the variable fields filled with the data defined in this file.  
 - `JWS.Templates.Prefix`: this is the prefix used to determine which parts of the template files need to be replaced by template data. Do not set this to `$` as it will conflict with the JCF variable parser. The default is `~`, which is the prefix we'll use in this file.  
 - `JWS.Templates.Indexing`: Used when working with GET and POST variables. A GET variable with key `key` can be accessed from the template using `[Prefix]GET[Indexing]key` (analogous for POST). The default is `::`, which is the indexing operator we'll use in this file. Setting the Indexing operator and Prefix to the same value is undefined behavior.  

 As template files are associated with MIME types, they have to be set in `JWS.Server.FileAssoc`. For readability purposes, we recommend setting the using the `$JWS.Templates.Assoc$` (for template files) and `$JWS.Templates.Data$` (for data files) references as values. This way, changing the MIME types associated with templates doesn't break anything.  

## Creating a Template HTML
A template HTML file is much like a normal HTML file, except:  
 - It has the `*.jtmp` file extension (or any other extension you configured), and  
 - It has some fields to be filled.  

A field is defined using the variable prefix `~`. Any defined variable in the data file will replace the corresponding field in the template. For example: if I were to define a variable called `title` in my data file, then I can access that file using `~title` in my template. Whenever the data file is accessed, its template is loaded, and each occurrence of `~title` is replaced with the value of `title`.  

### Pre-defined variables
JWS gives some pre-defined variables as well:  
 - `~server`, which returns the server's name (as set in the config file), and  
 - `~page`, which returns the path of the page accessed (this is the web path, not the physical path).  

### GET and POST
You can access GET or POST variables set by the client:  
 - Use `~GET::<key>` to get the value corresponding to `<key>` from the GET variables,  
 - Use `~POST::<key>` to get the value corresponding to `<key>` from the POST variables,  
 - Use `~AllGET` to get all GET variables as `<key> = <value>`, each on a separate line (separated using `<br />\n`),  
 - Use `~AllPOST` to get all POST variables as `<key> = <value>`, each on a separate line (analogous to `~AllGET`).  

### Nonexistent variables
When you try to access a non-defined or nonexistent variable, JWS will not replace the reference (say you referenced a variable `~somevar`, but your data file doesn't define `somevar`, then the webpage will show `~somevar`. It won't be replaced by a blank or anything else; you won't get warnings either).

## Injecting into a Template
Injecting data into a template is done by data files (`*.jtfl` files). A data file defines key-value pairs, one per line (that means no multi-line values) and separated by `=`. See the example below:  
```
template = Simple.jtmp
title = Templated Page
copyright = (c) Jay 2020 (part of the JWS examples)
```  
There is one mandatory field in the data file: `template`, it defines which template file to use (relative to the directory defined in `JWS.Paths.Template`). This is the only variable that's modified when used in the template: the template name's file extension is removed. However, if your file name contains dots (`.`), only the part before the first dot (`.`) is kept.  

## Example of Template and Injection
Suppose we have a template file called `Simple.jtmp` in the Template directory (which is a subdirectory of the HTML directory, as per the default settings), containing the following text:  
```html
<!DOCTYPE html>
<html>
        <head>
                <meta charset=utf-8 />
                <title>~server - ~title</title>
                <link href="/css/tmpl.css" rel=stylesheet type=text/css />
        </head>
        <body>
                <div>
                        <h1>~title on ~server</h1>
                        <p>Simple template for ~server, running on JWS.</p>
                        <p>You're currently on the page ~title.</p>
                        <p>Some links:</p>
                        <ul>
                                <li><a href="/">Home page</a></li>
                                <li><a href="~page">This page</a></li>
                        </ul>
                        <div class=footer>
                                <p class=footer>~copyright</p>
                        </div>
                </div>
        </body>
</html>
```  
Suppose we have another file, the data file, `data.jtfl`, in the HTML directory, containing the following text:  
```
template = Simple.jtmp
title = Templated Page
copyright = (c) Jay 2020 (part of the JWS examples)
```  
Then, accessing `/template/Simple.jtmp` would give an HTTP 403 (Forbidden) response and accessing `/data.jtfl` would give an HTTP 200 (OK) response with the following content:  
```html
<!DOCTYPE html>
<html>
        <head>
                <meta charset=utf-8 />
                <title>_jws_example - Templated Page</title>
                <link href="/css/tmpl.css" rel=stylesheet type=text/css />
        </head>
        <body>
                <div>
                        <h1>Templated Page on _jws_example</h1>
                        <p>Simple template for _jws_example, running on JWS.</p>
                        <p>You're currently on the page Templated Page.</p>
                        <p>Some links:</p>
                        <ul>
                                <li><a href="/">Home page</a></li>
                                <li><a href="/data.jtfl">This page</a></li>
                        </ul>
                        <div class=footer>
                                <p class=footer>(c) Jay 2020 (part of the JWS examples)</p>
                        </div>
                </div>
        </body>
</html>
```
