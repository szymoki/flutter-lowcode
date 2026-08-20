grammar RslBis;

start: usecase+ notiondef* EOF
    ;

usecase:
    'Use case' name mainscenario scenarios?
    ;

ucconditions:
    '{' conditions '}'
    ;

conditions:
    condition (';' conditions)?
    ;

condition:
    valuecondition | contextcondition
    ;

contextcondition:
    notion
    ;

valuecondition:
    ('user' | notion) '?' ('-')? value
    ;

mainscenario:
    'Main scenario' ucconditions? sentences endsentence
    ;

scenarios:
    scenario scenarios?
    ;

scenario:
    'Scenario' repsentence altsentences endsentence
    ;

sentences:
    sentence sentences?
    ;
	
altsentences:
    altsentence altsentences?
    ;

sentence:
    svosentence
    | condsentence
    ;

altsentence:
    altsvosentence
    | condsentence
    ;

condsentence:
    '[' conditions ']'
    ;

endsentence:
    '->' (resultsentence | rejoinsentence)
    ;

resultsentence:
    'end' '!' value
    ;

rejoinsentence:
    'rejoin' labelref
    ;

result:
    STRING
    ;

repsentence:
    (label | altlabel) '-"-'
    ;

svosentence:
    label step
    ;

altsvosentence:
    altlabel step
    ;

step:
    userstep
    | systemstep
    ;

systemstep:
    'System' (tosystempredicate | toactorpredicate | sysinvoke)
    ;

toactorpredicate:
    showpredicate
    | closepredicate
    ;

showpredicate:
    '<show>' notion
    ;

closepredicate:
     '<close>' notion
    ;

tosystempredicate:
    readpredicate
    | updatepredicate
    | deletepredicate
    | checkpredicate
    | executepredicate
    ;

readpredicate:
    '<read>' notion
    ;

updatepredicate:
    '<update>' notion
    ;

deletepredicate:
    '<delete>' notion
    ;

checkpredicate:
    '<check>' notion
    ;

executepredicate:
    '<execute>' notion
    ;

userstep:
    actor (selectpredicate | enterpredicate | invoke)
    ;

selectpredicate:
    '<select>' notion
    ;

enterpredicate:
    '<enter>' notion
    ;

invoke:
    '<invoke>' names
    ;

sysinvoke:
    '<invoke>' name
    ;

notiondef:
    viewnotion | triggernotion | datanotion
    ;

viewnotion: 
    'View' '*' viewtype '*' namesandlabels
    ;

viewtype:
    STRING
    ;

triggernotion: 
    'Trigger' ('*' triggertype '*')? namesandlabels
    ;

triggertype:
    STRING
    ;

namesandlabels:
    name ('{' uilabel '}')? (',' namesandlabels)?
    ;

datanotion: 
    'Data' name '{' attributes '}'
    ;

attributes:
    attribute (',' attributes)?
    ;

attribute:
    name ':' (datatype | notion | refnotion | multnotion)
    ;

refnotion:
    '->' notion
    ;

multnotion:
    '[' notion ']'
    ;

datatype:
    'integer' | 'float' | 'text' | 'boolean' | 'time' | 'date' | 'location' | 'file' | 'image'
    ;

names:
    name (',' names)?
    ;

actor:
    STRING
    ;

notion:
    (STRING | CHAR)+
    ;

value:
    (STRING | CHAR)+
    ;

name:
    (STRING | CHAR)+
    ;

uilabel:
    (STRING | CHAR)+
    ;

label:
    NUMBER ':'
    ;
    
altlabel:
    CHAR NUMBER ':'
    ;

labelref:
    CHAR? NUMBER
    ;

NUMBER:
    [0-9]+
    ;

CHAR:
    [a-zA-Z]
    ;

STRING:
    CHAR+
    ;

WS:
    [ \t\r\n]+ -> skip
    ;
