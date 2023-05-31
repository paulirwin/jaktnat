grammar Jaktnat;

file: declaration* EOF;

declaration: function
    | classDeclaration
    | structDeclaration;

function: visibilityModifier? FUNCTION NAME LPAREN parameterList? RPAREN 
    THROWS? functionReturnType? 
    (block | (FATARROW expression));

functionReturnType: ARROW type;

classDeclaration: CLASS NAME LCURLY classOrStructMember* RCURLY;

structDeclaration: STRUCT NAME LCURLY classOrStructMember* RCURLY;

classOrStructMember: (property | function) COMMA?;

property: visibilityModifier? NAME variableDeclarationType defaultArgument?;

visibilityModifier: PUBLIC | PRIVATE;

parameterList: parameter (COMMA parameter)*;

parameter: thisParameter | namedParameter;

thisParameter: MUT? THIS;

namedParameter: (ANON | ANONYMOUS)? MUT? NAME COLON type defaultArgument?;

defaultArgument: EQUAL expression;

block: LCURLY statement* RCURLY;

statement: 
    block 
    | matchStatement
    | expressionStatement
    | ifStatement 
    | tryStatement
    | throwStatement
    | letStatement 
    | mutStatement
    | whileStatement
    | loopStatement
    | breakStatement
    | continueStatement
    | returnStatement
    | deferStatement
    | unsafeBlock
    | csharpBlock
    | forInStatement
    | guardStatement;
    
forInStatement: FOR identifier IN expression block;
    
unsafeBlock: UNSAFE block;

csharpBlock: CSHARP block;

deferStatement: DEFER (expression | block);

expressionStatement: expression SEMICOLON?;

ifStatement: IF expression block elseStatement?;

guardStatement: GUARD expression elseStatement;

matchStatement: matchExpression;

tryStatement: TRY (expression | block) catchClause;

catchClause: CATCH identifier block;

elseStatement: ELSE (block | ifStatement);

whileStatement: WHILE expression block;

loopStatement: LOOP block;

breakStatement: BREAK SEMICOLON?;

continueStatement: CONTINUE SEMICOLON?;

returnStatement: RETURN expression? SEMICOLON?;

throwStatement: THROW expression? SEMICOLON?;

letStatement: LET variableDeclaration EQUAL expression SEMICOLON?;

mutStatement: MUT NAME variableDeclarationType? EQUAL expression SEMICOLON?;

variableDeclaration: MUT? NAME variableDeclarationType?;

variableDeclarationType: COLON type;

expression: 
    primaryExpr
    | expression typeCastOperator type
    | expression IS type
    | expression call
    | expression memberAccess
    | expression indexerAccess
    | expression binaryMultiplyDivideModulo expression  // parser.rs: precedence 100
    | expression binaryAddSubtract expression           // 90
    | expression binaryShift expression                 // 85
    | expression binaryBoolean expression               // 80
    | expression binaryBitwiseAnd expression            // 73
    | expression binaryBitwiseXor expression            // 72
    | expression binaryBitwiseOr expression             // 71
    | expression binaryLogicalAnd expression            // 70
    | expression binaryOrCoalescing expression          // 69
    | expression binaryAssign expression                // 50
    | expression postfixUnaryOperator
    | parenthesizedExpression;

primaryExpr:
    operand
    | thisExpression
    | prefixUnaryOperator expression
    | typeName
    | array
    | scopeAccess
    | memberAccess
    | matchExpression;
    
thisExpression: THIS;

expressionList: expression (COMMA expression)*;

parenthesizedExpression: LPAREN expression RPAREN;

memberAccess: DOT identifier;

scopeAccess: identifier DOUBLECOLON identifier;

indexerAccess: LBRACKET expression RBRACKET;

literal: 
    number 
    | BINARY_LITERAL
    | STRING 
    | CHARACTER 
    | TRUE 
    | FALSE;

array: LBRACKET expressionList RBRACKET;

number: (FLOATING | INTEGER) numberSuffix?;

numberSuffix: NUMBER_SUFFIX;

operand: literal | identifier;

identifier: NAME;

type: typeName | arrayType;

typeName: NAME | NUMBER_SUFFIX;

arrayType: LBRACKET typeName RBRACKET;

call: LPAREN callArgument* RPAREN;

callArgument: argumentName? expression COMMA?;

argumentName: NAME COLON;

binaryAssign: 
    EQUAL
    | PLUSEQUAL
    | MINUSEQUAL
    | ASTERISKEQUAL
    | DIVIDEEQUAL
    | MODULOEQUAL
    | AMPERSANDEQUAL
    | PIPEEQUAL
    | CARETEQUAL
    | DOUBLELEFTEQUAL
    | DOUBLERIGHTEQUAL
    | DOUBLEQUESTIONEQUAL;

binaryOrCoalescing:
    OR
    | DOUBLEQUESTION;

binaryLogicalAnd: AND;

binaryBitwiseOr: PIPE;

binaryBitwiseXor: CARET;

binaryBitwiseAnd: AMPERSAND;

binaryBoolean:
    GTE 
    | GT 
    | LTE 
    | LT
    | EQUALEQUAL 
    | NOTEQUAL;

binaryShift:
    DOUBLELEFT
    | TRIPLELEFT
    | DOUBLERIGHT
    | TRIPLERIGHT;

binaryAddSubtract:
    MINUS 
    | PLUS;

binaryMultiplyDivideModulo: 
    ASTERISK 
    | DIVIDE 
    | MODULO;

typeCastOperator: ASBANG | ASQUESTION; 

prefixUnaryOperator: 
    { JaktnatParserSupport.IsPrefixOp(_input) }?
    PLUSPLUS 
    | MINUSMINUS
    | MINUS
    | ASTERISK
    | AMPERSAND
    | NOT
    | TILDE;

postfixUnaryOperator:
    { JaktnatParserSupport.IsPostfixOp(_input) }?
    PLUSPLUS
    | MINUSMINUS;
    
matchExpression: MATCH expression LCURLY matchCase* RCURLY;

matchCase: matchCasePattern (PIPE matchCasePattern)* FATARROW matchCaseBody SEMICOLON?;

matchCaseBody: expression | block;

matchCasePattern: 
    matchCasePatternExpression
    | matchCasePatternElse;
    
matchCasePatternExpression: LPAREN expression RPAREN;

matchCasePatternElse: ELSE;

NUMBER_SUFFIX: ('f32' | 'f64' | 'u8' | 'u16' | 'u32' | 'u64' | 'uz' | 'i8' | 'i16' | 'i32' | 'i64');
ASBANG: 'as!';
ASQUESTION: 'as?';
IS: 'is';
IF: 'if';
ELSE: 'else';
DEFER: 'defer';
TRY: 'try';
CATCH: 'catch';
THROW: 'throw';
LET: 'let';
PUBLIC: 'public';
PRIVATE: 'private';
FUNCTION: 'fn';
CLASS: 'class';
STRUCT: 'struct';
THIS: 'this';
WHILE: 'while';
LOOP: 'loop';
BREAK: 'break';
CONTINUE: 'continue';
RETURN: 'return';
ANON: 'anon';
ANONYMOUS: 'anonymous';
MUT: 'mut';
TRUE: 'true';
FALSE: 'false';
AND: 'and';
OR: 'or';
NOT: 'not';
THROWS: 'throws';
UNSAFE: 'unsafe';
CSHARP: 'csharp';
FOR: 'for';
IN: 'in';
GUARD: 'guard';
MATCH: 'match';

FLOATING: MINUS? NUMBER_DIGIT+ '.' NUMBER_DIGIT+;
INTEGER: MINUS? NUMBER_DIGIT+;
BINARY_LITERAL: BINARY_LITERAL_PREFIX BINARY_DIGIT+;

NAME: (LOWER | UPPER | UNDERSCORE) (LOWER | UPPER | NUMBER_DIGIT)*;

fragment LOWER: 'a'..'z';
fragment UPPER: 'A'..'Z';
fragment UNDERSCORE: '_';

CHARACTER: 'c\'' ( ~'\'' | '\\' '\'') '\'';
STRING: '"' ( ~'"' | '\\' '"' )* '"';

DOUBLECOLON: '::';
ARROW: '->';
FATARROW: '=>';
LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
LBRACKET: '[';
RBRACKET: ']';
COMMA: ',';
EQUALEQUAL: '==';
NOTEQUAL: '!=';
EQUAL: '=';
COLON: ':';
TILDE: '~';
EXCLAMATION: '!';
DOUBLELEFTEQUAL: '<<=';
DOUBLERIGHTEQUAL: '>>=';
DOUBLEQUESTIONEQUAL: '??=';
DOUBLEQUESTION: '??';
TRIPLELEFT: '<<<';
DOUBLELEFT: '<<';
TRIPLERIGHT: '>>>';
DOUBLERIGHT: '>>';
PLUSEQUAL: '+=';
MINUSEQUAL: '-=';
ASTERISKEQUAL: '*=';
DIVIDEEQUAL: '/=';
MODULOEQUAL: '%=';
AMPERSANDEQUAL: '&=';
PIPEEQUAL: '|=';
CARETEQUAL: '^=';
AMPERSAND: '&';
PIPE: '|';
CARET: '^';
GT: '>';
GTE: '>=';
LT: '<';
LTE: '<=';
PLUSPLUS: '++';
MINUSMINUS: '--';
MINUS: '-';
PLUS: '+';
ASTERISK: '*';
DIVIDE: '/';
MODULO: '%';
DOT: '.';
SEMICOLON: ';';
//EOL: '\n';

fragment BINARY_LITERAL_PREFIX: '0b' | '0B';
fragment NUMBER_DIGIT: [0-9_];
fragment BINARY_DIGIT: [0-1_];

LINE_COMMENT: '//' ~[\r\n]* -> skip;

WHITESPACE: [ \n\r\t]+ -> channel(HIDDEN);

GARBAGE: . ;