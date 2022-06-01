grammar Jaktnat;

file: syntax* EOF;

syntax: function;

function: 'function' NAME LPAREN RPAREN block;

block: LCURLY statement* RCURLY;

statement: block | expression;

expression: operand | literal;

literal: number | STRING;

number: (FLOATING | INTEGER) numberSuffix?;

numberSuffix: ('f32' | 'f64' | 'u8' | 'u16' | 'u32' | 'u64' | 'uz' | 'i8' | 'i16' | 'i32' | 'i64');

operand: call;

call: NAME LPAREN callArgument* RPAREN;

callArgument: expression COMMA?;

FLOATING: MINUS? NUMBER_DIGIT+ '.' NUMBER_DIGIT+;
INTEGER: MINUS? NUMBER_DIGIT+;

NAME: [a-zA-Z_] [a-zA-Z_0-9]*;

STRING: '"' ( ~'"' | '\\' '"' )* '"';

LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
COMMA: ',';
//EOL: '\n';

fragment NUMBER_DIGIT: [0-9_];
fragment MINUS: '-';

LINE_COMMENT: '//' ~[\r\n]* -> skip;

WHITESPACE: [ \n\r\t]+ -> channel(HIDDEN);