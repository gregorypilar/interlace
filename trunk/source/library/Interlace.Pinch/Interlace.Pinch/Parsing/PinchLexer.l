%using System.Text;
%namespace Interlace.Pinch.Parsing

%%
protocol		    { return (int)Tokens.PROTOCOL; }
message			    { return (int)Tokens.MESSAGE; }
enumeration		    { return (int)Tokens.ENUMERATION; }
structure		    { return (int)Tokens.STRUCTURE; }
choice  		    { return (int)Tokens.CHOICE; }
removed			    { return (int)Tokens.REMOVED; }
required		    { return (int)Tokens.REQUIRED; }
optional		    { return (int)Tokens.OPTIONAL; }

[a-zA-Z_][a-zA-Z0-9_]*          { yylval.identifier = yytext; 
				                  return (int)Tokens.IDENTIFIER; }
[ \r\n\t]+          { }

[0-9]+			    { yylval.integer = int.Parse(yytext);
				      return (int)Tokens.INTEGER; }
				  
\{				{ return (int)Tokens.LBRACE; }
\}				{ return (int)Tokens.RBRACE; }
\[				{ return (int)Tokens.LBRACKET; }
\]				{ return (int)Tokens.RBRACKET; }
\(				{ return (int)Tokens.LPAREN; }
\)				{ return (int)Tokens.RPAREN; }
\<				{ return (int)Tokens.LPOINTY; }
\>				{ return (int)Tokens.RPOINTY; }

\.              { return (int)Tokens.DOT; }
,               { return (int)Tokens.COMMA; }
;               { return (int)Tokens.SEMICOLON; }

.				{ throw new FormatException("Syntax error"); }

                { yylloc = new LexLocation(tokLin, tokCol, tokELin, tokECol); }

%%

public override void yyerror(string message, params object[] arguments) 
{
	throw new ParsingException(message, yylloc);
}
