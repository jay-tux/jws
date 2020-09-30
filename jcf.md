# Reading the Config File
### A short JCF overview

## Structure
A JCF file can contain up to four different structures:
 - Key-value pairs: the most easy, all values and all keys are strings, no need to escape them:  
 ```
 somekey: somevalue
 ```  
 As you can see, the key and value are separated by a colon (`:`). Only the first colon is used as a separator, the others will be used in the value:  
 ```
 key: something: value
 ```  
 will result in the key being `key` and the value being `something: value`.  
 A value ends on the end of a line, other leading and trailing whitespace is ignored.  
 - Single-line comments, are actually key-value pairs without a key (with a blank key):
 ```
 : some comment
 ```  
 *Note: JWS ignores comments during parsing (to limit RAM usage), so when exiting (and thus overwriting the config file), JWS removes all comments.*  
 - Blocks: a way of structuring the JCF file, a block is surrounded by curly braces (`{}`) and can contain key-value pairs, other blocks, lists and comments. Blocks are always values to a corresponding key:
 ```
 somekey: {
     someinnerkey: someinnervalue
     someinnerblock: {
         someinnerinnerkey: someinnerinnervalue
     }
 }
 ```  
 The newline before the closing curly brace (`}`) is mandatory, otherwise the last entry in the block is ignored. This is not the case for empty blocks.  
 - Lists: are simple lists that contain zero or more blocks and are surrounded by square brackets (`[]`):  
 ```
 somelist: [
    {
        block1: value1
    }
    {
        block2: value2
    }
    {
        block3: value3
    }
 ]
 ```  
 Lists can only contain blocks, no loose key-value pairs.  

## 'Routing' in a JCF File
JCF uses dot-routing, meaning that every dot in a route means we step into a block: `JCF.Server.Name` means we take the root node (`JCF`), search in that node for a node called `Server`, and search that node (the `JCF.Server` node) for a key called `Name`.  
This approach does have some drawbacks:  
 - You can't refer to single list elements,  
 - You can't have a dot (`.`) in your keys.

## Variables in JCF
One can reference the value of another key (possibly in another block, but not in a list) by surrounding the 'route' to that key in dollar signs (`$`). This is done bottom-to-top (meaning that first this block's keys are checked, then the parent's keys, then the parent's parent's key and so on).  
Supposing we have a JCF file like this:
```
Root: {
    Child: {
        Grandchild: {
            Great-grandchild: {
                Name: John
                Favorite_relative: [[]]
            }
            Name: Jane
        }
        Other-grandchild: {
            Name: Hans
        }
        Name: Julie
    }
    Other-child: {
        Grandchild: {
            Name: Jeff
        }
        Name: George
    }
    Name: Diana
}
```
Now, suppose we want to replace John's (`Root.Child.Grandchild.Great-grandchild`) `Favorite_relative` key with the name of his favorite relative, we could just hardcode it, but we could also just reference the relative:
Preferred relative | Absolute path | Relative path | Value | Route the parser took  
--- | --- | --- | --- | ---  
Diana | `Root` | `Root` | `$Root.Name$` | John -> Jane -> Julie -> Diana  
Julie | `Root.Child` | `Child` | `$Child.Name$` | John -> Jane -> Julie  
Jane | `Root.Child.Grandchild` | `Grandchild` | `$Grandchild.Name$` | John -> Jane  
John (Himself, the narcissist) | `Root.Child.Grandchild.Great-grandchild` | none | `$Name$` | John  
Hans | `Root.Child.Other-grandchild` | `Other-grandchild` | `$Other-grandchild.Name$` | John -> Jane -> Julie -> Hans  
Jeff | `Root.Other-child.Grandchild` | `Other-child.Grandchild` | `$Other-child.Grandchild.Name$` | John -> Jane -> Julie -> Diana -> George -> Jeff  
George | `Root.Other-child` | `Other-child` | `$Other-child.Name$` | John -> Jane -> Julie -> Diana -> George  
