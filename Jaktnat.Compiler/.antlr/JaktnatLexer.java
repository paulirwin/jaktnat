// Generated from c:\Users\PaulIrwin\git\Jaktnat\Jaktnat.Compiler\Jaktnat.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class JaktnatLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, NAME=2, STRING=3, LPAREN=4, RPAREN=5, LCURLY=6, RCURLY=7, COMMA=8, 
		LINE_COMMENT=9, WHITESPACE=10;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"T__0", "NAME", "STRING", "LPAREN", "RPAREN", "LCURLY", "RCURLY", "COMMA", 
			"LINE_COMMENT", "WHITESPACE"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'function'", null, null, "'('", "')'", "'{'", "'}'", "','"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, "NAME", "STRING", "LPAREN", "RPAREN", "LCURLY", "RCURLY", 
			"COMMA", "LINE_COMMENT", "WHITESPACE"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public JaktnatLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "Jaktnat.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\fN\b\1\4\2\t\2\4"+
		"\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t"+
		"\13\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\3\3\3\7\3#\n\3\f\3\16\3&\13"+
		"\3\3\4\3\4\3\4\3\4\7\4,\n\4\f\4\16\4/\13\4\3\4\3\4\3\5\3\5\3\6\3\6\3\7"+
		"\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\n\3\n\7\nA\n\n\f\n\16\nD\13\n\3\n\3\n\3"+
		"\13\6\13I\n\13\r\13\16\13J\3\13\3\13\2\2\f\3\3\5\4\7\5\t\6\13\7\r\b\17"+
		"\t\21\n\23\13\25\f\3\2\7\5\2C\\aac|\6\2\62;C\\aac|\3\2$$\4\2\f\f\17\17"+
		"\5\2\13\f\17\17\"\"\2R\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2"+
		"\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2\2\2\2\25"+
		"\3\2\2\2\3\27\3\2\2\2\5 \3\2\2\2\7\'\3\2\2\2\t\62\3\2\2\2\13\64\3\2\2"+
		"\2\r\66\3\2\2\2\178\3\2\2\2\21:\3\2\2\2\23<\3\2\2\2\25H\3\2\2\2\27\30"+
		"\7h\2\2\30\31\7w\2\2\31\32\7p\2\2\32\33\7e\2\2\33\34\7v\2\2\34\35\7k\2"+
		"\2\35\36\7q\2\2\36\37\7p\2\2\37\4\3\2\2\2 $\t\2\2\2!#\t\3\2\2\"!\3\2\2"+
		"\2#&\3\2\2\2$\"\3\2\2\2$%\3\2\2\2%\6\3\2\2\2&$\3\2\2\2\'-\7$\2\2(,\n\4"+
		"\2\2)*\7^\2\2*,\7$\2\2+(\3\2\2\2+)\3\2\2\2,/\3\2\2\2-+\3\2\2\2-.\3\2\2"+
		"\2.\60\3\2\2\2/-\3\2\2\2\60\61\7$\2\2\61\b\3\2\2\2\62\63\7*\2\2\63\n\3"+
		"\2\2\2\64\65\7+\2\2\65\f\3\2\2\2\66\67\7}\2\2\67\16\3\2\2\289\7\177\2"+
		"\29\20\3\2\2\2:;\7.\2\2;\22\3\2\2\2<=\7\61\2\2=>\7\61\2\2>B\3\2\2\2?A"+
		"\n\5\2\2@?\3\2\2\2AD\3\2\2\2B@\3\2\2\2BC\3\2\2\2CE\3\2\2\2DB\3\2\2\2E"+
		"F\b\n\2\2F\24\3\2\2\2GI\t\6\2\2HG\3\2\2\2IJ\3\2\2\2JH\3\2\2\2JK\3\2\2"+
		"\2KL\3\2\2\2LM\b\13\3\2M\26\3\2\2\2\b\2$+-BJ\4\b\2\2\2\3\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}