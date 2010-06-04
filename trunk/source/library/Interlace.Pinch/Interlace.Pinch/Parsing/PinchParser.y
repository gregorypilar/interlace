%{
%}

%union { 
    public string identifier; 
    public int integer; 
    public Document document;
    public NamespaceName namespaceName; 
    public Versioning versioning; 
    public Protocol protocol;
    public Declaration declaration;
    public ProtocolIdentifier protocolIdentifier;
    public Enumeration enumeration;
    public Structure structure;
    public EnumerationMember enumerationMember;
    public StructureMember structureMember;
    }

%namespace Talcasoft.Pinch.Parsing
%start document

%token PROTOCOL ENUMERATION MESSAGE STRUCTURE CHOICE
%token REMOVED
%token IDENTIFIER INTEGER 
%token LBRACE RBRACE LBRACKET RBRACKET LPAREN RPAREN LPOINTY RPOINTY
%token DOT COMMA SEMICOLON
%token REQUIRED OPTIONAL

%type <identifier> identifier 
%type <integer> integer	field_modifier structure_kind
%type <document> document document_element_list
%type <namespaceName> namespace namespace_list field_type
%type <versioning> version_attributes
%type <protocol> document_element protocol declaration_list 
%type <declaration> declaration enumeration structure 
%type <protocolIdentifier> protocol_identifier 
%type <enumeration> enumeration_member_list 
%type <enumerationMember> enumeration_member 
%type <structure> structure_member_list 
%type <structureMember> structure_member 

%using Talcasoft.Pinch.Dom

%%

document			    : document_element_list    						{ _result = $1; }
					    ;

document_element_list   : document_element_list document_element        { $1.Protocols.Add($2); }
                        | document_element                              { $$ = new Document(); $$.Protocols.Add($1); }
                        ;

document_element        : protocol                                      { $$ = $1; }
                        ;

protocol                : PROTOCOL namespace version_attributes LBRACE declaration_list RBRACE
                                                                        { $$ = $5; $$.Name = $2; $$.Versioning = $3; }
                        | PROTOCOL namespace LPAREN protocol_identifier RPAREN version_attributes LBRACE declaration_list RBRACE
                                                                        { $$ = $8; $$.Name = $2; $$.ProtocolIdentifier = $4; $$.Versioning = $6; }
                        ;
                        
protocol_identifier     : protocol_identifier DOT integer               { $$ = new ProtocolIdentifier($1, $3); }
                        | integer                                       { $$ = new ProtocolIdentifier($1); }
                        ;
                        
version_attributes      : LBRACKET integer RBRACKET                         { $$ = new Versioning($2); }
                        | LBRACKET integer COMMA REMOVED integer RBRACKET   { $$ = new Versioning($2, $5); }
                        ;
                        
namespace               : namespace_list                                { $$ = $1; }
                        ;

namespace_list		    : namespace_list DOT identifier				    { $$ = new NamespaceName($1, $3); }
					    | identifier									{ $$ = new NamespaceName($1); }	
					    ;
                        
declaration_list        : declaration_list declaration                  { $$ = $1; $$.Declarations.Add($2); }
                        | declaration                                   { $$ = new Protocol(); $$.Declarations.Add($1); }
                        ;
                        
declaration             : enumeration                                   { $$ = $1; }
                        | structure                                     { $$ = $1; }
                        ;
                        
enumeration             : ENUMERATION identifier version_attributes LBRACE enumeration_member_list RBRACE
                                                                        { $$ = $5; $5.Identifier = $2; $5.Versioning = $3; }
                        ;
                        
enumeration_member_list : enumeration_member_list enumeration_member           { $$ = $1; $1.Members.Add($2); }
                        | enumeration_member                                   { $$ = new Enumeration(); $$.Members.Add($1); }
                        ;
                        
enumeration_member             : identifier version_attributes SEMICOLON       { $$ = new EnumerationMember($1, $2); }
                        ;
                        
structure               : structure_kind identifier version_attributes LBRACE structure_member_list RBRACE
                                                                        { $$ = $5; $5.Identifier = $2; $5.Versioning = $3; $5.StructureKind = (StructureKind)$1; }
                        ;
                        
structure_kind          : MESSAGE                                       { $$ = (int)StructureKind.Message; }
                        | STRUCTURE                                     { $$ = (int)StructureKind.Structure; }
                        | CHOICE                                        { $$ = (int)StructureKind.Choice; }
                        ;
                        
structure_member_list   : structure_member_list structure_member        { $$ = $1; $1.Members.Add($2); }
                        | structure_member                              { $$ = new Structure(); $$.Members.Add($1); }
                        ;
                        
structure_member        : field_modifier field_type identifier version_attributes SEMICOLON
                                                                        { $$ = new StructureMember((FieldModifier)$1, $2, $3, $4); }
                        | field_type identifier version_attributes SEMICOLON
                                                                        { $$ = new StructureMember((FieldModifier)0, $1, $2, $3); }
                        | field_type LPOINTY field_modifier field_type RPOINTY identifier version_attributes SEMICOLON
                                                                        { $$ = new StructureMember((FieldModifier)$3, $4, $6, $7, $1); }
                        ;
                        
field_type              : namespace                                     { $$ = $1; }
                        ;
                        
identifier              : IDENTIFIER                                    { $$ = $<identifier>1; }
                        ;
                        
integer                 : INTEGER                                       { $$ = $<integer>1; }
                        ;

field_modifier          : REQUIRED                                      { $$ = 1; }
                        | OPTIONAL                                      { $$ = 2; }
                        ;
                        
%%

Document _result = null;

public Document Result
{
	get { return _result; }
}
