# Paprika.Net

## What is Paprika?

Paprika is a language, and software, for generating semi-random arrangements of words - like computer-driven mad-libs. This can be useful for generating game dialogue, tweet bots, and flavour text which have **spice and variety**! Paprika (the software) takes a "request" and uses it to execute some "grammar" (grammars are the files which make up your random options). Better to show than to tell:

	# Here's a grammar file. Hashes denote comments (lines the software should not read)
	# Below is a "category", denoted with an asterisk:
	* animal
	cow
	horse
	mouse
	
	* nice
	lovely
	delightful
	amazing
	
If this grammar was loaded, the user could enter the following input to get the following example output:

	Check out this [animal]
	-> "Check out this cow"
	
	My [animal] is [nice]
	-> "My mouse is delightful"
	
If you run each command again, the output can be different, because the bits in `[square brackets]` are randomised.

The language has **many more features** than this, which you can explore at <http://paprika.me.uk>


## In this repository

`Paprika.Net` is the core functionality of the Paprika engine. It can load, parse, execute, and validate grammars and queries. It is well unit-tested in `Paprika.NetTests`. This project is available as a NuGet package as `SteGriff.Paprika.Net`.

`Paprika.Net.Console` is basically a desktop client for Paprika. It's a command line app which calls upon `Paprika.Net`, letting you play with and iterate your grammar files rapidly. 


## Using the Paprika.Net package (API)

**WIP**

Install the NuGet package using `Install-Package SteGriff.Paprika.Net`

 * Instantiate a `Core` object using `var engine = new SteGriff.Paprika.Net.Core()`;
 * Load a grammar with `engine...`
 * Parse a request with `engine.Parse("My query is [cool/nice]")`
 * You can select specific values you want to use (instead of random ones) using an overload of `engine.Parse...`

 
## Config for Paprika.Net.Console app

By default, the grammars are chosen from the `paprika-grammar` submodule (`../../paprika-grammar/`) but you can override this in `App.config` like so:

	<appSettings>
		<add key="GrammarRoot" value="C:/path/to/my/paprika-grammar/" />
	</appSettings>
	
The specified directory must contain an `index.grammar` file.

[pap]: http://github.com/stegriff/paprika


## Packaging for NuGet (note to self)

 * Build in Release Mode
 * Publish from VS, not Nuspec
 * Go to nuget.org Upload and upload the new package from the Build directory
 