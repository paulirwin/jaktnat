grammar Jaktnat;

file: syntax* EOF;

syntax: function;

function: 'function' NAME LPAREN RPAREN block;

block: LCURLY statement* RCURLY;

statement: block | expression;

expression: operand | literal;

literal: STRING;

operand: call;

call: NAME LPAREN callArgument* RPAREN;

callArgument: expression COMMA?;

NAME: [a-zA-Z_] [a-zA-Z_0-9]*;

STRING: '"' ( ~'"' | '\\' '"' )* '"';

LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
COMMA: ',';
//EOL: '\n';

LINE_COMMENT: '//' ~[\r\n]* -> skip;

WHITESPACE: [ \n\r\t]+ -> channel(HIDDEN);