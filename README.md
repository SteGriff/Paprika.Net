# Paprika.Net

This is a .Net port of [Paprika][pap]

## Config

By default, the grammars are chosen from the `paprika-grammar` submodule (`../../paprika-grammar/`) but you can override this in `App.config` like so:

	<appSettings>
		<add key="GrammarRoot" value="C:/path/to/my/paprika-grammar/" />
	</appSettings>
	
The specified directory must contain an `index.grammar` file.

[pap]: http://github.com/stegriff/paprika