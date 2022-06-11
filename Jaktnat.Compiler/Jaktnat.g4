grammar Jaktnat;

file: syntax* EOF;

syntax: function;

function: FUNCTION NAME LPAREN parameter* RPAREN block;

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
    | prefixUnaryOp=unaryOperator expression
    | expression postfixUnaryOp=unaryOperator
    | expression binaryOp=binaryOperator expression
    | expression assignment=EQUAL expression
    | call
    | parenthesizedExpression 
    | indexerAccess;

primaryExpr:
    operand
    | typeName
    | primaryExpr call;

expressionList: expression (COMMA expression)*;

parenthesizedExpression: LPAREN expression RPAREN;

indexerAccess: operand LBRACKET expression RBRACKET;

literal: number | STRING | TRUE | FALSE | arrayLiteral;

arrayLiteral: LBRACKET expressionList RBRACKET;

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

binaryOperator: GTE | GT | LTE | LT | MINUS | PLUS | ASTERISK | DIVIDE | MODULO | ASBANG | AS | DOT;

unaryOperator: PLUSPLUS | MINUSMINUS;

NUMBER_SUFFIX: ('f32' | 'f64' | 'u8' | 'u16' | 'u32' | 'u64' | 'uz' | 'i8' | 'i16' | 'i32' | 'i64');
ASBANG: 'as!';
AS: 'as';
IF: 'if';
LET: 'let';
FUNCTION: 'function';
THIS: 'this';
WHILE: 'while';
ANONYMOUS: 'anonymous';
MUTABLE: 'mutable';
TRUE: 'true';
FALSE: 'false';
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
EQUAL: '=';
COLON: ':';
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