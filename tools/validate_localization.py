#!/usr/bin/env python3
"""Mewgic String Table kaynak doğrulaması; Unity veya ek paket gerektirmez."""
import argparse, copy, json, pathlib, re, sys
CODES=('tr','en','de','fr','es','pt-BR','it','pl')
ROOT=pathlib.Path(__file__).resolve().parents[1]
def validate(rows,references=()):
    errors=[];keys=set()
    for row in rows:
        key=row.get('key','')
        if not re.fullmatch(r'[a-z][a-z0-9_.]*',key):errors.append('invalid-key:'+key)
        if key in keys:errors.append('duplicate:'+key)
        keys.add(key);values=row.get('values',{});params=set(re.findall(r'\{(\d+)(?=[:}])',values.get('en','')))
        for code in CODES:
            value=values.get(code,'')
            if not value.strip():errors.append('missing:'+code+':'+key)
            if set(re.findall(r'\{(\d+)(?=[:}])',value))!=params:errors.append('parameters:'+code+':'+key)
    errors.extend('source:'+key for key in sorted(set(references)-keys))
    return errors

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--self-test',action='store_true');parser.add_argument('--output');args=parser.parse_args()
    rows=json.loads((ROOT/'Assets/Localization/Source/Mewgic.json').read_text(encoding='utf-8-sig'))
    refs=[]
    for path in (ROOT/'Assets/Scripts').rglob('*.cs'):
        if 'Tests' not in path.parts:refs+=re.findall(r'L\.V\("([a-z0-9_.]+)"',path.read_text(encoding='utf-8-sig'))
    errors=validate(rows,refs)
    if args.self_test:
        sample=[{'key':'sample.key','values':{c:'Value {0}' for c in CODES}}]
        assert not validate(sample)
        broken=copy.deepcopy(sample);del broken[0]['values']['fr'];assert 'missing:fr:sample.key' in validate(broken)
        broken=copy.deepcopy(sample);broken[0]['values']['pl']='Value {1}';assert 'parameters:pl:sample.key' in validate(broken)
        assert 'duplicate:sample.key' in validate(sample+sample)
        assert 'source:missing.key' in validate(sample,['missing.key'])
    result={'success':not errors,'keys':len(rows),'languages':list(CODES),'literalSourceReferences':len(refs),'errors':errors,'validatorNegativeCasesPassed':4 if args.self_test else None,'note':'Smart Strings dilbilgisi, glifler ve canlı yerleşim ayrıca Unity içinde doğrulanır.'}
    output=json.dumps(result,ensure_ascii=False,indent=2)
    if args.output:(ROOT/args.output).write_text(output,encoding='utf-8')
    if hasattr(sys.stdout,"reconfigure"):sys.stdout.reconfigure(encoding="utf-8")
    print(output);return int(bool(errors))
if __name__=='__main__':sys.exit(main())
