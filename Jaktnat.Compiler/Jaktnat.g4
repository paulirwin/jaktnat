grammar Jaktnat;

file: syntax* EOF;

syntax: function;

function: FUNCTION NAME LPAREN parameterList? RPAREN block;

parameterList: parameter (COMMA parameter)*;

parameter: ANONYMOUS? (thisParameter | namedParameter);

thisParameter: MUTABLE? THIS;

namedParameter: NAME COLON MUTABLE? type;

block: LCURLY statement* RCURLY;

statement: 
    block 
    | expression 
    | ifStatement 
    | letStatement 
    | whileStatement;

ifStatement: IF expression block;

whileStatement: WHILE expression block;

letStatement: LET variableDeclaration EQUAL expression;

variableDeclaration: MUTABLE? NAME variableDeclarationType?;

variableDeclarationType: COLON type;

expression: 
    primaryExpr
    | prefixUnaryOperator expression
    | expression postfixUnaryOperator
    | expression typeCastOperator type
    | expression IS type
    | expression call
    | expression memberAccess
    | expression indexerAccess
    | expression binaryOperator expression
    | parenthesizedExpression;

primaryExpr:
    operand
    | typeName
    | array;

expressionList: expression (COMMA expression)*;

parenthesizedExpression: LPAREN expression RPAREN;

memberAccess: DOT identifier;

indexerAccess: LBRACKET expression RBRACKET;

literal: number | STRING | TRUE | FALSE;

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

binaryOperator: 
    MINUS 
    | PLUS 
    | ASTERISK 
    | DIVIDE 
    | MODULO 
    | EQUALEQUAL 
    | NOTEQUAL 
    | EQUAL 
    | AND 
    | OR
    | AMPERSAND
    | PIPE
    | CARET
    | DOUBLELEFT
    | TRIPLELEFT
    | DOUBLERIGHT
    | TRIPLERIGHT
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
    | DOUBLEQUESTION
    | DOUBLEQUESTIONEQUAL
    | GTE 
    | GT 
    | LTE 
    | LT;

typeCastOperator: ASBANG | ASQUESTION; 

prefixUnaryOperator: 
    PLUSPLUS 
    | MINUSMINUS
    | MINUS
    | ASTERISK
    | AMPERSAND
    | NOT
    | TILDE;

postfixUnaryOperator:
    PLUSPLUS
    | MINUSMINUS;

NUMBER_SUFFIX: ('f32' | 'f64' | 'u8' | 'u16' | 'u32' | 'u64' | 'uz' | 'i8' | 'i16' | 'i32' | 'i64');
ASBANG: 'as!';
ASQUESTION: 'as?';
IS: 'is';
IF: 'if';
LET: 'let';
FUNCTION: 'function';
THIS: 'this';
WHILE: 'while';
ANONYMOUS: 'anonymous';
MUTABLE: 'mutable';
TRUE: 'true';
FALSE: 'false';
AND: 'and';
OR: 'or';
NOT: 'not';
FLOATING: MINUS? NUMBER_DIGIT+ '.' NUMBER_DIGIT+;
INTEGER: MINUS? NUMBER_DIGIT+;

NAME: (LOWER | UPPER | UNDERSCORE) (LOWER | UPPER | NUMBER_DIGIT)*;

fragment LOWER: 'a'..'z';
fragment UPPER: 'A'..'Z';
fragment UNDERSCORE: '_';

STRING: '"' ( ~'"' | '\\' '"' )* '"';

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
//EOL: '\n';

fragment NUMBER_DIGIT: [0-9_];

LINE_COMMENT: '//' ~[\r\n]* -> skip;

WHITESPACE: [ \n\r\t]+ -> channel(HIDDEN);

GARBAGE: . ;